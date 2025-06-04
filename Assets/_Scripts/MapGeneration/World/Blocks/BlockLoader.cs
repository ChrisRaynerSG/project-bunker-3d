using System.Collections.Generic;
using UnityEngine;

public class BlockLoader
{
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
            }
            else
            {
                Debug.LogWarning($"Failed to parse block definition from {jsonFile.name}");
            }
        }
        return blocks;
    }

    public static HashSet<string> GetTextureNames(List<BlockDefinition> blocks)
    {
        HashSet<string> textureNames = new();
        foreach (BlockDefinition block in blocks)
        {
            if (block.textures != null)
            {
                textureNames.Add(block.textures.Top);
                textureNames.Add(block.textures.Bottom);
                textureNames.Add(block.textures.Side);
            }
        }
        return textureNames;
    }

    public static List<Texture2D> LoadTextures(HashSet<string> textureNames, out List<string> nameOrder)
    {
        List<Texture2D> textures = new();
        nameOrder = new();
        foreach (string name in textureNames)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Textures/{name}");
            if (texture != null)
            {
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
}