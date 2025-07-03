using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.Entities
{
    // Movement components
    namespace Movement
    {
        /// <summary>
        /// Represents the movement direction of an entity.
        /// </summary>
        public struct MoveDirection : IComponentData
        {
            public float3 Value;
        }

        /// <summary>
        /// Represents the speed of an entity's movement.
        /// </summary>
        public struct MoveSpeed : IComponentData
        {
            public float Value;
            public float BaseSpeed;
            public float SpeedModifier;
        }
    }

    // Health components
    namespace Health
    {
        /// <summary>
        /// Represents the health of an entity.
        /// </summary>
        public struct Health : IComponentData
        {
            public float Value;
        }

        /// <summary>
        /// Represents the maximum health of an entity.
        /// </summary>
        public struct MaxHealth : IComponentData
        {
            public float Value;
        }

        /// <summary>
        /// Represents the health regeneration rate of an entity.
        /// </summary>
        public struct HealthRegenRate : IComponentData
        {
            public float Value;
        }
    }

    // Visual components
    namespace Visual
    {
        public struct EntityVisual : IComponentData
        {
            public float Scale;
            public float4 Colour;
            public bool IsVisible;
            public bool CastShadows;
        }

        // Add animation components later when worrying about animations
    }

    // AI components
    namespace AI
    {
        public struct AIState : IComponentData
        {
            public int Current;
            public int Previous;
            public float StateTime;
        }
    }
}
