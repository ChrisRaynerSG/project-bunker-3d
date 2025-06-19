// using System.Collections.Generic;
// using Unity.Collections;
// using Unity.Entities;
// using UnityEngine;
// using Unity.Mathematics;
// public static class BlockDefinitionBlobBuilder
// {
//     public static BlobAssetReference<BlockDefinitionBlobAsset> Create(List<BlockDefinition> blockDefinitions)
//     {
//         var builder = new BlobBuilder(Allocator.Temp);
//         ref var root = ref builder.ConstructRoot<BlockDefinitionBlobAsset>();
//         var array = builder.Allocate(ref root.BlockDefinitions, blockDefinitions.Count);

//         BlockDatabase db = BlockDatabase.Instance; // we have the uvs of the textures in the database, we need to use them to fill the uv references in the blob asset.
//         Dictionary<string, Rect> uvRects = TextureAtlasBuilder.UVRects; // we have db and uv rects now we can store the rects in the blob asset.

//         BlockUtilities.Initialise(blockDefinitions.Count, Allocator.Persistent);

//         for (int i = 0; i < blockDefinitions.Count; i++)
//         {
//             var def = blockDefinitions[i];

//             Debug.Log($"Creating UV reference for block {def.id} with textures: Top={def.textures?.top}, Bottom={def.textures?.bottom}, Side={def.textures?.side}");

//             float4 topUv = !string.IsNullOrEmpty(def.textures?.top) && uvRects.TryGetValue(def.textures.top, out var topRect)
//                 ? new float4(topRect.x, topRect.y, topRect.width, topRect.height)
//                 : new float4(0, 0, 0, 0);
//             float4 sideUv = !string.IsNullOrEmpty(def.textures?.side) && uvRects.TryGetValue(def.textures.side, out var sideRect)
//                 ? new float4(sideRect.x, sideRect.y, sideRect.width, sideRect.height)
//                 : new float4(0, 0, 0, 0);
//             float4 bottomUv = !string.IsNullOrEmpty(def.textures?.bottom) && uvRects.TryGetValue(def.textures.bottom, out var bottomRect)
//                 ? new float4(bottomRect.x, bottomRect.y, bottomRect.width, bottomRect.height)
//                 : new float4(0, 0, 0, 0);
//             Debug.Log($"Block {def.id} UVs: Top={topUv}, Side={sideUv}, Bottom={bottomUv}");

//             array[i] = new BlockDefinitionDOTS
//             {
//                 Id = def.id,
//                 Name = def.displayName,
//                 IsSolid = def.isSolid,
//                 IsMineable = def.isMineable,
//                 IsFlammable = def.isFlammable,
//                 MovementCost = def.movementCost,
//                 MiningTime = def.miningTime,
//                 Hardness = def.hardness,
//                 ThermalConductivity = def.thermalConductivity,
//                 IgnitionTemperature = def.ignitionTemperature,
//                 Textures = new BlockTextureDefinition
//                 {
//                     Top = def.textures?.top ?? "",
//                     Bottom = def.textures?.bottom ?? "",
//                     Side = def.textures?.side ?? ""
//                 },
//                 UvReference = new BlockDefinitionUvReference
//                 {
//                     Top = topUv,
//                     Side = sideUv,
//                     Bottom = bottomUv
//                 }
//             };
//             BlockUtilities.Add(def.id, (ushort)i);
//         }

//         // might not need to save the textures as strings, as this was the old way of doing this in the mesh,
//         // which might change when moving to DOTS.

//         var blob = builder.CreateBlobAssetReference<BlockDefinitionBlobAsset>(Allocator.Persistent);
//         builder.Dispose();
//         return blob;
//     }
// }