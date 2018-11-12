using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData {

    Vector2Int mapsize;

    public string seed = "test";
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    public int[,] background;

    public void GenerateMap(int sizeX, int sizeY)
    {
        mapsize = new Vector2Int(sizeX, sizeY);

        background = new int[sizeX, sizeY];

        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

    }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < mapsize.x; x++)
        {
            for (int y = 0; y < mapsize.y; y++)
            {
                if (x == 0 || x == mapsize.x - 1 || y == 0 || y == mapsize.y - 1)
                {
                    background[x, y] = 1;
                }
                else
                {
                    background[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < mapsize.x; x++)
        {
            for (int y = 0; y < mapsize.y; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    background[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    background[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < mapsize.x && neighbourY >= 0 && neighbourY < mapsize.y)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += background[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }




}
