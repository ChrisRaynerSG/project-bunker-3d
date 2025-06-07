using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.Npc
{
    public struct SkillTag : IComponentData { } // Tag component to identify skill entities (most likely enemies and dwellers)
    public struct MedicineSkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }
    
    public struct EngineeringSkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct CookingSkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct BotanySkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }


    public struct ZoologySkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct GeologySkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct CombatSkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct DiplomacySkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct ComputingSkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public struct ChemistrySkill : IComponentData
    {
        public int Level;
        public int MaxLevel => SkillAuthoring.MAX_SKILL_LEVEL;
        public float Experience;
    }

    public partial class SkillAuthoring : MonoBehaviour
    {
        public const int MAX_SKILL_LEVEL = 20;

        int medicineSkillLevel;

        public SkillAuthoring(int medicineSkillLevel = 1)
        {
            this.medicineSkillLevel = medicineSkillLevel;
        }

        private class Baker : Baker<SkillAuthoring>
        {
            public override void Bake(SkillAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MedicineSkill { Level = authoring.medicineSkillLevel });
            }
        }
    }
}