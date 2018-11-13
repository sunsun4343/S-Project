using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData {

    public Map map { get; private set; }
    public SaveData()
    {
        map = new Map();
    }

    public class Map
    {
        public Vector2Int size;
        public int[,] map_layer0;


    }

}
