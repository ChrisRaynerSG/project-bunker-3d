using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.Dweller
{
    public struct DwellerTag : IComponentData { }

    public struct Hunger : IComponentData
    {
        public float Value;
        public const float MAX_VALUE = 100f;
    }
    public struct Thirst : IComponentData
    {
        public float Value;
        public const float MAX_VALUE = 100f;
    }

    public struct Happiness : IComponentData
    {
        public float Value;
        public const float MAX_VALUE = 100f;
    }

    public struct Energy : IComponentData
    {
        public float Value;
        public const float MAX_VALUE = 100f;
    }

    public struct Ailment : IBufferElementData
    {
        public AilmentType Type;
        public float Severity;
    }

    public enum AilmentType
    {
        none,
        hunger,
        thirst,
        tiredness,

    }
    public class DwellerAuthoring : MonoBehaviour
    {
        public float hunger = Hunger.MAX_VALUE;
        public float thirst = Thirst.MAX_VALUE;
        public float happiness = Happiness.MAX_VALUE;
        public float energy = Energy.MAX_VALUE;

        private class Baker : Baker<DwellerAuthoring>
        {
            public override void Bake(DwellerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DwellerTag());
                AddComponent(entity, new Hunger { Value = authoring.hunger });
                AddComponent(entity, new Thirst { Value = authoring.thirst });
                AddComponent(entity, new Happiness { Value = authoring.happiness });
                AddComponent(entity, new Energy { Value = authoring.energy });
            }
        }
    }
}