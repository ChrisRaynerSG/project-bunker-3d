using System.Collections.Generic;
using UnityEngine;

public class TextureAtlasBuilder
{
    public static Texture2D AtlasTexture;
    public static Dictionary<string, Rect> UVRects = new();

    public static void BuildAtlas(List<Texture2D> textures, List<string> names)
    {
        if (textures == null || textures.Count == 0)
        {
            Debug.LogWarning("No textures provided for atlas building.");
            return;
        }

        int tileSize = textures[0].width;
        int atlasTilesPerRow = Mathf.CeilToInt(Mathf.Sqrt(textures.Count));
        int atlasSize = atlasTilesPerRow * tileSize;

        AtlasTexture = new Texture2D(atlasSize, atlasSize);
        AtlasTexture.filterMode = FilterMode.Point;
        AtlasTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < textures.Count; i++)
        {
            int x = i % atlasTilesPerRow;
            int y = i / atlasTilesPerRow;

            int px = x * tileSize;
            int py = y * tileSize;

            Color[] pixels = textures[i].GetPixels();
            AtlasTexture.SetPixels(px, py, tileSize, tileSize, pixels);

            float u = px / (float)atlasSize;
            float v = py / (float)atlasSize;
            float s = tileSize / (float)atlasSize;

            UVRects[names[i]] = new Rect(u, v, s, s);
        }

        AtlasTexture.Apply();
    }
}