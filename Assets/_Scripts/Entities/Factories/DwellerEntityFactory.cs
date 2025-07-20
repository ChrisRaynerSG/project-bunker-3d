using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Factory class for creating Dweller entities.
/// This class encapsulates the logic for creating a Dweller entity with its components.
/// It can be extended to include more complex logic such as combining
/// different components or adding additional behavior.
/// </summary>
/// <remarks>
/// This factory is used by the DwellerManager to create Dweller entities
/// when spawning new dwellers in the simulation.
/// </remarks>  
public static class DwellerEntityFactory
{
    static DwellerEntityFactory()
    {
        // Initialize any static data or configurations if needed
        NameDatabase.LoadNameDatabase();
    }

    /// <summary>
    /// Creates a new Dweller entity.
    /// </summary>
    /// <param name="entityManager">The EntityManager instance used to create the entity.</param>
    /// <param name="authoring">The DwellerAuthoring component containing the initial data for the entity.</param>
    /// <param name="position">The initial position of the entity.</param>
    /// <returns>The newly created Dweller entity.</returns>
    public static Entity CreateDwellerEntity(EntityManager entityManager, DwellerAuthoring authoring, Vector3 position)
    {
        var entity = entityManager.CreateEntity();
        entityManager.SetName(entity, $"Dweller_{GenerateDwellerName(true /* for now, will need to update later */ )}_{UnityEngine.Random.Range(1000, 9999)}");

        // Core transform
        entityManager.AddComponentData(entity, new Unity.Transforms.LocalTransform
        {
            Position = position,
            Scale = 1f
        });

        // Tags
        entityManager.AddComponent<Bunker.Entities.Components.Tags.Dweller.Dweller>(entity);
        entityManager.AddComponent<Bunker.Entities.Components.Tags.Common.Moveable>(entity);

        // Movement components
        entityManager.AddComponentData(entity, new Bunker.Entities.Movement.MoveDirection { Value = float3.zero });
        entityManager.AddComponentData(entity, new Bunker.Entities.Movement.MoveSpeed
        {
            Value = authoring.moveSpeed,
            BaseSpeed = authoring.moveSpeed,
            SpeedModifier = 1f
        });

        Debug.Log($"Created dweller entity: {entity}");
        return entity;
    }
    /// <summary>
    /// Generates a random name for the Dweller entity.
    /// This is a placeholder and can be replaced with a more sophisticated naming system.
    /// </summary>
    /// <param name="isMale">Indicates if the generated name should be male or female.</param>
    /// <returns>A random name string.</returns>
    /// <remarks>
    /// This method currently uses a simple random selection from predefined lists.
    /// In the future, it can be expanded to include more complex naming logic.
    /// </remarks>
    private static string GenerateDwellerName(bool isMale)
    {
        return NameDatabase.GenerateRandomName("human", isMale);
    }
}