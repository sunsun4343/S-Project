using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameDB {

    public enum TileKey
    {
        Empty,
        Ground,

        StoneBlock = 1000,

        MAX
    }

    public Dictionary<ushort, TileBase> tilebaseDic = new Dictionary<ushort, TileBase>();

    public IEnumerator Init_TileBase()
    {
        TileBase[] tileBases = Resources.LoadAll<TileBase>("TileBase");
        for (int i = 0; i < tileBases.Length; i++)
        {
            ushort key;
            bool isSucess = ushort.TryParse(tileBases[i].name, out key);
            if (isSucess)
            {
                tilebaseDic.Add(key, tileBases[i]);
            }
        }
        yield break;
    }

}
