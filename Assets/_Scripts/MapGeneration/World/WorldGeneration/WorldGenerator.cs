using System.Collections.Generic;

public class WorldGenerator
{
    private readonly List<IWorldGenStep> steps = new();

    public void AddStep(IWorldGenStep step) => steps.Add(step);

    public void Generate(WorldData data, WorldGenContext context)
    {
        foreach (var step in steps)
        {
            step.Apply(data, context);
        }
    }

}