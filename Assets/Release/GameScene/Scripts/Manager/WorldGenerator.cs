using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {

    public Tilemap tilemap_background;

    public IEnumerator Generator()
    {
        for (int x = 0; x < halfsize.x; x++)
        {
            for (int y = -halfsize.y; y < halfsize.y; y++)
            {
                tilemap_Background.SetTile(new Vector3Int(x, y, 0), tiles[mapData.background[x + halfsize.x, y + halfsize.y]]);
            }
        }

        yield return null;
    }

}
