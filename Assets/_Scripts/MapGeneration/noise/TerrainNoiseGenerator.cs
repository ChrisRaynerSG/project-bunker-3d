using UnityEngine;

public class TerrainNoiseGenerator
{

    private readonly FastNoise terrainNoise;
    
    private const float NoiseScale = 0.1f; // Adjust this value to change the noise frequency
    private const float HeightMultiplier = 10f; // Adjust this value to change the height of the terrain

    public static float GenerateHeight(float x, float z)
    {
        // Generate Perlin noise based height
        float noiseValue = Mathf.PerlinNoise(x * NoiseScale, z * NoiseScale);
        return noiseValue * HeightMultiplier;
    }
}