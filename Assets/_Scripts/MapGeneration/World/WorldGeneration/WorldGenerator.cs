using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages and executes a sequence of world generation steps.
/// 
/// The <see cref="WorldGenerator"/> class allows you to build a modular world generation pipeline
/// by adding steps that implement <see cref="IWorldGenStep"/>. When <see cref="Generate"/> is called,
/// each step is applied in order, modifying the world data according to the provided context.
/// </summary>
public class WorldGenerator
{
    /// <summary>
    /// The ordered list of world generation steps to apply.
    /// </summary>
    private readonly List<IWorldGenStep> steps = new();

    /// <summary>
    /// Adds a world generation step to the pipeline.
    /// </summary>
    /// <param name="step">The step to add. Must implement <see cref="IWorldGenStep"/>.</param>
    public void AddStep(IWorldGenStep step) => steps.Add(step);

    /// <summary>
    /// Executes all added world generation steps in order, modifying the provided world data.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Generate(WorldData data, WorldGenContext context)
    {
        foreach (var step in steps)
        {
            step.Apply(data, context);
            Debug.Log($"Applied world generation step: {step.GetType().Name}.");
        }
    }
}