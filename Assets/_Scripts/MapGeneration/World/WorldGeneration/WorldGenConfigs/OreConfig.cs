[System.Serializable]
public class OreConfig
{
    public string oreBlockId;
    public string replaceBlockId = "bunker:stone_block"; // Default to stone block
    public float threshold = 0.5f;
    public float frequencyMultiplier = 10f;
    public int seedOffset = 1;

    public int minDepth = 0;
    public int maxDepth = int.MaxValue; // maybe change this to a more reasonable default

    public int minElevation = 0;
    public int maxElevation = int.MaxValue; // maybe change this to a more reasonable default

}