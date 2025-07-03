using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.Entities
{
    public struct MoveDirection : IComponentData
    {
        public float3 Value;
    }
    
}