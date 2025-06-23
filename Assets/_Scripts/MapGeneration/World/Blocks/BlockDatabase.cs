using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BlockDatabase
{
    // static dictionary to hold all block types, created on awake
    private Dictionary<string, BlockDefinition> stringIdToBlock = new();
    private Dictionary<ushort, BlockDefinition> numericIdToBlock = new();
    private Dictionary<string, ushort> stringToNumericId = new();


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
            if (stringIdToBlock.ContainsKey(block.id))
            {
                Debug.LogWarning($"Duplicate block ID found: {block.id}. Skipping this block.");
                continue;
            }
            if (numericIdToBlock.ContainsKey(block.numericId))
            {
                Debug.LogWarning($"Duplicate numeric ID found: {block.numericId}. Skipping this block.");
                continue;
            }
            stringIdToBlock[block.id] = block;
            numericIdToBlock[block.numericId] = block;
            stringToNumericId[block.id] = block.numericId;

            Debug.Log($"Registered block: {block.id} with numeric ID {block.numericId}");
        }
        Debug.Log($"Block database initialized with {stringIdToBlock.Count} blocks.");
    }
    /// <summary>
    /// Gets all blocks in the database by their string ID.
    /// </summary>
    public Dictionary<string, BlockDefinition> GetBlocks()
    {
        return stringIdToBlock;
    }
    /// <summary>
    /// Gets the block definition by its string ID.
    /// </summary>
    public BlockDefinition GetBlockDefinition(string id)
    {
        if (stringIdToBlock.TryGetValue(id, out BlockDefinition block))
        {
            return block;
        }
        else
        {
            Debug.LogWarning($"No block definition found for ID: {id}");
            return null;
        }
    }
    /// <summary>
    /// Gets the block definition by its numeric ID.
    /// </summary>
    public BlockDefinition GetBlockDefinition(ushort id)
    {
        if (numericIdToBlock.TryGetValue(id, out var def))
        {
            return def;
        }
        Debug.LogWarning($"No block definition found for ID: {id}");
        return null;
    }
    /// <summary>
    /// Gets the numeric ID of a block by its string ID.
    /// </summary>
    public ushort GetNumericId(string blockName)
    {
        return stringToNumericId.TryGetValue(blockName, out ushort numericId) ? numericId : (ushort)0;
    }
    /// <summary>
    /// Gets the string ID of a block by its numeric ID.
    /// </summary>
    public string GetStringId(ushort numericId)
    {
        BlockDefinition definition = GetBlockDefinition(numericId);
        return definition?.id ?? "unknown";
    }

    /// <summary>
    /// Gets all blocks in the database by their numeric ID.
    /// </summary>
    public IReadOnlyDictionary<ushort, BlockDefinition> GetAllBlocks() => numericIdToBlock;
    /// <summary>
    /// Gets all blocks in the database by their string ID.
    /// </summary>  
    public IReadOnlyDictionary<string, BlockDefinition> GetAllBlocksByStringId() => stringIdToBlock;

    /// <summary>
    /// Checks if a block exists in the database by its numeric ID.
    /// </summary>
    public bool HasBlock(ushort numericId) => numericIdToBlock.ContainsKey(numericId);

    /// <summary>   
    /// Checks if a block exists in the database by its string ID.
    /// </summary>
    public bool HasBlock(string stringId) => stringIdToBlock.ContainsKey(stringId);

    /// <summary>
    /// Creates a lookup array for blocks based on their numeric IDs.
    /// Index = numeric ID, value = BlockDefinition.
    /// This allows for fast access to block definitions by their numeric ID.
    /// </summary>
    public BlockDefinition[] CreateDotsLookupArray()
    {
        if (numericIdToBlock.Count == 0)
        {
            Debug.LogWarning("No blocks found in the database. Returning empty array.");
            return new BlockDefinition[0];
        }
        ushort maxId = 0;
        foreach (var kvp in numericIdToBlock)
        {
            if (kvp.Key > maxId)
            {
                maxId = kvp.Key;
            }
        }
        BlockDefinition[] lookupArray = new BlockDefinition[maxId + 1];
        foreach (var kvp in numericIdToBlock)
        {
            lookupArray[kvp.Key] = kvp.Value;
        }
        return lookupArray;
    }
    /// <summary>
    /// Gets the pathfinding cost for a block by its numeric ID. - Useful for DOTS pathfinding.
    /// </summary>
    /// <remarks>
    /// The pathfinding cost is used to determine how difficult it is to traverse a block.
    /// If the block is not found, it defaults to 1 (normal cost).
    /// </remarks>
    public byte GetPathfindingCost(ushort numericId)
    {
        BlockDefinition def = GetBlockDefinition(numericId);
        return def?.pathfindingCost ?? 1;
    }
}