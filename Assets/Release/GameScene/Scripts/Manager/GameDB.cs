using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameDB {

    public enum TileLayer0
    {
        Empty,
        Ground,



        MAX
    }

    public List<TileBase> tileBases = new List<TileBase>();

    public IEnumerator Init_TileBase()
    {
        int count = (int)TileLayer0.MAX;
        for (int i = 0; i < count; i++)
        {
            var req = Resources.LoadAsync<TileBase>(string.Format("TileBase/{0}", i));

            while (!req.isDone)
            {
                yield return null;
            }

            tileBases.Add(req.asset as TileBase);
        }
        yield break;


    }

}
