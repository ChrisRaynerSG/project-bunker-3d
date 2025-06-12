
using System;
using Unity.Burst;
using Unity.Collections;
using UnityEditor.Profiling.Memory.Experimental;

[BurstCompile]
public static class BlockUtilities
{
    public static NativeHashMap<FixedString64Bytes, ushort> DefinitionToIdMap;
    public static NativeHashMap<ushort, FixedString64Bytes> IdToDefinitionMap;


    public static void Initialise(int capacity, Allocator allocator)
    {
        if (DefinitionToIdMap.IsCreated)
        {
            DefinitionToIdMap.Dispose();
        }
        if (IdToDefinitionMap.IsCreated)
        {
            IdToDefinitionMap.Dispose();
        }

        DefinitionToIdMap = new NativeHashMap<FixedString64Bytes, ushort>(capacity, allocator);
        IdToDefinitionMap = new NativeHashMap<ushort, FixedString64Bytes>(capacity, allocator);
    }

    public static void Add(FixedString64Bytes definition, ushort id)
    {
        if (!DefinitionToIdMap.TryAdd(definition, id))
        {
            throw new InvalidOperationException($"Definition '{definition}' already exists in the map.");
        }

        if (!IdToDefinitionMap.TryAdd(id, definition))
        {
            throw new InvalidOperationException($"ID '{id}' already exists in the map.");
        }
    }

    public static void Dispose()
    {
        DefinitionToIdMap.Dispose();
        IdToDefinitionMap.Dispose();
    }

}