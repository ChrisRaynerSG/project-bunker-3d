using Unity.Entities;
using Unity.Mathematics;

public struct Block : IBufferElementData
{
    public ushort Id;
    public float Temperature;
    public float Radiation;

}