using Unity.Entities;

public struct HeatSink : IComponentData
{
    public float coolingOutput;
    public float coolingRadius;
    public bool isActive;
    public float fuelLevel;
}