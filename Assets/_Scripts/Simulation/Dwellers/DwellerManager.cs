using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

public class DwellerManager : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject dwellerPrefab;
    public int initialDwellerCount = 3;
    public float spawnRadius = 10f;

    public float spawnHeightOffset = 5f;

    [Header("Spawn Location")]
    public Vector3 spawnLocation; // need to set this to the middle of the world at the surface height after the world has been generated.
    // This will be the location where dwellers are spawned initially.
    // If not set, it will default to the GameObject's transform position.

    [Header("Testing")]
    public KeyCode spawnKey = KeyCode.C; // C for colonist as D for dweller is used for moving right.


    private EntityManager entityManager;
    private bool hasSpawnedInitialDwellers = false;
    void Start()
    {
        entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
        World.OnWorldGenerated += OnWorldReady;
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            Vector3 validSpawnPosition = GetValidSpawnPosition(spawnLocation);
            SpawnDweller(validSpawnPosition);
        }
    }

    void OnDestroy() {
        World.OnWorldGenerated -= OnWorldReady;
    }

    void OnWorldReady()
    {
        if (hasSpawnedInitialDwellers) return;

        Vector3 initialSpawnPosition = GetInitialSpawnLocation();
        spawnLocation = initialSpawnPosition;
        Debug.Log($"Initial spawn location set to: {spawnLocation}");
        
        SpawnInitialDwellers();
        hasSpawnedInitialDwellers = true;
        
    }

    void SpawnInitialDwellers()
    {
        for (int i = 0; i < initialDwellerCount; i++)
        {
            float angle = (float)i / initialDwellerCount * math.PI * 2;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                0f,
                Mathf.Sin(angle) * spawnRadius
            ); // possibly need to move up and down to get the right height based on the x,z coordinates.
            Vector3 basePosition = spawnLocation + offset;

            Vector3 roundedPosition = new Vector3(
                Mathf.Round(basePosition.x),
                basePosition.y,
                Mathf.Round(basePosition.z)
            );
            
            Vector3 validSpawnPosition = GetValidSpawnPosition(roundedPosition);
            SpawnDweller(validSpawnPosition);
        }
        Debug.Log($"Spawned {initialDwellerCount} initial dwellers.");
    }

    public GameObject SpawnDweller(Vector3 position)
    {
        if (dwellerPrefab == null)
        {
            Debug.LogError("Dweller prefab is not assigned.");
            return null;
        }

        GameObject dweller = Instantiate(dwellerPrefab, position, Quaternion.identity);

        var dwellerAuthoring = dweller.GetComponent<DwellerAuthoring>();
        if (dwellerAuthoring != null)
        {

        }
        return dweller;
    }

    private Vector3 GetValidSpawnPosition(Vector3 position)
    {
        return GetSurfaceSpawnPosition(position.x, position.z);
    }

    private Vector3 GetInitialSpawnLocation()
    {
        int centreX = World.Instance.maxX / 2;
        int centreZ = World.Instance.maxZ / 2;

        return GetSurfaceSpawnPosition(centreX, centreZ);
    }

    private Vector3 GetSurfaceSpawnPosition(float x, float z)
    {
        Vector3Int searchStart = new Vector3Int(
            Mathf.FloorToInt(x),
            World.Instance.maxY - 1,
            Mathf.FloorToInt(z)
        );

        int surfaceY = SurfaceUtils.FindSurfaceY(searchStart);

        return new Vector3(x, surfaceY + spawnHeightOffset, z);
    }
}