using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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

        FastNoise noise = new FastNoise();
        noise.SetNoiseType(FastNoise.NoiseType.Simplex);
        noise.SetFrequency(frequency);
        noise.SetSeed(seed);

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

        // first pass to set solid blocks

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

                        // WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ].IsSolid = true;
                        // WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ] = 1;
                        // WorldData.Instance.YSlices[yIndex].Grid[x][z].IsSolid = true;
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
                                if (!IsSolid(worldX, y + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition);
                                if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition);
                                if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition);
                                if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition);
                                if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition);
                                if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition);
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
                    if(chunk.layer == 3)
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
                            Vector3 targetPosition = new Vector3(x, y, z);
                            MeshUtilities.CreateFaceUp(meshData, targetPosition);
                            if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition);
                            if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition);
                            if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition);
                            if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition);
                            if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition);
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
                            Vector3 targetPosition = new Vector3(x, y, z);
                            if (currentElevation == y)
                            {
                                MeshUtilities.CreateFaceUp(meshData, targetPosition);
                            }
                            else
                            {
                                if (!IsSolid(worldX, y + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition);
                            }
                            if (!IsSolid(worldX, y - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition);
                            if (!IsSolid(worldX, y, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition);
                            if (!IsSolid(worldX + 1, y, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition);
                            if (!IsSolid(worldX, y, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition);
                            if (!IsSolid(worldX - 1, y, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition);
                        }
                    }
                }
                LoadMeshData(meshData, chunkFilter);
                chunkObject.GetComponent<MeshCollider>().sharedMesh = chunkFilter.mesh;
            }
        }
    }

    public void RebuildChunkMesh(ChunkData chunkData)
    {
        if (chunkData == null) return;

        int chunkX = chunkData.ChunkX;
        int chunkZ = chunkData.ChunkZ;
        int yIndex = chunkData.OriginY - minElevation;

        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length) return;

        GameObject chunkObject = ySlices[yIndex].transform.Find($"Chunk_{chunkX}_{chunkZ}_{chunkData.OriginY}").gameObject;

        if (chunkObject == null) return;

        MeshFilter chunkFilter = chunkObject.GetComponent<MeshFilter>();
        MeshData meshData = new MeshData();

        for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
            {
                int worldX = chunkX * ChunkData.CHUNK_SIZE + x;
                int worldZ = chunkZ * ChunkData.CHUNK_SIZE + z;

                if (worldX < maxX && worldZ < maxZ && IsSolid(worldX, chunkData.OriginY, worldZ))
                {
                    Vector3 targetPosition = new Vector3(x, chunkData.OriginY, z);
                    if (currentElevation == chunkData.OriginY)
                        {
                            MeshUtilities.CreateFaceUp(meshData, targetPosition);
                        }
                        else
                        {
                            if (!IsSolid(worldX, chunkData.OriginY + 1, worldZ)) MeshUtilities.CreateFaceUp(meshData, targetPosition);
                        }
                    if (!IsSolid(worldX, chunkData.OriginY - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition);
                    if (!IsSolid(worldX, chunkData.OriginY, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition);
                    if (!IsSolid(worldX + 1, chunkData.OriginY, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition);
                    if (!IsSolid(worldX, chunkData.OriginY, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition);
                    if (!IsSolid(worldX - 1, chunkData.OriginY, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition);
                }
            }
        }
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

    // public ChunkData GetChunkAtWorldPosition(Vector3Int position)
    // {
    //     Vector3Int chunkCoord = WorldToChunkCoord(position);

    // }
    // public Vector3Int WorldToChunkCoord(Vector3Int position)
    // {
    //     return new Vector3Int(
    //         position.x / ChunkData.CHUNK_SIZE,
    //         position.y,
    //         position.z / ChunkData.CHUNK_SIZE
    //     );
    // }
}