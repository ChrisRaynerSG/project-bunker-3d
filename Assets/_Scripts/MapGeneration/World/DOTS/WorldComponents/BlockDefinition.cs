using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct BlockDefinitionDOTS : IComponentData
{
    public bool IsSolid;
    public bool IsMineable;
    public bool IsFlammable;
    public float MovementCost;
    public float MiningTime;
    public float Hardness;
    public float ThermalConductivity;
    public float IgnitionTemperature;
    public BlockTextureDefinition Textures;

}

public struct BlockTextureDefinition : IComponentData
{
    public FixedString64Bytes Top;
    public FixedString64Bytes Bottom;
    public FixedString64Bytes Side;
}

public struct BlockDefinitionBlobAsset
{
    public BlobArray<BlockDefinitionDOTS> BlockDefinitions;
}