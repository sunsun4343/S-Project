using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerater : MonoBehaviour {

    public Tilemap tilemap_Background;

    public List<TileBase> tiles = new List<TileBase>();

    public Vector2Int mapsize;


    void Start () {

        GenerateMap();
    }

    MapData mapData;

    void GenerateMap()
    {
        mapData = new MapData();
        mapData.GenerateMap(mapsize.x, mapsize.y);

        Vector2Int halfsize = new Vector2Int(mapsize.x / 2, mapsize.y / 2);

        for (int x = -halfsize.x; x < halfsize.x; x++)
        {
            for (int y = -halfsize.y; y < halfsize.y; y++)
            {
                tilemap_Background.SetTile(new Vector3Int(x, y, 0), tiles[mapData.background[x + halfsize.x,y + halfsize.y]]);
            }
        }



    }


}
