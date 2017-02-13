using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundArea : MonoBehaviour {

    public float tileSize = 1.0f;
    public Vector2 areaSize = new Vector2(10.0f, 10.0f);
    public GameObject tile;

	// Use this for initialization
	void Start () {
        InstantiateTiles();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void InstantiateTiles()
    {
        // For now this is extremely simple, it lays tiles in a plane, using its parent's transform and overtiling (overflowing borders) if they don't line up

        // Calculate tile counts in X, Y
        int tilesX = (int)(areaSize.x / tileSize) + 1;
        int tilesY = (int)(areaSize.y / tileSize) + 1;

        // Calculate offsets for first corner tile
        float halfTile = tileSize / 2.0f;
        float x0 = (-tilesX + 1) * halfTile;
        float y0 = (-tilesY + 1) * halfTile;

        // Planes are by default 10.0x10.0 so we need this scale factor
        float planeScale = tileSize / 10.0f;

        // Lay tiles
        for (int i = 0; i < tilesX; i++)
        {
            for (int j = 0; j < tilesY; j++)
            {
                float x = x0 + i * tileSize;
                // Remember, we're doing this in 3D space so our Y is actually Z
                float z = y0 + j * tileSize;

                GameObject t = Instantiate<GameObject>(tile, transform);
                t.transform.position = new Vector3(x, 0, z);
                t.transform.localScale = new Vector3(planeScale, 1, planeScale);
            }
        }
    }
}
