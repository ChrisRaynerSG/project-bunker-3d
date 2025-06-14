using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class World : MonoBehaviour
{
    MeshFilter filter;

    // world sizes will be: 
    // 128x128 small world
    // 192x192 medium world
    // 256x256 large world
    // 400x400 huge world
    public int maxX = 16;
    public int maxZ = 16;
    public float frequency = 0.1f;
    public int seed = 0;
    public int maxY = 32;
    public int maxTerrainHeight = 24;
    public int minElevation = 0;
    public int currentElevation;
    public static World Instance;
    public GameObject YSlicePrefab;
    public GameObject ChunkPrefab;
    private List<GameObject> ySlices = new List<GameObject>();
    public List<GameObject> YSlices => ySlices;
    public static event Action<int> OnCurrentElevationChanged;
    public int ChunkXCount => maxX / ChunkData.CHUNK_SIZE;
    public int ChunkZCount => maxZ / ChunkData.CHUNK_SIZE;

    public int numberOfHedges = 10;

    private BlockDatabase blockDatabase;

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
    }
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        StartCoroutine(GenerateWorldCoroutine());
    }

    void Update()
    {
        HandleElevationChange();
    }
    private IEnumerator GenerateWorldCoroutine()
    {
        WorldGenerator generator = new WorldGenerator();
        generator.AddStep(new HeightMapStep());
        generator.AddStep(new OreGenerationStep());
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
            blockAccessor = new BlockAccessor(this),
            blockDatabase = blockDatabase,
            ySlicePrefab = YSlicePrefab,
            chunkPrefab = ChunkPrefab,
            ySlices = ySlices,
            world = this
        };

        // Run all non-mesh steps synchronously
        generator.Generate(WorldData.Instance, context);

        // Run mesh generation as a coroutine for smooth progress at some point add an event to update loading screen UI
        var meshStep = new ChunkMeshGenerationStep();
        yield return StartCoroutine(meshStep.ApplyCoroutine(WorldData.Instance, context));

        currentElevation = maxTerrainHeight;
        OnCurrentElevationChanged?.Invoke(currentElevation);
        SetWorldLayerVisibility(currentElevation, false);
    }

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

        RebuildTopFacesForceTop(y);
        if (isGoingUp) ChunkUtils.RebuildMeshAtLevel(y - 1);

        currentElevation = y;
    }

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
    private void RebuildTopFacesForceTop(int y)
    {
        if (ySlices.Count < maxY - minElevation)
        {
            return;
        }
        // Rebuild the top faces of the current layer
        GameObject ySliceObject = ySlices[y - minElevation];
        for (int chunkX = 0; chunkX < ChunkXCount; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < ChunkZCount; chunkZ++)
            {
                GameObject chunkObject = ySliceObject.transform.GetChild(chunkX * ChunkZCount + chunkZ).gameObject;
                MeshFilter chunkFilter = chunkObject.GetComponent<MeshFilter>();
                MeshData meshData = new MeshData();

                for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                    {
                        int worldX = chunkX * ChunkData.CHUNK_SIZE + x;
                        int worldZ = chunkZ * ChunkData.CHUNK_SIZE + z;

                        if (worldX < maxX && worldZ < maxZ && BlockUtils.IsSolid(worldX, y, worldZ))
                        {
                            BlockData blockData = WorldData.Instance.YSlices[y - minElevation].Chunks[chunkX][chunkZ].Grid[x][z];
                            Vector3 targetPosition = new Vector3(x, y, z);
                            MeshUtilities.CreateFaces(y, meshData, worldX, worldZ, targetPosition, blockData, true);
                        }
                    }
                }
                MeshUtilities.LoadMeshData(meshData, chunkFilter);
            }
        }
    }
}