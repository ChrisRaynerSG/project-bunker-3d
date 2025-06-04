using System.Collections.Generic;
using UnityEngine;

public class BlockDatabase : MonoBehaviour
{
    // static dictionary to hold all block types, created on awake
    public static Dictionary<string, BlockType> Blocks = new();

    void Awake()
    {
        LoadBlocks();
    }

    private void LoadBlocks()
    {
        // Load all BlockType assets in the Resources/Blocks folder
        BlockType[] blockTypes = Resources.LoadAll<BlockType>("Blocks");
        Debug.Log($"Loaded {blockTypes.Length} block types from Resources/Blocks");

        foreach (BlockType block in blockTypes)
        {
            if (!Blocks.ContainsKey(block.Id))
            {
                Blocks.Add(block.Id, block);
            }
            else
            {
                Debug.LogWarning($"Block with name {block.name} already exists in the database.");
            }
        }
    }
}