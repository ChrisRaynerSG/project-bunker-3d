using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// OreGenerationStep is a world generation step that distributes ore blocks within the terrain using 3D noise.
/// 
/// This step iterates through underground blocks below the surface and uses a noise function to determine
/// where to replace stone blocks with ore blocks (e.g., coal ore). The density and distribution of ores
/// can be controlled by adjusting the noise frequency and threshold.
/// </summary>
public class OreGenerationStep : IWorldGenStep
{

    private List<OreConfig> oreConfigs = new List<OreConfig>
    { //set up list with a default ore config
        new OreConfig{
            oreBlockId = "bunker:coal_ore_block",
            replaceBlockId = "bunker:stone_block",
            threshold = 0.5f,
            frequencyMultiplier = 10f,
            seedOffset = 1,
            minDepth = -12,
            maxDepth = 30 // Adjust as needed
        },
        new OreConfig{
            oreBlockId = "bunker:iron_ore_block",
            replaceBlockId = "bunker:stone_block",
            threshold = 0.4f,
            frequencyMultiplier = 8f,
            seedOffset = 2,
            minDepth = -32,
            maxDepth = 50 // Adjust as needed
        },
        new OreConfig{
            oreBlockId = "bunker:copper_ore_block",
            replaceBlockId = "bunker:stone_block",
            threshold = 0.3f,
            frequencyMultiplier = 6f,
            seedOffset = 3,
            minDepth = -20,
            maxDepth = 60 // Adjust as needed
        }
    };

    public List<OreConfig> OreConfigs
    {
        get => oreConfigs;
        set => oreConfigs = value ?? new List<OreConfig>();
    }

    /// <summary>
    /// Applies ore generation to the provided world data using the given context.
    /// Uses 3D noise to determine which stone blocks should be replaced with ore blocks.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        if (oreConfigs == null || oreConfigs.Count == 0)
        {
            Debug.LogWarning("No ore configurations provided for OreGenerationStep. Skipping ore generation.");
            return;
        }

        foreach (var oreConfig in oreConfigs)
        {
            GenerateOreType(data, context, oreConfig);
        }
    }

    private void GenerateOreType(WorldData data, WorldGenContext context, OreConfig config)
    {
        BlockDefinition oreDefinition = context.blockDatabase.GetBlockDefinition(config.oreBlockId);
        if (oreDefinition == null)
        {
            Debug.LogWarning($"Ore block definition for '{config.oreBlockId}' not found. Skipping generation for this ore type.");
            return;
        }

        int oreSeed = Mathf.Clamp(context.seed + config.seedOffset, 1, int.MaxValue);
        FastNoise oreNoise = NoiseProvider.CreateNoise(context.frequency * config.frequencyMultiplier, oreSeed);

        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                int surfaceHeight = context.heights[x, z];
                if (surfaceHeight <= context.minElevation)
                    continue;

                GenerateOreColumn(context, config, oreNoise, oreDefinition, x, z, surfaceHeight);
            }
        }
    }

    private void GenerateOreColumn(WorldGenContext context, OreConfig config, FastNoise noise, BlockDefinition block, int x, int z, int surfaceHeight)
    {
        for (int y = context.minElevation; y < surfaceHeight; y++)
        {
            if (y < config.minElevation || y > config.maxElevation) continue;

            int depthBelowSurface = surfaceHeight - y;
            if (depthBelowSurface < config.minDepth || depthBelowSurface > config.maxDepth) continue;

            float noiseValue = noise.GetNoise(x, y, z);
            if (noiseValue <= config.threshold) continue;

            BlockData currentBlock = context.blockAccessor.GetBlockDataFromPosition(x, y, z);
            if (currentBlock.definition.id != config.replaceBlockId) continue;

            context.blockAccessor.SetBlockNoMeshUpdate(x, y, z, block);
        }
    }

    /// <summary>
    /// Adds a new ore configuration to the generation step.
    /// </summary>
    /// <param name="oreConfig">The ore configuration to add.</param>
    public void AddOreConfig(OreConfig oreConfig)
    {
        if (oreConfig != null)
        {
            oreConfigs.Add(oreConfig);
        }
    }

    /// <summary>
    /// Removes an ore configuration by ore block ID.
    /// </summary>
    /// <param name="oreBlockId">The ID of the ore block to remove.</param>
    /// <returns>True if the configuration was found and removed.</returns>
    public bool RemoveOreConfig(string oreBlockId)
    {
        for (int i = oreConfigs.Count - 1; i >= 0; i--)
        {
            if (oreConfigs[i].oreBlockId == oreBlockId)
            {
                oreConfigs.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
}