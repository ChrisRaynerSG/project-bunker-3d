using Bunker.Entities.Components.Tags.Common;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Bunker.Entities.Health;

public class DwellerAuthoring : MonoBehaviour
{

    [Header("Basic Info")]
    public string dwellerName = "Dweller";
    public int age = 25;

    [Header("Movement")]
    public float moveSpeed = 1f;
    public float wanderRadius = 20f;
    public float directionChangeInterval = 3f;

    [Header("Starting Needs")]
    // Placeholder values for needs, can be expanded later.
    public float hunger = 100f;

    [Header("Starting Skills")]
    // This is a placeholder for skills, can be expanded later.
    public float cookingSkill = 10f;


    private class Baker : Baker<DwellerAuthoring>
    {
        public override void Bake(DwellerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // == CORE TAGS == //
            AddComponent(entity, new Bunker.Entities.Components.Tags.Dweller.Dweller());


            // == MOVEMENT COMPONENTS == //

            AddComponent(entity, new Moveable());

            AddComponent(entity, new Bunker.Entities.Movement.MoveDirection { Value = float3.zero });
            AddComponent(entity, new Bunker.Entities.Movement.MoveSpeed
            {
                Value = authoring.moveSpeed,
                BaseSpeed = authoring.moveSpeed,
                SpeedModifier = 1f
            });

            // == HEALTH COMPONENTS == //
            AddComponent(entity, new Killable());
            AddComponent(entity, new Health { Value = 100f });
            AddComponent(entity, new MaxHealth { Value = 100f });
            AddComponent(entity, new HealthRegenRate { Value = 0.1f });
        }
    }
}