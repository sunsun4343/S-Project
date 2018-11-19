using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData {

    public Map map { get; private set; }
    public SaveData()
    {
        map = new Map();
    }

    [System.Serializable]
    public class Map
    {
        public Vector2Int size;
        public ushort[,] map_layer0;
        public ushort[,] map_layer1;



    }

}
