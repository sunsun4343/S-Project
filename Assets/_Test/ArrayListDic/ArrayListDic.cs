using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using debug = UnityEngine.Debug;

using UnityEngine;
using UnityEngine.Tilemaps;

public class ArrayListDic : MonoBehaviour {

    public int MaxSize = 1000;
    public int count = 300;
    public TileBase tileBase;

    public int callCount = 1000;


    private void Start()
    {
        Init();

        Stopwatch sw = new Stopwatch();

        sw.Reset();
        sw.Start();
        for (int i = 0; i < callCount; i++)
        {
            int r = Random.Range(0, array.Length);
            var temp = array[r];
        }
        sw.Stop();
        debug.Log("array " + sw.ElapsedTicks.ToString());

        sw.Reset();
        sw.Start();
        for (int i = 0; i < callCount; i++)
        {
            int r = Random.Range(0, list.Count);
            var temp = list[r];
        }
        sw.Stop();
        debug.Log("list " + sw.ElapsedTicks.ToString());

        sw.Reset();
        sw.Start();
        for (int i = 0; i < callCount; i++)
        {

            int r = Random.Range(0, count);
            var temp = dic[r];
        }
        sw.Stop();
        debug.Log("dic " + sw.ElapsedTicks.ToString());

    }


    public TileBase[] array;
    public List<TileBase> list;
    public Dictionary<int, TileBase> dic;

    void Init()
    {
        array = new TileBase[MaxSize];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = tileBase;
        }

        

        list = new List<TileBase>();
        for (int i = 0; i < count; i++)
        {
            list.Add(tileBase);
        }

        dic = new Dictionary<int, TileBase>();
        for (int i = 0; i < count; i++)
        {
            dic.Add((int)i, tileBase);
        }


    }




}
