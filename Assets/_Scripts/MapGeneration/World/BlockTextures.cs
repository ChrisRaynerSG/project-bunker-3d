using System.Collections.Generic;
using UnityEngine;
public static class BlockTextures
{

    public static int atlasSize = 4;

    public static Dictionary<BlockType, BlockTextureSet> BlockUVs = new Dictionary<BlockType, BlockTextureSet>
    {
        { BlockType.Dirt, new BlockTextureSet(2, 2, 2) },
        { BlockType.Grass, new BlockTextureSet(0, 2, 1) },
        { BlockType.Stone, new BlockTextureSet(3, 3, 3) },
        { BlockType.IronOre, new BlockTextureSet(6, 6, 6) },
        { BlockType.CoalOre, new BlockTextureSet(5, 5, 5) },
        {BlockType.CopperOre,  new BlockTextureSet(7, 7, 7) },
        {BlockType.Air, new BlockTextureSet(-1, -1, -1) } // Air has no texture

    };

    public static Vector2 GetUVCoordinates(BlockType type, FaceDirection face)
    {
        if (!BlockUVs.TryGetValue(type, out var texSet))
        {
            Debug.LogWarning($"No texture set for block type {type}");
            return Vector2.zero;
        }

        int index = texSet.GetTextureIndex(face);
        int x = index % atlasSize;
        int y = index / atlasSize;

        float unit = 1f / atlasSize;

        return new Vector2(x * unit, y * unit); // Bottom-left corner of the tile


    }

    public enum FaceDirection
    {
        Up,
        Down,
        North,
        South,
        East,
        West
    }

    public struct BlockTextureSet
    {
        public int top;
        public int bottom;
        public int side;

        public BlockTextureSet(int top, int bottom, int side)
        {
            this.top = top;
            this.bottom = bottom;
            this.side = side;
        }

        public int GetTextureIndex(FaceDirection face)
        {
            switch (face)
            {
                case FaceDirection.Up:
                    return top;
                case FaceDirection.Down:
                    return bottom;
                case FaceDirection.North:
                case FaceDirection.South:
                case FaceDirection.East:
                case FaceDirection.West:
                    return side;
                default:
                    return -1; // Invalid face direction
            }
        }
    }
}

