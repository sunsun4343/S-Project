using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {

    public Tilemap tilemap_background;

    public IEnumerator Generator()
    {
        GenerateData();
        GenerateMap();
        yield break;
    }

    void GenerateData()
    {
        SaveData.Map map = GM.Instance.SaveData.map;

        map.map_layer0 = new int[map.size.x, map.size.y];

        for (int x = 0; x < map.size.x; x++)
        {
            for (int y = 0; y < map.size.y; y++)
            {
                switch ((GameDB.TileLayer0)map.map_layer0[x,y])
                {
                    case GameDB.TileLayer0.Ground:

                        break;
                }

                tilemap_background.SetTile(new Vector3Int(x, y, 0), );
                
            }
        }


    }
    
    void GenerateMap()
    {

    }

}
