using Unity.Entities;
using Unity.Mathematics;

public struct BlockBuffer : IBufferElementData
{
    public BlockSimulationData SimulationData;
    public int3 localPosition;
}
