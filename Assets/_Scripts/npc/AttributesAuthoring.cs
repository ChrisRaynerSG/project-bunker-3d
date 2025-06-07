using UnityEngine;
using Unity.Entities;

namespace Bunker.Npc
{
    public struct AttributesTag : IComponentData { } // Tag component to identify attribute entities

    public struct Strength : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }
    public struct Dexterity : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }
    public struct Intelligence : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }
    public struct Charisma : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }
    public struct Endurance : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }
    public struct Luck : IComponentData
    {
        public int Value;
        public int MaxValue;
        public float Experience;
        public float ExperienceToNextLevel;
    }

    public partial class AttributesAuthoring : MonoBehaviour
    {
        public int Strength;
        public int Dexterity;
        public int Intelligence;
        public int Charisma;
        public int Endurance;
        public int Luck;

        public AttributesAuthoring(int strength, int dexterity, int intelligence, int charisma, int endurance, int luck)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Charisma = charisma;
            Endurance = endurance;
            Luck = luck;
        }
        
        private class Baker : Baker<AttributesAuthoring>
        {
            public override void Bake(AttributesAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AttributesTag());
                AddComponent(entity, new Strength
                {
                    Value = authoring.Strength,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
                AddComponent(entity, new Dexterity
                {
                    Value = authoring.Dexterity,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
                AddComponent(entity, new Intelligence
                {
                    Value = authoring.Intelligence,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
                AddComponent(entity, new Charisma
                {
                    Value = authoring.Charisma,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
                AddComponent(entity, new Endurance
                {
                    Value = authoring.Endurance,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
                AddComponent(entity, new Luck
                {
                    Value = authoring.Luck,
                    MaxValue = 10,
                    Experience = 0f,
                    ExperienceToNextLevel = 100f
                });
            }
        }
    }
}