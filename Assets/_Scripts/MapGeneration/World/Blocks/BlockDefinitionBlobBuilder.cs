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

            Debug.Log($"Creating UV reference for block {def.id} with textures: Top={def.textures?.top}, Bottom={def.textures?.bottom}, Side={def.textures?.side}");
            Debug.Log($"Block {def.id} Rectangles: " +
                      $"Top={uvRects.GetValueOrDefault(def.textures?.top, new Rect(0, 0, 0, 0))}, " +
                      $"Bottom={uvRects.GetValueOrDefault(def.textures?.bottom, new Rect(0, 0, 0, 0))}, " +
                      $"Side={uvRects.GetValueOrDefault(def.textures?.side, new Rect(0, 0, 0, 0))}");
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
                    
                    Top = !string.IsNullOrEmpty(def.textures?.top) && uvRects.TryGetValue(def.textures.top, out var topRect) 
                        ? new float4(topRect.x, topRect.y, topRect.width, topRect.height) 
                        : new float4(0, 0, 0, 0),
                    Side = !string.IsNullOrEmpty(def.textures?.side) && uvRects.TryGetValue(def.textures.side, out var sideRect) 
                        ? new float4(sideRect.x, sideRect.y, sideRect.width, sideRect.height) 
                        : new float4(0, 0, 0, 0),
                    Bottom = !string.IsNullOrEmpty(def.textures?.bottom) && uvRects.TryGetValue(def.textures.bottom, out var bottomRect) 
                        ? new float4(bottomRect.x, bottomRect.y, bottomRect.width, bottomRect.height) 
                        : new float4(0, 0, 0, 0)
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