using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Manages the voxel world, including world generation, chunk management, and elevation-based rendering.
/// 
/// <see cref="World"/> is a singleton MonoBehaviour that holds world parameters, references to chunk and slice prefabs,
/// and orchestrates the world generation pipeline. It also manages visibility of world layers and responds to elevation changes.
/// </summary>
public class World : MonoBehaviour
{
    /// <summary>
    /// The mesh filter attached to the world object (optional).
    /// </summary>
    MeshFilter filter;

    // World configuration parameters
    /// <summary>Maximum X dimension of the world (in blocks).</summary>
    public int maxX = 16;
    /// <summary>Maximum Z dimension of the world (in blocks).</summary>
    public int maxZ = 16;
    /// <summary>Noise frequency for terrain generation.</summary>
    public float frequency = 0.1f;
    /// <summary>Seed for procedural generation.</summary>
    public int seed = 0;
    /// <summary>Maximum Y dimension (height) of the world (in blocks).</summary>
    public int maxY = 32;
    /// <summary>Maximum terrain height (surface elevation).</summary>
    public int maxTerrainHeight = 24;
    /// <summary>Minimum elevation (Y) of the world.</summary>
    public int minElevation = 0;
    /// <summary>The current elevation (Y layer) being viewed or interacted with.</summary>
    public int currentElevation;

    /// <summary>Singleton instance of the world.</summary>
    public static World Instance;

    /// <summary>Prefab for a vertical slice (Y layer) of the world.</summary>
    public GameObject YSlicePrefab;
    /// <summary>Prefab for a chunk of the world.</summary>
    public GameObject ChunkPrefab;

    /// <summary>List of GameObjects representing each vertical slice (Y layer) of the world.</summary>
    private List<GameObject> ySlices = new List<GameObject>();
    /// <summary>Public getter for the list of Y slice GameObjects.</summary>
    public List<GameObject> YSlices => ySlices;

    /// <summary>Event triggered when the current elevation changes.</summary>
    public static event Action<int> OnCurrentElevationChanged;

    /// <summary>The number of chunks along the X axis.</summary>
    public int ChunkXCount => maxX / ChunkData.CHUNK_SIZE;
    /// <summary>The number of chunks along the Z axis.</summary>
    public int ChunkZCount => maxZ / ChunkData.CHUNK_SIZE;

    /// <summary>The size of each simulation chunk in blocks (in each dimension).</summary>
    /// <remarks>
    /// This value determines the granularity of the simulation updates and the size of the data processed in each chunk.
    /// </remarks>
    [SerializeField] private int simulationChunkSize = 16; // Size of each chunk in blocks in each dimension

    private Unity.Entities.World dotsWorld;
    private EntityManager entityManager;

    private BlockAccessor blockAccessor;

    /// <summary>The number of hedges to generate in the world.</summary>
    public int numberOfHedges = 10;

    /// <summary>Reference to the block database for block definitions and textures.</summary>
    private BlockDatabase blockDatabase;

    private List<OreConfig> oreConfigs;

