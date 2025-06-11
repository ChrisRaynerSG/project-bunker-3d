using Unity.Entities;

public struct BlockDefinitionSingleton : IComponentData
{
    public BlobAssetReference<BlockDefinitionBlobAsset> Blob;
}