using Unity.Entities;
using Unity.Mathematics;

public struct BlockBuffer : IBufferElementData
{
    public BlockSimulationData blockData;
    public int3 localPosition;
}
