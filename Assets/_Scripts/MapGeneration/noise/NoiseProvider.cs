public static class NoiseProvider
{
    public static FastNoise CreateNoise(float frequency, int seed)
    {
        var noise = new FastNoise();
        noise.SetNoiseType(FastNoise.NoiseType.Simplex);
        noise.SetFrequency(frequency);
        noise.SetSeed(seed);
        return noise;
    }
}