using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setup : MonoBehaviour
{

    public List<List<Tile>> tileGrid = new List<List<Tile>>();
    public GameObject tilePrefab;
    public GameObject hotelPrefab;
    public GameObject shopPrefab;
    public GameObject carPrefab;
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
                if (!(y % 2 == 0) && !(x % 2 == 0))
                {
                    row.Add(null);
                    continue;
                }
                GameObject tileObj = Object.Instantiate(tilePrefab, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                rand = Random.Range(0, roadTypes.Count);
                //tile.roadType = RoadType.Intersection;//roadTypes[rand];
                rand = Random.Range(0, rotations.Count);
                //tile.tileRotation = rotations[rand];
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
                if (tile != null)
                {
                    Dictionary<Direction, bool> neighbors = new Dictionary<Direction, bool>();
                    neighbors.Add(Direction.NORTH, getTile(new Vector2(x, y + 1)) != null);
                    neighbors.Add(Direction.EAST, getTile(new Vector2(x + 1, y)) != null);
                    neighbors.Add(Direction.SOUTH, getTile(new Vector2(x, y - 1)) != null);
                    neighbors.Add(Direction.WEST, getTile(new Vector2(x - 1, y)) != null);
                    tile.CalculateRoadType(neighbors);
                }
            }
        }


        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                if (tile != null)
                {
                    tile.DisableUnusedRoadNodes();
                }
            }
        }
        List<RoadNodeLocation> allLocations = new List<RoadNodeLocation>() { RoadNodeLocation.NIN, RoadNodeLocation.NOUT, RoadNodeLocation.EIN, RoadNodeLocation.EOUT, RoadNodeLocation.SIN, RoadNodeLocation.SOUT, RoadNodeLocation.WIN, RoadNodeLocation.WOUT };
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                if(tile == null) { continue; }
                tile.EstablishNodeConnections();

                if (Random.Range(0, 10) > 1)
                {
                    for (int i = 0; i < allLocations.Count; i++)
                    {
                        RoadNodeLocation location = allLocations[i];
                        RoadNode roadNode = tile.roadNodeMap[location];
                        if (roadNode.disabled) { continue; }
                        GameObject carObj = Object.Instantiate(carPrefab, transform);
                        carObj.transform.position = roadNode.transform.position;
                        Car car = carObj.GetComponent<Car>();
                        car.SetNewCurrentNode((RoadNode)roadNode);
                        car.homeNode = (RoadNode)roadNode;
                        car.desiredShopType = ShopType.COFFEE;
                        break;
                    }
                }

                if (Random.Range(0, 10) > 7)
                {
                    Node roadNode = tile.roadNodeMap[RoadNodeLocation.SOUT];
                    roadNode.shopType = ShopType.COFFEE;
                }
            }
        }
        Hotel hotel1 = createHotel(new Vector2(gridWidth, gridHeight - 1), Rotation.TWOSEVENTY);
        Hotel hotel2 = createHotel(new Vector2(2, -1), Rotation.ZERO);
        Hotel hotel3 = createHotel(new Vector2(-1, 2), Rotation.NINETY);
        createShop(ShopType.COFFEE, new Vector2(-1, 0), Rotation.NINETY);
        createShop(ShopType.TEA, new Vector2(-1, gridHeight - 1), Rotation.NINETY);
        createShop(ShopType.BEER, new Vector2(gridWidth, 0), Rotation.TWOSEVENTY);


        List<ShopType> shopTypes = new List<ShopType>() { ShopType.COFFEE, ShopType.TEA, ShopType.BEER };
        for (int i = 0; i < 10; i++)
        {
            rand = Random.Range(0, shopTypes.Count);
            ShopType type = shopTypes[rand];
            hotel1.SpawnPedestrian(type);
            hotel2.SpawnPedestrian(type);
            hotel3.SpawnPedestrian(type);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Tile tile = getTile(new Vector2(x, y));
                if (tile != null)
                {
                    tile.RecalculateNodeLines();
                }
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

    private Hotel createHotel(Vector2 coords, Rotation rotation)
    {
        GameObject hotelObj = Object.Instantiate(hotelPrefab, transform);
        hotelObj.transform.position = new Vector2(coords.x, coords.y);
        Hotel hotel = hotelObj.GetComponent<Hotel>();
        hotel.rotation = rotation;
        hotel.x = (int)coords.x;
        hotel.y = (int)coords.y;
        hotel.ConnectToStreets();
        return hotel;
    }

    private void createShop(ShopType shopType, Vector2 coords, Rotation rotation)
    {
        GameObject shopObj = Object.Instantiate(shopPrefab, transform);
        shopObj.transform.position = new Vector3(coords.x, coords.y);
        Shop shop = shopObj.GetComponent<Shop>();
        shop.rotation = rotation;
        shop.x = (int) coords.x;
        shop.y = (int) coords.y;
        shop.setShopType(shopType);
        shop.ConnectToStreets();
    }
}
