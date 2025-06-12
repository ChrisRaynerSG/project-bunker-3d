using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public static class BlockDefinitionBlobBuilder
{
    public static BlobAssetReference<BlockDefinitionBlobAsset> Create(List<BlockDefinition> blockDefinitions)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<BlockDefinitionBlobAsset>();
        var array = builder.Allocate(ref root.BlockDefinitions, blockDefinitions.Count);

        BlockDatabase db = BlockDatabase.Instance; // we have the uvs of the textures in the database, we need to use them to fill the uv references in the blob asset.
        Dictionary<string, Rect> uvRects = TextureAtlasBuilder.UVRects; // we have db and uv rects now we can store the rects in the blob asset.

        for (int i = 0; i < blockDefinitions.Count; i++)
        {
            var def = blockDefinitions[i];
            
            array[i] = new BlockDefinitionDOTS
            {
                Id = def.id,
                Name = def.displayName,
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
                },
                UvReference = new BlockDefinitionUvReference
                {
                    Top = uvRects.ContainsKey(def.textures?.top) ? new float4(uvRects[def.textures.top].x, uvRects[def.textures.top].y, uvRects[def.textures.top].width, uvRects[def.textures.top].height) : new float4(0, 0, 0, 0),
                    Side = uvRects.ContainsKey(def.textures?.side) ? new float4(uvRects[def.textures.side].x, uvRects[def.textures.side].y, uvRects[def.textures.side].width, uvRects[def.textures.side].height) : new float4(0, 0, 0, 0),
                    Bottom = uvRects.ContainsKey(def.textures?.bottom) ? new float4(uvRects[def.textures.bottom].x, uvRects[def.textures.bottom].y, uvRects[def.textures.bottom].width, uvRects[def.textures.bottom].height) : new float4(0, 0, 0, 0)
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