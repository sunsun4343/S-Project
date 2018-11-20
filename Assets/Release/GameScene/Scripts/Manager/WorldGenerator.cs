using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {

    public Tilemap tilemap0;
    public Tilemap tilemap1;

    public IEnumerator Generator()
    {
        GenerateData();
        GenerateMap();
        SettingCamera();
        yield break;
    }

    void GenerateData()
    {
        SaveData saveData = GM.Instance.SaveData;
        SaveData.Map map = saveData.map;

        map.map_layer0 = new ushort[map.size.x, map.size.y];
        map.map_layer1 = new ushort[map.size.x, map.size.y];

        //산악 지형
        CellularAutomata cellular_stone = new CellularAutomata(map.size.x, map.size.y, 50, 5, saveData.seed);
        int[,] mountinMap = cellular_stone.GenerateMap();

        //나무
        CellularAutomata cellular_tree = new CellularAutomata(map.size.x, map.size.y, 50, 5, saveData.seed);
        int[,] treeMap = cellular_tree.GenerateMap(mountinMap, true);


        for (int x = 0; x < map.size.x; x++)
        {
            for (int y = 0; y < map.size.y; y++)
            {
                map.map_layer0[x, y] = (ushort)GameDB.TileKey.Ground;

                if (mountinMap[x,y] == 0)
                {
                    if (treeMap[x, y] == 0)
                    {
                        map.map_layer1[x, y] = (ushort)GameDB.TileKey.Empty;
                    }
                    else
                    {
                        map.map_layer1[x, y] = (ushort)GameDB.TileKey.Tree;
                    }
                }
                else
                {
                    map.map_layer1[x, y] = (ushort)GameDB.TileKey.StoneBlock;
                }

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
                ushort index = map.map_layer0[x, y];
                tilemap0.SetTile(new Vector3Int(x, y, 0), db.tilebaseDic[index]);

                index = map.map_layer1[x, y];
                tilemap1.SetTile(new Vector3Int(x, y, 0), db.tilebaseDic[index]);

            }
        }
    }

    void SettingCamera()
    {
        SaveData.Map map = GM.Instance.SaveData.map;
        GM.Instance.CamController.transform.position = new Vector3(map.size.x * 0.5f, map.size.y * 0.5f, -10);



    }

}
