using System;
using UnityEngine;

[Serializable]
public class WorldBlock
{
    private int x;
    public int X
    {
        get { return x; }
        set { x = value; }
    }
    private int y;
    public int Y
    {
        get { return y; }
        set { y = value; }
    }
    private int z;
    public int Z
    {
        get { return z; }
        set { z = value; }
    }

    private bool isSolid;
    public bool IsSolid
    {
        get { return isSolid; }
        set { isSolid = value; }
    }

    private String blockType; // may need to change to enum to make it easier to work with;
    public String BlockType
    {
        get { return blockType; }
        set { blockType = value; }
    }
}