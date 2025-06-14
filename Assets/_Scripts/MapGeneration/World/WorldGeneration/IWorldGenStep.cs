public interface IWorldGenStep
{
    void Apply(WorldData data, WorldGenContext context);
}