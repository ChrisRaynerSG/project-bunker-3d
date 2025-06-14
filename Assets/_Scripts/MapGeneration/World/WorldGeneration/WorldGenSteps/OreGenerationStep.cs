public class OreGenerationStep : IWorldGenStep
{
    public void Apply(WorldData data, WorldGenContext context)
    {
        var blockAccessor = context.blockAccessor;
        var blockDatabase = context.blockDatabase;

        for (int x = 0; x < context.maxX; x++)
        {
            for (int y = 0; y < context.maxY; y++)
            {
                for (int z = 0; z < context.maxZ; z++)
                {
                    
                }
            }
        }
    }
}