using System;

[Serializable]
public class VerticalSlice
{

    public int Y { get; set; }
    public bool IsVisible { get; set; }
    public WorldBlock[][] Grid { get; private set; }

    // Constructor
    public VerticalSlice(int y, int maxX, int maxZ)
    {
        Y = y;
        Grid = new WorldBlock[maxX][];
        for (int x = 0; x < maxX; x++)
        {

            Grid[x] = new WorldBlock[maxZ];

            for (int z = 0; z < maxZ; z++)
            {
                Grid[x][z] = new WorldBlock();
                Grid[x][z].X = x;
                Grid[x][z].Y = y;
                Grid[x][z].Z = z;
                Grid[x][z].IsSolid = false;
            }
        }
    }
}