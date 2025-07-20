using Unity.Entities;
using UnityEngine;

public class DwellerAuthoring : MonoBehaviour
{
    private class Baker : Baker<DwellerAuthoring>
    {
        public override void Bake(DwellerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}