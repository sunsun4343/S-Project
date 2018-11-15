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
        SettingCamera();
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
                map.map_layer0[x, y] = (int)GameDB.TileLayer0.Ground;
            }
        }
    }
    
    void GenerateMap()
    {
        SaveData.Map map = GM.Instance.SaveData.map;
        GameDB db = GM.Instance.DB;

        for (int x = 0; x < map.size.x; x++)
        {
            for (int y = 0; y < map.size.y; y++)
            {
                int index = map.map_layer0[x, y];
                tilemap_background.SetTile(new Vector3Int(x, y, 0), db.tileBases[index]);
            }
        }
    }

    void SettingCamera()
    {
        SaveData.Map map = GM.Instance.SaveData.map;
        GM.Instance.CamController.transform.position = new Vector3(map.size.x * 0.5f, map.size.y * 0.5f, -10);



    }

}
