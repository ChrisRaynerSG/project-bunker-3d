using UnityEngine;
public static class DwellerAppearance
{
    private static readonly Color[] SkinColors = new Color[]
    {
        new Color32(0xFD, 0xBC, 0xB4, 0xFF), // #FDBCB4 - Very light peach
        new Color32(0xED, 0xBA, 0xA6, 0xFF), // #EDBAA6 - Light peach
        new Color32(0xE8, 0xB6, 0x8A, 0xFF), // #E8B68A - Light tan
        new Color32(0xC6, 0x86, 0x42, 0xFF), // #C68642 - Medium tan
        new Color32(0xA0, 0x52, 0x2D, 0xFF), // #A0522D - Sienna brown
        new Color32(0x8D, 0x55, 0x24, 0xFF), // #8D5524 - Medium brown
        new Color32(0x65, 0x43, 0x21, 0xFF), // #654321 - Dark brown
    };

    public static void ApplyRandomSkinColor(GameObject dweller)
    {
        Transform headTransform = dweller.transform.Find("Head");
        if (headTransform == null)
        {
            Debug.LogWarning($"Head child not found on dweller: {dweller.name}");
            return;
        }

        Renderer headRenderer = headTransform.GetComponent<Renderer>();
        if (headRenderer == null)
        {
            Debug.LogWarning($"Renderer not found on head: {headTransform.name}");
            return;
        }

        Material headMaterial = new Material(headRenderer.material);
        headMaterial.color = GetRandomSkinColor();
        headRenderer.material = headMaterial;

        Debug.Log($"Applied skin color {headMaterial.color} to dweller head");
    }

    public static Color GetRandomSkinColor()
    {
        return SkinColors[Random.Range(0, SkinColors.Length)];
    }

    // Future: Add methods for combining parent skin colors
    // public static Color CombineParentSkinColors(Color parent1, Color parent2)
    // {
    //     // Blend parent colors with some variation
    //     return Color.Lerp(parent1, parent2, Random.Range(0.3f, 0.7f));
    // }
}