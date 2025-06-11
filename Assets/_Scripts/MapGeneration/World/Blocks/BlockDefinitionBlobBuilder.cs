using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
public static class BlockDefinitionBlobBuilder
{
    public static BlobAssetReference<BlockDefinitionBlobAsset> Create(List<BlockDefinition> blockDefinitions)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<BlockDefinitionBlobAsset>();
        var array = builder.Allocate(ref root.BlockDefinitions, blockDefinitions.Count);

        for (int i = 0; i < blockDefinitions.Count; i++)
        {
            var def = blockDefinitions[i];
            array[i] = new BlockDefinitionDOTS
            {
                IsSolid = def.isSolid,
                IsMineable = def.isMineable,
                IsFlammable = def.isFlammable,
                MovementCost = def.movementCost,
                MiningTime = def.miningTime,
                Hardness = def.hardness,
                ThermalConductivity = def.thermalConductivity,
                IgnitionTemperature = def.ignitionTemperature,
                Textures = new BlockTextureDefinition
                {
                    Top = def.textures?.top ?? "",
                    Bottom = def.textures?.bottom ?? "",
                    Side = def.textures?.side ?? ""
                }
            };
        }

        // might not need to save the textures as strings, as this was the old way of doing this in the mesh,
        // which might change when moving to DOTS.

        var blob = builder.CreateBlobAssetReference<BlockDefinitionBlobAsset>(Allocator.Persistent);
        builder.Dispose();
        return blob;
    }
}