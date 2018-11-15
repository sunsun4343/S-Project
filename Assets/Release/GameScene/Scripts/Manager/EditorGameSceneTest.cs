using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGameSceneTest : MonoBehaviour {

#if UNITY_EDITOR

    public Vector2Int mapSize;

    public void Init()
    {
        WorldConfigMessenger worldConfig = FindObjectOfType<WorldConfigMessenger>();

        if (worldConfig == null)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "WorldConfigMessenger";
            worldConfig = gameObject.AddComponent<WorldConfigMessenger>();

            worldConfig.map = new SaveData.Map();
            worldConfig.map.size = mapSize;

        }
    }

#endif

}
