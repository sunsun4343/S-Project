using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour {

    #region ---Singleton
    private static GM instance;
    public static GM Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GM>();
            }
            return instance;
        }
    }
    #endregion

    //Class
    public GameDB DB { get; private set; }
    public SaveData SaveData { get; private set; }
    public RuntimeData RuntimeData { get; private set; }

    //MonoBehaviour
    public WorldConfigMessenger WorldConfig { get; private set; }
    public WorldGenerator Generator { get; private set; }
    public MultiPlatformInputSystem InputSystem { get; private set; }

    private void Awake()
    {
        //Class
        DB = new GameDB();
        SaveData = new SaveData();
        RuntimeData = new RuntimeData();

        //MonoBehaviour
        WorldConfig = FindObjectOfType<WorldConfigMessenger>();
        Generator = this.GetComponent<WorldGenerator>();
        InputSystem = FindObjectOfType<MultiPlatformInputSystem>();

        //WorldConfigMessenger
        InitWorldConfigMessenger();

    }

    void InitWorldConfigMessenger()
    {
        SaveData.seed = WorldConfig.seed;
        SaveData.map.size = WorldConfig.map.size;

    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(DB.Init_TileBase());
        yield return StartCoroutine(Generator.Generator());


    }



}
