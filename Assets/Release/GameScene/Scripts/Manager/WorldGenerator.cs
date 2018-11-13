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


    }
    
    void GenerateMap()
    {

    }

}
