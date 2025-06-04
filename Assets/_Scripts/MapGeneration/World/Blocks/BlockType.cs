using UnityEngine;

[CreateAssetMenu(fileName = "BlockType", menuName = "Blocks/BlockType", order = 1)]
public class BlockType : ScriptableObject
{
    public string Id; // may be better to use an int ID for performance reasons, but string is easier to work with in the editor
    public string DisplayName;
    public string Description;
    public bool IsSolid;
    public float MovementCost;
    public bool IsMineable; //  maybe make like minecraft where bottom layer is not mineable to prevent falling through the bottom of the world?
    public float MiningTime;
    public float Hardness;
}