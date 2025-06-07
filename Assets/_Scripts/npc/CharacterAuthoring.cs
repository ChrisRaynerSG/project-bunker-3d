using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;

namespace Bunker.Npc
{
    public struct CharacterTag : IComponentData { } // Tag component to identify character entities
    public struct CharacterMoveDirection : IComponentData
    {
        public float3 Value;
    }

    public struct CharacterMoveSpeed : IComponentData
    {
        public float Value; // how quickly character moves
    }

    public struct Health : IComponentData
    {
        public float Value;
        public const float MAX_VALUE = 100f; // max health value
    }


    // Regular Unity component for authoring
    public class CharacterAuthoring : MonoBehaviour
    {
        public float moveSpeed;

        private class Baker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic); //Dynamic for an entity that can move around the world .None - dont apply transform data to component
                AddComponent<CharacterMoveDirection>(entity);
                AddComponent(entity, new CharacterMoveSpeed { Value = authoring.moveSpeed }); // add character move speed component with a default value
                AddComponent<Health>(entity, new Health { Value = Health.MAX_VALUE }); // add health component with a default value
            }
        }
    }
    
    public partial struct CharacterMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state) //regular update, called every single frame
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (velocity, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>, CharacterMoveDirection, CharacterMoveSpeed>())
            {
                var moveStep3d = direction.Value * speed.Value * deltaTime; // calculate the movement step
                velocity.ValueRW.Linear = moveStep3d; // apply the movement step to the entity's position
            }
        }
    }
}