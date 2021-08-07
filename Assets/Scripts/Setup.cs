using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setup : MonoBehaviour
{

    public List<List<Tile>> tileGrid = new List<List<Tile>>();
    public GameObject tilePrefab;
    public int gridHeight = 5;
    public int gridWidth = 5;
    // Start is called before the first frame update
    void Start()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            List<Tile> row = new List<Tile>();
            tileGrid.Add(row);
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tileObj = Object.Instantiate(tilePrefab, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.x = x;
                tile.y = y;
                tile.transform.position = new Vector2(x, y);
                row.Add(tile);
            }
        }
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                tile.ConnectToNeighboringTiles();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Tile getTile(Vector2 coord)
    {
        int x = (int) coord.x;
        int y = (int) coord.y;
        if (y < 0 || y >= gridHeight || x < 0 || x >= gridWidth)
        {
            return null;
        } else
        {
            return tileGrid[y][x];
        }
    }
}
