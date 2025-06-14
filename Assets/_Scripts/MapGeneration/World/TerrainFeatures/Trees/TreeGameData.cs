using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class TreeGameData
{
    public List<BlockData> logBlocks = new();
    public List<BlockData> leafBlocks = new();

    public List<Vector3Int> LogPositions = new();
    public List<Vector3Int> LeafPositions = new();

    public void AddLogBlock(BlockData block)
    {
        logBlocks.Add(block);
    }

    public void AddLeafBlock(BlockData block)
    {
        leafBlocks.Add(block);
    }
    public void AddLogPosition(Vector3Int position)
    {
        LogPositions.Add(position);
    }
    public void AddLeafPosition(Vector3Int position)
    {
        LeafPositions.Add(position);
    }

}