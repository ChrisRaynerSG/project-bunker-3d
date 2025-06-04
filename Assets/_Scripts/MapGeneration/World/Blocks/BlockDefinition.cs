[System.Serializable]
public class BlockDefinition
{
    public string id;
    public string displayName;
    public string description;
    public bool isSolid;
    public bool isMineable;
    public bool isFlammable;
    public bool IsRenderable => textures != null;
    public float movementCost;

    public float miningTime;
    public float hardness;
    public TextureDefinition textures;

    [System.Serializable]
    public class TextureDefinition
    {
        public string Top;
        public string Bottom;
        public string Side;
    }
}