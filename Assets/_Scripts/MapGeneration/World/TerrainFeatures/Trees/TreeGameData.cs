using System;
using System.Collections.Generic;
[Serializable]
public class TreeGameData
{
    public List<BlockData> logBlocks = new();
    public List<BlockData> leafBlocks = new();

    public void AddLogBlock(BlockData block)
    {
        logBlocks.Add(block);
    }

    public void AddLeafBlock(BlockData block)
    {
        leafBlocks.Add(block);
    }
}