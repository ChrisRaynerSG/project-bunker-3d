using UnityEngine;

public static class Randomizer
{
    public static System.Random GetDeterministicRNG(Vector3 position, int worldSeed)
    {
        int hash = position.GetHashCode() ^ worldSeed;
        return new System.Random(hash);
        // Use a combination of position and world seed to create a deterministic random number generator

    }
    
}