/// <summary>
/// Defines a step in the world generation pipeline.
/// 
/// Implementations of this interface encapsulate a single, modular operation that modifies the world data
/// during procedural world generation. Each step receives the current world data and a context object
/// containing parameters and shared resources. Steps can include terrain shaping, cave carving, ore placement,
/// vegetation, or any other world feature.
/// </summary>
public interface IWorldGenStep
{
    /// <summary>
    /// Applies this generation step to the provided world data using the given context.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    void Apply(WorldData data, WorldGenContext context);
}