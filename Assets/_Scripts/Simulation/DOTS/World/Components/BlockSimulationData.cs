using Unity.Collections;
using Unity.Entities;
public struct BlockSimulationData : IComponentData
{
    public float temperature;
    public float radiationLevel;
    public byte pathfindingCost;
    public bool isWalkable;
    public FixedString64Bytes blockType;
}