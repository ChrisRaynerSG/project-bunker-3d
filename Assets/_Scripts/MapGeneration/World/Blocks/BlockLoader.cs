using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides utility methods for loading block definitions and textures from resources,
/// and for creating ECS blob assets for block data.
/// </summary>
public class BlockLoader
{
    /// <summary>
    /// Loads all block definitions from JSON files in the Resources/Blocks directory.
    /// </summary>
    /// <returns>
    /// A list of <see cref="BlockDefinition"/> objects parsed from the JSON files.
    /// </returns>
    public static List<BlockDefinition> LoadBlockDefinitions()
    {
        List<BlockDefinition> blocks = new();
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Blocks");
        foreach (TextAsset jsonFile in jsonFiles)
        {
            BlockDefinition block = JsonUtility.FromJson<BlockDefinition>(jsonFile.text);
            if (block != null)
            {
                blocks.Add(block);
                Debug.Log($"Loaded block definition: {block.id}");
            }
            else
            {
                Debug.LogWarning($"Failed to parse block definition from {jsonFile.name}");
            }
        }
        AssignNumericIds(blocks);
        return blocks;
    }

    /// <summary>
    /// Assigns numeric IDs to each block definition for efficient processing.
    /// </summary>
    /// <param name="blocks">The list of block definitions to assign IDs to.</param>
    private static void AssignNumericIds(List<BlockDefinition> blocks)
    {
        foreach(BlockDefinition block in blocks)
        {
            if (block.id == "bunker:air_block")
            {
                block.numericId = 0; // Assign a default numeric ID for air block
                break; //Once we find the air block, we can stop searching
            }
        }
        ushort index = 1;
        foreach (BlockDefinition block in blocks)
        {
            if (block.numericId == 0)
            {
                // Skip the air block, it already has numeric ID 0
                continue;
            }
            block.numericId = index++;
            Debug.Log($"Assigned numeric ID {block.numericId} to block {block.id}");
        }
    }

    /// <summary>
    /// Extracts all unique texture names from a list of block definitions.
    /// </summary>
    /// <param name="blocks">The list of block definitions to scan.</param>
    /// <returns>
    /// A <see cref="HashSet{T}"/> containing all unique texture names used by the blocks.
    /// </returns>
    public static HashSet<string> GetTextureNames(List<BlockDefinition> blocks)
    {
        HashSet<string> textureNames = new();
        foreach (BlockDefinition block in blocks)
        {
            Debug.Log($"Loading block: {block.id}");
            if (block.textures != null)
            {
                Debug.Log($"Block {block.id} textures: Top={block.textures.top}, Bottom={block.textures.bottom}, Side={block.textures.side}");
                textureNames.Add(block.textures.top);
                textureNames.Add(block.textures.bottom);
                textureNames.Add(block.textures.side);
            }
        }
        return textureNames;
    }

    /// <summary>
    /// Loads textures from the Resources/Textures directory based on the provided texture names.
    /// </summary>
    /// <param name="textureNames">A set of texture names to load.</param>
    /// <param name="nameOrder">Outputs the order of texture names as they are loaded.</param>
    /// <returns>
    /// A list of <see cref="Texture2D"/> objects corresponding to the loaded textures.
    /// </returns>
    public static List<Texture2D> LoadTextures(HashSet<string> textureNames, out List<string> nameOrder)
    {
        List<Texture2D> textures = new();
        nameOrder = new();
        foreach (string name in textureNames)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Textures/{name}");
            if (texture != null)
            {
                Debug.Log($"Loaded texture: {name} : {texture.name}");
                textures.Add(texture);
                nameOrder.Add(name);
            }
            else
            {
                Debug.LogWarning($"Texture {name} not found in Resources/Textures");
                continue;
            }
        }
        return textures;
    }

    // /// <summary>
    // /// Loads block definitions and creates a blob asset for use with Unity ECS.
    // /// </summary>
    // /// <returns>
    // /// A <see cref="BlobAssetReference{T}"/> containing the block definition blob asset.
    // /// </returns>
    // public static BlobAssetReference<BlockDefinitionBlobAsset> LoadAndCreateBlobAsset()
    // {
    //     List<BlockDefinition> blocks = LoadBlockDefinitions();
    //     return BlockDefinitionBlobBuilder.Create(blocks);
    // }
}