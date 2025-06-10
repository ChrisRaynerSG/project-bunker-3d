using Unity.Entities;
public struct DirtyChunkTag : IComponentData , IEnableableComponent
{
    // This tag is used to mark chunks that need to be updated or processed.
    // It can be used in systems to filter out chunks that have been modified.
}