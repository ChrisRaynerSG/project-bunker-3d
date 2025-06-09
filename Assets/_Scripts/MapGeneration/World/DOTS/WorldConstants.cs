using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEditor.PackageManager;

public static class WorldConstants
{
    public const int CHUNK_SIZE_X = 16;
    public const int CHUNK_SIZE_Y = 16;
    public const int CHUNK_SIZE_Z = 16;
    public const int WORLD_SIZE = 128; // 128 blocks in x and z directions
    public static readonly int3 CHUNK_SIZE = new int3(CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z);

    public static int WORLD_HEIGHT = 64;
    public static int SEED;
    
}