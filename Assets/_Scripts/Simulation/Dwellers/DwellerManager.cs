using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bunker.Entities.Components.Tags.Common;

public class DwellerManager : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject dwellerPrefab;
    public int initialDwellerCount = 3;
    public float spawnRadius = 10f;

    [Header("Spawn Location")]
    public Transform spawnLocation; // need to set this to the middle of the world at the surface height after the world has been generated.
    // This will be the location where dwellers are spawned initially.
    // If not set, it will default to the GameObject's transform position.

    [Header("Testing")]
    public KeyCode spawnKey = KeyCode.C; // C for colonist as D for dweller is used for moving right.


    private EntityManager entityManager;
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (spawnLocation == null)
        {
            spawnLocation = this.transform;
        }

        SpawnInitialDwellers();
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnDweller(spawnLocation.position);
        }
    }

    void SpawnInitialDwellers()
    {
        for (int i = 0; i < initialDwellerCount; i++)
        {
            float angle = (float)i / initialDwellerCount * math.PI * 2;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                0f, // Assuming a flat ground for spawning need to get world height here hmmm...
                Mathf.Sin(angle) * spawnRadius
            ); // possibly need to move up and down to get the right height based on the x,z coordinates.
            Vector3 spawnPosition = spawnLocation.position + offset;
            SpawnDweller(spawnPosition);
        }
        Debug.Log($"Spawned {initialDwellerCount} initial dwellers.");
    }

    public GameObject SpawnDweller(Vector3 position)
    {
        if(dwellerPrefab == null)
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
}