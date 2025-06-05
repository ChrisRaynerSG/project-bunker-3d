using System.Collections.Generic;
using UnityEngine;

public class BlockDatabase
{
    // static dictionary to hold all block types, created on awake
    private Dictionary<string, BlockDefinition> Blocks = new();

    private Texture2D textureAtlas;
    public Texture2D TextureAtlas
    {
        get { return textureAtlas; }
        private set { textureAtlas = value; }
    }

    public static BlockDatabase _instance;

    public static BlockDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BlockDatabase();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private void Initialize()
    {
        // Load all block definitions from resources
        List<BlockDefinition> blockDefinitions = BlockLoader.LoadBlockDefinitions();
        HashSet<string> textureNames = BlockLoader.GetTextureNames(blockDefinitions);
        List<Texture2D> textures = BlockLoader.LoadTextures(textureNames, out List<string> nameOrder);

        // Build the texture atlas
        TextureAtlas = TextureAtlasBuilder.BuildAtlas(textures, nameOrder);

        // Create block types from definitions
        foreach (BlockDefinition block in blockDefinitions)
        {
            if (!Blocks.ContainsKey(block.id))
            {
                Blocks[block.id] = block;
                Debug.Log($"Added block: {block.id}");
            }
            else
            {
                Debug.LogWarning($"Block with ID {block.id} already exists in the database.");
            }
        }
    }
    public Dictionary<string, BlockDefinition> GetBlocks()
    {
        return Blocks;
    }
    public BlockDefinition GetBlock(string id)
    {
        if (Blocks.TryGetValue(id, out BlockDefinition block))
        {
            return block;
        }
        else
        {
            Debug.LogWarning($"Block with ID {id} not found in the database.");
            return null;
        }
    }
}