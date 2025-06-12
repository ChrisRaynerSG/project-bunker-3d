using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct BlockDefinitionDOTS : IComponentData
{
    public FixedString64Bytes Id; // Unique identifier for the block type
    public FixedString64Bytes Name; // Unique name for the block type
    public bool IsSolid;
    public bool IsMineable;
    public bool IsFlammable;
    public float MovementCost;
    public float MiningTime;
    public float Hardness;
    public float ThermalConductivity;
    public float IgnitionTemperature;
    public BlockTextureDefinition Textures;
    public BlockDefinitionUvReference UvReference;

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

public struct BlockDefinitionUvReference
{

    public float4 Top;
    public float4 Side;
    public float4 Bottom;
    
}