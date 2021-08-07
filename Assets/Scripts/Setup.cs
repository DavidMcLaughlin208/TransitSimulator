using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setup : MonoBehaviour
{

    public List<List<Tile>> tileGrid = new List<List<Tile>>();
    public GameObject tilePrefab;
    public GameObject hotelPrefab;
    public GameObject shopPrefab;
    public int gridHeight = 5;
    public int gridWidth = 5;
    // Start is called before the first frame update
    void Start()
    {
        int rand = 0;
        List<Rotation> rotations = new List<Rotation>() { Rotation.ZERO, Rotation.NINETY, Rotation.ONEEIGHTY, Rotation.TWOSEVENTY };
        List<RoadType> roadTypes = new List<RoadType>() { RoadType.Straight, RoadType.Intersection, RoadType.TJunction, RoadType.Corner };
        for (int y = 0; y < gridHeight; y++)
        {
            List<Tile> row = new List<Tile>();
            tileGrid.Add(row);
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tileObj = Object.Instantiate(tilePrefab, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                rand = Random.Range(0, roadTypes.Count);
                tile.roadType = roadTypes[rand];
                rand = Random.Range(0, rotations.Count);
                tile.roadRotation = rotations[rand];
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
                tile.ConnectInterally();
                tile.ConnectToNeighboringTiles();
            }
        }
        Hotel hotel = createHotel();
        createShop();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                tile.RecalculateNodeLines();
            }
        }
        hotel.SpawnPedestrian(ShopType.COFEE);
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                tile.RecalculateNodeLines();
            }
        }
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

    private Hotel createHotel()
    {
        GameObject hotelObj = Object.Instantiate(hotelPrefab, transform);
        hotelObj.transform.position = new Vector3(gridWidth, gridHeight - 1);
        Hotel hotel = hotelObj.GetComponent<Hotel>();
        hotel.rotation = Rotation.TWOSEVENTY;
        hotel.x = gridWidth;
        hotel.y = gridHeight - 1;
        hotel.ConnectToStreets();
        return hotel;
    }

    private void createShop()
    {
        GameObject shopObj = Object.Instantiate(shopPrefab, transform);
        shopObj.transform.position = new Vector3(0, 0 - 1);
        Shop shop = shopObj.GetComponent<Shop>();
        shop.rotation = Rotation.ZERO;
        shop.x = 0;
        shop.y = -1;
        shop.setShopType(ShopType.COFEE);
        shop.ConnectToStreets();
    }
}