    /// <summary>
    /// Initializes the world singleton, loads block definitions, and initializes world data.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        blockDatabase = BlockDatabase.Instance; // Load block definitions from the database
        ChunkPrefab.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = blockDatabase.TextureAtlas; // Set a default material for the chunk prefabs
        WorldData.Instance.Initialise(maxX, maxY, maxZ, minElevation);
        LoadAllConfigurations(); // Load all ore configurations at startup
        InitialiseDotsWorld(); // Set up the DOTS world and entity manager
        blockAccessor = new BlockAccessor(this);
    }

    private void LoadAllConfigurations()
    {
        oreConfigs = OreConfigLoader.LoadAllOreConfigs();
        Debug.Log($"Loaded {oreConfigs.Count} ore configurations.");

    }

    private void InitialiseDotsWorld()
    {
        dotsWorld = Unity.Entities.World.DefaultGameObjectInjectionWorld;
        entityManager = dotsWorld.EntityManager;
    }

    /// <summary>
    /// Starts world generation on scene start.
    /// </summary>
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        StartCoroutine(GenerateWorldCoroutine());
    }

    /// <summary>
    /// Handles elevation change input each frame.
    /// </summary>
    void Update()
    {
        HandleElevationChange();
    }

    /// <summary>
    /// Runs the world generation pipeline and mesh generation as a coroutine.
    /// </summary>
    private IEnumerator GenerateWorldCoroutine()
    {
        currentElevation = maxTerrainHeight;

        WorldGenerator generator = new WorldGenerator();
        generator.AddStep(new HeightMapStep());

        OreGenerationStep oreGenerationStep = new OreGenerationStep();
        oreGenerationStep.OreConfigs = oreConfigs; // Set the loaded ore configurations
        generator.AddStep(oreGenerationStep);

        generator.AddStep(new CaveGenerationStep());
        generator.AddStep(new VegetationGenerationStep());



        var context = new WorldGenContext
        {
            maxX = maxX,
            maxY = maxY,
            maxZ = maxZ,
            frequency = frequency,
            seed = seed,
            minElevation = minElevation,
            maxTerrainHeight = maxTerrainHeight,
            dirtHeight = 8,
            blockAccessor = blockAccessor,
            blockDatabase = blockDatabase,
            ySlicePrefab = YSlicePrefab,
            chunkPrefab = ChunkPrefab,
            ySlices = ySlices,
            world = this
        };

        // Run all non-mesh steps synchronously
        generator.Generate(WorldData.Instance, context);

        // Run mesh generation as a coroutine for smooth progress
        var meshStep = new ChunkMeshGenerationStep();
        yield return StartCoroutine(meshStep.ApplyCoroutine(WorldData.Instance, context));

        // After mesh generation create DOTS entities for chunk simulation
        yield return StartCoroutine(CreateSimulationEntitiesCoroutine());

        OnCurrentElevationChanged?.Invoke(currentElevation);
        SetWorldLayerVisibility(currentElevation, false);
    }

    /// <summary>
    /// Sets the visibility of world layers up to the specified elevation, and rebuilds meshes as needed.
    /// </summary>
    /// <param name="y">The elevation (Y layer) to make visible.</param>
    /// <param name="isGoingUp">Whether the elevation is increasing (for mesh rebuild logic).</param>
    private void SetWorldLayerVisibility(int y, bool isGoingUp)
    {
        for (int i = 0; i < ySlices.Count; i++)
        {
            int layerY = i + minElevation;
            bool shouldBeVisible = layerY <= y;

            Transform ySliceTransform = ySlices[i]?.transform;
            if (ySliceTransform == null) continue;

            for (int j = 0; j < ySliceTransform.childCount; j++)
            {
                GameObject chunk = ySliceTransform.GetChild(j).gameObject;
                MeshRenderer renderer = chunk.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = shouldBeVisible;
                }

                if (layerY > y)
                {
                    chunk.layer = 3;
                }
                else
                {
                    if (chunk.layer == 3)
                    {
                        chunk.layer = 0;
                    }
                }
            }
        }

        ChunkUtils.RebuildMeshAtLevel(y, true);
        if (isGoingUp) ChunkUtils.RebuildMeshAtLevel(y - 1);

        currentElevation = y;
    }

    /// <summary>
    /// Handles user input for changing the current elevation (PageUp/PageDown).
    /// </summary>
    private void HandleElevationChange()
    {
        if (Input.GetKeyUp(KeyCode.PageUp))
        {
            if (currentElevation < maxY + 1)
            {
                currentElevation++;
                SetWorldLayerVisibility(currentElevation, true);
                OnCurrentElevationChanged?.Invoke(currentElevation);
            }
        }
        else if (Input.GetKeyUp(KeyCode.PageDown))
        {
            if (currentElevation > minElevation + 1)
            {
                currentElevation--;
                SetWorldLayerVisibility(currentElevation, false);
                OnCurrentElevationChanged?.Invoke(currentElevation);
            }
        }
    }

    private IEnumerator CreateSimulationEntitiesCoroutine()
    {
        int chunksX = Mathf.CeilToInt((float)maxX / simulationChunkSize);
        int chunksZ = Mathf.CeilToInt((float)maxZ / simulationChunkSize);
        int chunksY = Mathf.CeilToInt(((float)maxY - minElevation) / simulationChunkSize);

        Debug.Log($"Creating {chunksX * chunksZ * chunksY} simulation chunks...");

        int processedChunks = 0;
        for (int x = 0; x < chunksX; x++)
        {
            for (int y = 0; y < chunksY; y++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    CreateSimulationChunk(x, y, z);
                    processedChunks++;
                    if (processedChunks % 10 == 0)
                    {
                        yield return null; // Yield every 10 chunks to avoid freezing the main thread
                    }
                }
            }
        }
        Debug.Log($"Created {processedChunks} simulation chunks");
    }

    private void CreateSimulationChunk(int chunkX, int chunkY, int chunkZ)
    {
        Entity chunkEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(chunkEntity, new ChunkSimulationData
        {
            chunkPosition = new Unity.Mathematics.int3(chunkX, chunkY, chunkZ),
            chunkSize = simulationChunkSize,
            needsUpdate = true
        });
        entityManager.SetName(chunkEntity, $"SimulationChunk_{chunkX}_{chunkY}_{chunkZ}");
        // set up block buffer for the chunk
        DynamicBuffer<BlockBuffer> blockBuffer = entityManager.AddBuffer<BlockBuffer>(chunkEntity);

        for (int x = 0; x < simulationChunkSize; x++)
        {
            for (int y = 0; y < simulationChunkSize; y++)
            {
                for (int z = 0; z < simulationChunkSize; z++)
                {
                    int worldX = chunkX * simulationChunkSize + x;
                    int worldY = chunkY * simulationChunkSize + y + minElevation; // Adjust for min elevation
                    int worldZ = chunkZ * simulationChunkSize + z;

                    if (worldX < maxX && worldY < maxY && worldZ < maxZ)
                    {
                        BlockData block = blockAccessor.GetBlockDataFromPosition(worldX, worldY, worldZ);
                        blockBuffer.Add(new BlockBuffer
                        {
                            blockData = new BlockSimulationData
                            {
                                temperature = GetInitialTemperature(worldY),
                                radiationLevel = 0f,
                                pathfindingCost = (byte)block.definition.pathfindingCost,
                                isWalkable = block.definition.isWalkable
                            },
                            localPosition = new Unity.Mathematics.int3(x, y, z)
                        });
                    }
                }
            }
        }
    }

    private float GetInitialTemperature(int worldY)
    {
        float surfaceTemperature = 21f; // Average surface temperature in Celsius   
        float temperatureGradient = 0.5f;
        return surfaceTemperature + (maxTerrainHeight - worldY) * temperatureGradient;
    }
    
}