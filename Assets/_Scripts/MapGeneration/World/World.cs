using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.UI;
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

    public int minElevation = 0;
    public int currentElevation;
    public static World Instance;
    public GameObject YSlicePrefab;
    public GameObject ChunkPrefab;
    private List<GameObject> ySlices = new List<GameObject>();
    public static event Action<int> OnCurrentElevationChanged;
    private int ChunkXCount => maxX / ChunkData.CHUNK_SIZE;
    private int ChunkZCount => maxZ / ChunkData.CHUNK_SIZE;

    // need to make chunks, 16x1x16 to make updating the world faster

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

    }
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        StartCoroutine(GenerateWorldCoroutine());
        SetWorldLayerVisibility(maxY, false);
    }

    void Update()
    {
        HandleElevationChange();
    }

    private IEnumerator GenerateWorldCoroutine()
    {
        WorldData.Instance.Initialise(maxX, maxY, maxZ, minElevation);
        // MeshData meshData = new MeshData();

        // Noises for terrain generation could these be split into different classes?

        FastNoise noise = NoiseProvider.CreateNoise(frequency, seed);
        FastNoise coalNoise = NoiseProvider.CreateNoise(frequency * 10f, -seed + 1);
        FastNoise ironNoise = NoiseProvider.CreateNoise(frequency * 10f, -seed + 2);
        FastNoise copperNoise = NoiseProvider.CreateNoise(frequency * 10f, -seed + 3);
    
        // Precompute all heights
        int[,] heights = new int[maxX, maxZ];
        for (int x = 0; x < maxX; x++)
        {
            for (int z = 0; z < maxZ; z++)
            {
                float rawHeight = noise.GetNoise(x, z);
                int height = Mathf.FloorToInt((rawHeight + 1f) * 0.5f * maxY);
                heights[x, z] = height;
            }
        }

        // first pass to set solid blocks and block types

        for (int x = 0; x < maxX; x++)
        {
            for (int z = 0; z < maxZ; z++)
            {
                for (int y = minElevation; y < maxY; y++)
                {
                    if (y < heights[x, z])
                    {
                        int yIndex = y - minElevation;

                        int chunkX = x / ChunkData.CHUNK_SIZE;
                        int chunkZ = z / ChunkData.CHUNK_SIZE;
                        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
                        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

                        BlockData blockData = WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ];
                        blockData.IsSolid = true;

                        // for world gen I want to have grass on top, then dirt for the next few layers then stone
                        // Basic terrain generation logic need to do ore distribution and fancier features like trees and bushes later - also have a main road that passes from the edge of the map to the centre

                        if (y == heights[x, z] - 1)
                        {
                            blockData.type = BlockDatabase.Blocks["Grass"];
                        }
                        else if (y < heights[x, z] - 1 && y > heights[x, z] - 8)
                        {
                            blockData.type = BlockDatabase.Blocks["Dirt"];
                        }
                        else
                        {
                            blockData.type = BlockDatabase.Blocks["Stone"];

                            // may need to change this generation logic to be more complex later, but will try with this for now maybe more dense ores lower down to promote mining further down?

                            if (y < maxY * 0.8f)
                            {
                                float coalNoiseValue = coalNoise.GetNoise(x, y, z);
                                float ironNoiseValue = ironNoise.GetNoise(x, y, z);
                                float copperNoiseValue = copperNoise.GetNoise(x, y, z);

                                if (coalNoiseValue > 0.8f)
                                {
                                    blockData.type = BlockDatabase.Blocks["CoalOre"];
                                }
                                else if (ironNoiseValue > 0.85f)
                                {
                                    blockData.type = BlockDatabase.Blocks["IronOre"];
                                }
                                else if (copperNoiseValue > 0.85f)
                                {
                                    blockData.type = BlockDatabase.Blocks["CopperOre"];
                                }
                            }
                        }
                    }
                }
            }
        }

        // second pass to create faces
        for (int y = minElevation; y < maxY; y++)
        {

            GameObject ySliceObject = Instantiate(YSlicePrefab, transform);
            ySliceObject.name = $"YSlice_{y}";
            ySliceObject.transform.position = new Vector3(0, y, 0);
            ySlices.Add(ySliceObject);

            // int ChunkXCount = maxX / ChunkData.CHUNK_SIZE;
            // int ChunkZCount = maxZ / ChunkData.CHUNK_SIZE;

            for (int chunkX = 0; chunkX < ChunkXCount; chunkX++)
            {
                for (int chunkZ = 0; chunkZ < ChunkZCount; chunkZ++)
                {
                    GameObject chunkObject = Instantiate(ChunkPrefab, ySliceObject.transform);
                    chunkObject.name = $"Chunk_{chunkX}_{chunkZ}_{y}";
                    chunkObject.transform.position = new Vector3(chunkX * ChunkData.CHUNK_SIZE, 0, chunkZ * ChunkData.CHUNK_SIZE);

                    MeshFilter chunkFilter = chunkObject.GetComponent<MeshFilter>();
                    MeshData meshData = new MeshData();

                    for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
                    {
                        for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                        {
                            int worldX = chunkX * ChunkData.CHUNK_SIZE + x;
                            int worldZ = chunkZ * ChunkData.CHUNK_SIZE + z;

                            if (worldX < 0 || worldZ < 0 || worldX >= maxX || worldZ >= maxZ)
                                continue;

                            if (worldX < maxX && worldZ < maxZ && y < heights[worldX, worldZ])
                            {
                                Vector3 targetPosition = new Vector3(x, y, z);
                                BlockData blockData = WorldData.Instance.YSlices[y - minElevation].Chunks[chunkX][chunkZ].Grid[x][z];
                                CreateFaces(y, meshData, worldX, worldZ, targetPosition, blockData);
                            }
                        }
                    }
                    LoadMeshData(meshData, chunkFilter);
                    chunkObject.GetComponent<MeshCollider>().sharedMesh = chunkFilter.mesh;
                    chunkObject.layer = 0; // Set default tag for chunks
                }
            }
            yield return null;
        }
        currentElevation = maxY;
        OnCurrentElevationChanged?.Invoke(currentElevation); // Notify listeners of the initial elevation
    }

    private void CreateFaces(int y, MeshData meshData, int worldX, int worldZ, Vector3 targetPosition, BlockData blockData)
    {

        // need to figure out how to create faces based on the block data, specifically the block type. CreateFaceUp etc has a third parameter for tileIndex, which is used to get the texture from the atlas

        if (!IsSolid(worldX, y + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
        if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition, blockData.type);
        if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition,blockData.type);;
        if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition, blockData.type);
        if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition,blockData.type);;
        if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition, blockData.type);
    }

    public void LoadMeshData(MeshData meshData)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = meshData.vertices.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.uvs.ToArray();

        mesh.RecalculateNormals();

        filter.mesh = mesh;
    }

    public void LoadMeshData(MeshData meshData, MeshFilter filter)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = meshData.vertices.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.uvs.ToArray();

        mesh.RecalculateNormals();

        filter.mesh = mesh;
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
        if (isGoingUp) RebuildMeshAtLevel(y - 1);

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

    private bool IsSolid(int x, int y, int z)
    {
        if (x < 0 || x >= maxX || z < 0 || z >= maxZ || y < minElevation || y >= maxY)
            return false;

        int yIndex = y - minElevation;
        int chunkX = x / ChunkData.CHUNK_SIZE;
        int chunkZ = z / ChunkData.CHUNK_SIZE;
        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

        return WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ].IsSolid;
        // return WorldData.Instance.YSlices[yIndex].Grid[x][z].IsSolid;
    }

    private void RebuildTopFacesForceTop(int y)
    {
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

                        if (worldX < maxX && worldZ < maxZ && IsSolid(worldX, y, worldZ))
                        {
                            BlockData blockData = WorldData.Instance.YSlices[y - minElevation].Chunks[chunkX][chunkZ].Grid[x][z];
                            Vector3 targetPosition = new Vector3(x, y, z);
                            MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition, blockData.type);
                        }
                    }
                }
                LoadMeshData(meshData, chunkFilter);
            }
        }
    }
    public void RebuildMeshAtLevel(int y)
    {
        // Rebuild the mesh at the current elevation
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

                        if (worldX < maxX && worldZ < maxZ && IsSolid(worldX, y, worldZ))
                        {
                            BlockData blockData = WorldData.Instance.YSlices[y - minElevation].Chunks[chunkX][chunkZ].Grid[x][z];
                            Vector3 targetPosition = new Vector3(x, y, z);
                            if (currentElevation == y)
                            {
                                MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
                            }
                            else
                            {
                                if (!IsSolid(worldX, y + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
                            }
                            if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition, blockData.type);
                            if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition, blockData.type);
                        }
                    }
                }
                LoadMeshData(meshData, chunkFilter);
                chunkObject.GetComponent<MeshCollider>().sharedMesh = chunkFilter.mesh;
            }
        }
    }

    public void RebuildChunkMesh(int x, int y, int z)
    {
        //Debug.Log($"Rebuilding chunk mesh at: {x}, {y}, {z}");

        ChunkData chunkData = GetChunkAtPosition(x, y, z);
        if (chunkData == null) return;

        int chunkX = chunkData.ChunkX;
        int chunkZ = chunkData.ChunkZ;
        int yIndex = chunkData.OriginY - minElevation;

        //Debug.Log("Rebuilding chunk at: " + chunkX + ", " + chunkData.OriginY + ", " + chunkZ);

        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length) return;

        GameObject chunkObject = ySlices[yIndex].transform.Find($"Chunk_{chunkX}_{chunkZ}_{chunkData.OriginY}").gameObject;
        //Debug.Log("Found chunk object: " + chunkObject.name);

        if (chunkObject == null) return;

        MeshFilter chunkFilter = chunkObject.GetComponent<MeshFilter>();
        MeshData meshData = new MeshData();

        for (int x1 = 0; x1 < ChunkData.CHUNK_SIZE; x1++)
        {
            for (int z1 = 0; z1 < ChunkData.CHUNK_SIZE; z1++)
            {
                int worldX = chunkX * ChunkData.CHUNK_SIZE + x1;
                int worldZ = chunkZ * ChunkData.CHUNK_SIZE + z1;

                if (worldX < maxX && worldZ < maxZ && IsSolid(worldX, chunkData.OriginY, worldZ))
                {
                    BlockData blockData = WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[x1][z1];
                    Vector3 targetPosition = new Vector3(x1, chunkData.OriginY, z1);
                    if (currentElevation == chunkData.OriginY)
                    {
                        MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
                    }
                    else
                    {
                        if (!IsSolid(worldX, chunkData.OriginY + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.type);
                    }
                    if (!IsSolid(worldX, chunkData.OriginY - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition, blockData.type);
                    if (!IsSolid(worldX, chunkData.OriginY, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition, blockData.type);
                    if (!IsSolid(worldX + 1, chunkData.OriginY, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition, blockData.type);
                    if (!IsSolid(worldX, chunkData.OriginY, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition, blockData.type);
                    if (!IsSolid(worldX - 1, chunkData.OriginY, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition, blockData.type);
                }
            }
        }
        LoadMeshData(meshData, chunkFilter);
        chunkObject.GetComponent<MeshCollider>().sharedMesh = chunkFilter.mesh;
    }

    public ChunkData GetChunkAtPosition(int x, int y, int z)
    {
        int chunkX = x / ChunkData.CHUNK_SIZE;
        int chunkZ = z / ChunkData.CHUNK_SIZE;
        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

        if (chunkX < 0 || chunkZ < 0 || chunkX >= ChunkXCount || chunkZ >= ChunkZCount)
            return null;

        int yIndex = y - minElevation;
        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length)
            return null;

        return WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ];
    }
}