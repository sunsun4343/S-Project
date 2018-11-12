using ATJS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoGameDB : GameDB {

    private static DemoGameDB instance = null;
    private static readonly object padlock = new object();

    private DemoGameDB()
    {
    }

    public static DemoGameDB Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new DemoGameDB();
                }
                return instance;
            }
        }
    }

    //--------------------------------------------------

    public GVector3 position = new GVector3();


}
