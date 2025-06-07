using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.World
{
    public struct ChunkTag : IComponentData { } // Tag component to identify chunk entities
    public struct DirtyChunkTag : IComponentData { } // Tag component to identify dirty chunk entities, which need to be updated

    public struct ChunkPosition : IComponentData
    {
        public int3 Value; // Position of the chunk in the world
    }
    public struct Block : IBufferElementData // Buffer element data to store blocks in a chunk
    {
        public ushort BlockId;

    }
}
