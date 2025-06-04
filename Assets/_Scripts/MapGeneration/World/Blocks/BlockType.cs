using UnityEngine;

[CreateAssetMenu(fileName = "BlockType", menuName = "Blocks/BlockType", order = 1)]
public class BlockType : ScriptableObject
{
    public int Id;
    public string displayName;
    public string description;
    public bool isSolid;
    public float movementCost;
    public bool isMineable; //  maybe make like minecraft where bottom layer is not mineable to prevent falling through the bottom of the world?
    public float miningTime;
    public float hardness;
}