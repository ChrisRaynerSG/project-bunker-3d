using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class for building a texture atlas from a list of textures and generating UV rects for each.
/// 
/// <see cref="TextureAtlasBuilder"/> creates a single atlas texture containing all provided textures,
/// adds padding to prevent texture bleeding, and stores the UV coordinates for each texture by name.
/// </summary>
public class TextureAtlasBuilder
{
    /// <summary>
    /// The generated atlas texture containing all block textures.
    /// </summary>
    public static Texture2D AtlasTexture;

    /// <summary>
    /// A dictionary mapping texture names to their UV rects within the atlas.
    /// </summary>
    public static Dictionary<string, Rect> UVRects = new();

    /// <summary>
    /// Builds a texture atlas from the provided textures and names, and calculates UV rects for each.
    /// </summary>
    /// <param name="textures">A list of textures to include in the atlas.</param>
    /// <param name="names">A list of names corresponding to each texture.</param>
    /// <returns>The generated atlas texture, or null if no textures are provided.</returns>
    public static Texture2D BuildAtlas(List<Texture2D> textures, List<string> names)
    {
        if (textures == null || textures.Count == 0)
        {
            Debug.LogWarning("No textures provided for atlas building.");
            return null;
        }

        int padding = 2; // Padding in pixels
        int tileSize = textures[0].width;
        int paddedTileSize = tileSize + padding * 2;

        int atlasTilesPerRow = Mathf.CeilToInt(Mathf.Sqrt(textures.Count));
        int atlasSize = atlasTilesPerRow * paddedTileSize;

        AtlasTexture = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false);
        AtlasTexture.filterMode = FilterMode.Point;
        AtlasTexture.wrapMode = TextureWrapMode.Clamp;
        AtlasTexture.mipMapBias = -10;

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D tex = textures[i];
            int x = i % atlasTilesPerRow;
            int y = i / atlasTilesPerRow;

            int px = x * paddedTileSize;
            int py = y * paddedTileSize;

            // Copy padded tile with duplicated edge pixels
            for (int dx = -padding; dx < tileSize + padding; dx++)
            {
                for (int dy = -padding; dy < tileSize + padding; dy++)
                {
                    int srcX = Mathf.Clamp(dx, 0, tileSize - 1);
                    int srcY = Mathf.Clamp(dy, 0, tileSize - 1);
                    Color color = tex.GetPixel(srcX, srcY);
                    AtlasTexture.SetPixel(px + dx + padding, py + dy + padding, color);
                }
            }

            float u = (px + padding) / (float)atlasSize;
            float v = (py + padding) / (float)atlasSize;
            float s = tileSize / (float)atlasSize;

            UVRects[names[i]] = new Rect(u, v, s, s);
        }

        AtlasTexture.Apply();
        return AtlasTexture;
    }
}