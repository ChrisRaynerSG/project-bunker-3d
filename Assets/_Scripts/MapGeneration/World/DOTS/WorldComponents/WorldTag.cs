using Unity.Entities;

public struct WorldTag : IComponentData
{
    // This tag is used to identify the world entity.
    // It can be used to query for the world entity in systems.
}