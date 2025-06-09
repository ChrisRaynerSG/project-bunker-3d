using Unity.Collections;
using Unity.Entities;
using System;


public class BlockTypeMapSingleton : IComponentData, IDisposable
{
    public NativeHashMap<FixedString64Bytes, ushort> BlockTypeMap;

    public void Dispose()
    {
        if (BlockTypeMap.IsCreated)
        {
            BlockTypeMap.Dispose();
        }
    }
}
