using Unity.Entities;

public struct HeatSource : IComponentData
{
    public float heatOutput;
    public float heatRadius;
    public bool isActive;
    public float fuelLevel;
}