using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class World : MonoBehaviour
{
    MeshFilter filter;
    public int maxX = 16;
    public int maxZ = 16;
    public float frequency = 0.1f;
    public int seed = 0;
    public int maxY = 32;
    public int minElevation = 0;
    public static World Instance;
    public GameObject YSlicePrefab;
    private List<GameObject> ySlices = new List<GameObject>();

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

    }

    private IEnumerator GenerateWorldCoroutine()
    {
        WorldData.Instance.Initialise(maxX, maxY, maxZ);
        MeshData meshData = new MeshData();

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

        for (int y = minElevation; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    if (y < heights[x, z])
                    {
                        WorldData.Instance.YSlices[y].Grid[x][z].IsSolid = true;

                        Vector3 targetPosition = new Vector3(x, y, z);
                        MeshUtilities.CreateFaceUp(meshData, targetPosition);
                        MeshUtilities.CreateFaceDown(meshData, targetPosition);
                        MeshUtilities.CreateFaceNorth(meshData, targetPosition);
                        MeshUtilities.CreateFaceEast(meshData, targetPosition);
                        MeshUtilities.CreateFaceSouth(meshData, targetPosition);
                        MeshUtilities.CreateFaceWest(meshData, targetPosition);
                    }
                }
            }
            LoadMeshData(meshData);
            yield return null;
        }
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

    // private IEnumerator GenerateVerticalSliceCoroutine(int minElevation, int height, int x, int z, MeshData meshData)
    // {
    //     for (int y = minElevation; y < height; y++)
    //     {
    //         WorldData.Instance.YSlices[y].Grid[x][z].IsSolid = true; // Set the block to solid as we are generating a solid block

    //                 Vector3 targetPosition = new Vector3(x, y, z);
    //                 MeshUtilities.CreateFaceUp(meshData, targetPosition);
    //                 MeshUtilities.CreateFaceDown(meshData, targetPosition);
    //                 MeshUtilities.CreateFaceNorth(meshData, targetPosition);
    //                 MeshUtilities.CreateFaceEast(meshData, targetPosition);
    //                 MeshUtilities.CreateFaceSouth(meshData, targetPosition);
    //                 MeshUtilities.CreateFaceWest(meshData, targetPosition);
    //         yield return null;
    //     }
    //  }

    // This is a placeholder for the SetState method
    // Wil be used to set the state of the world on load
    // public void LoadFromSaveData(SaveData saveData)
    // {

    // }
}