using System;
using System.Collections.Generic;

/// <summary>
/// Represents the properties and textures of a block type in the voxel world.
/// 
/// <see cref="BlockDefinition"/> contains physical properties, and texture references for a block.
/// This class is serializable for easy editing and saving in Unity.
/// </summary>
[Serializable]
public class BlockDefinition
{
    /// <summary>
    /// The unique identifier for this block type (e.g., "bunker:stone_block").
    /// </summary>
    public string id;

    /// <summary>
    /// The display name for this block, shown in UI.
    /// </summary>
    public string displayName;

    /// <summary>
    /// A description of the block, for tooltips or documentation.
    /// </summary>
    public string description;

    /// <summary>
    /// Whether this block is solid (cannot be passed through).
    /// </summary>
    public bool isSolid;

    /// <summary>
    /// Whether this block can be mined by the player.
    /// </summary>
    public bool isMineable;

    /// <summary>
    /// Whether this block can catch fire.
    /// </summary>
    public bool isFlammable;

    /// <summary>
    /// Whether this block has textures and should be rendered.
    /// </summary>
    public bool IsRenderable => textures != null;

    /// <summary>
    /// The movement cost for traversing this block (used for pathfinding).
    /// </summary>
    public float movementCost;

    /// <summary>
    /// The time required to mine this block.
    /// </summary>
    public float miningTime;

    /// <summary>
    /// The hardness of this block (affects mining speed).
    /// </summary>
    public float hardness;

    /// <summary>
    /// The thermal conductivity of this block (affects heat transfer).
    /// </summary>
    public float thermalConductivity;

    /// <summary>
    /// The temperature at which this block ignites.
    /// </summary>
    public float ignitionTemperature;

    /// <summary>
    /// The texture references for this block (top, bottom, side).
    /// </summary>
    public TextureDefinition textures;

    /// <summary>
    ///  A list of possible drops when this block is mined.
    ///  </summary>
    public List<DropDefinition> drops;

    /// <summary>
    /// Holds the texture names for each face of a block.
    /// </summary>
    [Serializable]
    public class TextureDefinition
    {
        /// <summary>
        /// The texture name for the top face.
        /// </summary>
        public string top;

        /// <summary>
        /// The texture name for the bottom face.
        /// </summary>
        public string bottom;

        /// <summary>
        /// The texture name for the side faces.
        /// </summary>
        public string side;
    }

    public class DropDefinition
    {
        /// <summary>
        /// The ID of the block that drops when this block is mined.
        /// </summary>
        public string id;

        /// <summary>
        /// The chance of this drop occurring (0.0 to 1.0).
        /// </summary>
        public float chance;

        /// <summary>
        /// The minimum quantity of this drop.
        /// </summary>
        public int minQuantity;

        /// <summary>
        /// The maximum quantity of this drop.
        /// </summary>
        public int maxQuantity;
    }
}