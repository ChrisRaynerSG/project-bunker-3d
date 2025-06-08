using JetBrains.Annotations;
using Unity.Mathematics;

public static class WorldConstants
{
    public const int CHUNK_SIZE_X = 16;
    public const int CHUNK_SIZE_Y = 16;
    public const int CHUNK_SIZE_Z = 16;
    public static readonly int3 CHUNK_SIZE = new int3(CHUNK_SIZE_X, CHUNK_SIZE_Y, CHUNK_SIZE_Z);
    
}