using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class Placer : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    List<Vector2Int> roadOutlines = new List<Vector2Int>();

    public void Awake()
    {
        datastore = this.gameObject.GetComponent<Datastore>();
        prefabs = this.gameObject.GetComponent<Prefabs>();
    }

    public void Update() {
        roadOutlines.ForEach(coord => {
            datastore.city[coord].nodeTile.RecalculateNodeLines();
        });
    }

    public void Start()
    {
        datastore.inputEvents.Receive<ClickEvent>().Subscribe(e =>
        {
            Debug.Log($"Clicked on {e.cell}");

            var blockOrientation = new List<List<Vector2Int>>() {
                BlockOrientations.I, BlockOrientations.O, BlockOrientations.J, BlockOrientations.L,
            }.getRandomElement();
            var topLeftOrigins = blockOrientation
                .Select(coord => new Vector2Int(
                    coord.x * datastore.lotScale.x,
                    coord.y * datastore.lotScale.y
                ))
                .Select(coord => new Vector2Int(
                    coord.x + e.cell.x, coord.y + e.cell.y
                ))
                .ToList();

            var validPlacement = topLeftOrigins.All(coord =>
                TileChunkExistsAt(coord, datastore.lotScale) && TileChunkIsVacantAt(coord, datastore.lotScale)
            );

            if (validPlacement)
            {
                Debug.Log($"Valid placement");
                topLeftOrigins
                    .Also(coord => PlaceLot(coord));

                roadOutlines = topLeftOrigins.SelectMany(topLeftCorner => {
                    return Utils.EndExclusiveRange2D(-1,datastore.lotScale.x + 1,-1,datastore.lotScale.y + 1)
                    .Select(coord => new Vector2Int(topLeftCorner.x + coord.x, topLeftCorner.y - coord.y));
                }).Where(coord => TileExistsAt(coord) && !TileIsOccupiedByLot(coord)).ToList();
                roadOutlines
                    .Also(i => PlaceRoad(i))
                    .Also(i => ReorientRoad(i))
                    .Also(i => datastore.city[i].nodeTile.DisableUnusedRoadNodes())
                    .Also(i => datastore.city[i].nodeTile.EstablishNodeConnections());
                var hotels = topLeftOrigins.getManyRandomElements(2).Also(i => PlaceHotel(i));
                var shops = topLeftOrigins.Except(hotels).Also(i => PlaceShop(i));
                hotels
                    .Also(i => {
                        var hotel = datastore.city[i].occupier.GetComponent<Hotel>();
                        hotel.ConnectToStreets();
                    });
                shops
                    .Also(i => {
                        var shop = datastore.city[i].occupier.GetComponent<Shop>();
                        shop.ConnectToStreets();
                    });
                hotels
                    .Also(i => {
                        var hotel = datastore.city[i].occupier.GetComponent<Hotel>();
                        hotel.SpawnPedestrian(ShopType.COFFEE);
                    });
            }
        });
    }

    void PlaceLot(Vector2Int origin) {
        var topLeftCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0));
        var topRightCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x + (datastore.lotScale.x - 1), origin.y, 0));
        var bottomLeftCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y - (datastore.lotScale.y - 1), 0));

        var lotCenterX = ((topRightCenter.x - topLeftCenter.x) / 2f) + topLeftCenter.x;
        var lotCenterY = ((bottomLeftCenter.y - topLeftCenter.y) / 2f) + topLeftCenter.y;

        var lot = GameObject.Instantiate(
            prefabs.lot,
            new Vector3(lotCenterX, lotCenterY, 0),
            Quaternion.identity
        );
        lot.transform.localScale = new Vector3(datastore.lotScale.x, datastore.lotScale.y, 1);
        datastore.city[origin] = new CityTile() {
            occupier = lot
        };
        for (int x = 0; x < datastore.lotScale.x; x++) {
            for (int y = 0; y < datastore.lotScale.y; y++) {
                // +x and -y because we're checking from top left corner
                var coord = new Vector2Int(origin.x + x, origin.y - y);
                datastore.city[coord] = new CityTile() {
                    occupier = lot
                };
            }
        }
    }

    void PlaceHotel(Vector2Int origin) {
        // we should opt for only storing Lot and Road tiles in the city grid
        // then attach components for specific behaviors, maybe less specific than Hotel
        // components like "spawner", "receiver", "queuer", and "transporter"
        // then we can mix and match components for buildings with various "floors" that have diff behavior
        // stacking an apartment on top of a restaurant would be easier if we're just adding components
        // and the components affect the behavior of the nodes attached to the street
        var hotel = datastore.city[origin].occupier.AddAndGetComponent<Hotel>();

        var nCoord = new Vector2Int(origin.x, origin.y + 1);
        var sCoord = new Vector2Int(origin.x, origin.y - datastore.lotScale.y);
        var eCoord = new Vector2Int(origin.x + datastore.lotScale.x, origin.y);
        var wCoord = new Vector2Int(origin.x - 1, origin.y);

        var neighboringStreets = new List<Vector2Int>(){ nCoord, sCoord, eCoord, wCoord }
            .Where(coord => datastore.city.ContainsKey(coord) && datastore.city[coord].nodeTile != null);

        if (neighboringStreets.Count() > 0) {
            var randomNeighboringStreet = neighboringStreets.getRandomElement();
            if (randomNeighboringStreet == nCoord) {
                hotel.rotation = Rotation.ZERO;
            }
            if (randomNeighboringStreet == sCoord) {
                hotel.rotation = Rotation.ONEEIGHTY;
            }
            if (randomNeighboringStreet == eCoord) {
                hotel.rotation = Rotation.NINETY;
            }
            if (randomNeighboringStreet == wCoord) {
                hotel.rotation = Rotation.TWOSEVENTY;
            }
        }

        datastore.city[origin].occupier.assignSpriteFromPath("Sprites/brred");
    }

    void PlaceShop(Vector2Int origin) {
        // we should opt for only storing Lot and Road tiles in the city grid
        // then attach components for specific behaviors, maybe less specific than Hotel
        // components like "spawner", "receiver", "queuer", and "transporter"
        // then we can mix and match components for buildings with various "floors" that have diff behavior
        // stacking an apartment on top of a restaurant would be easier if we're just adding components
        // and the components affect the behavior of the nodes attached to the street
        var shop = datastore.city[origin].occupier.AddAndGetComponent<Shop>();

        var nCoord = new Vector2Int(origin.x, origin.y + 1);
        var sCoord = new Vector2Int(origin.x, origin.y - datastore.lotScale.y);
        var eCoord = new Vector2Int(origin.x + datastore.lotScale.x, origin.y);
        var wCoord = new Vector2Int(origin.x - 1, origin.y);

        var neighboringStreets = new List<Vector2Int>(){ nCoord, sCoord, eCoord, wCoord }
            .Where(coord => datastore.city.ContainsKey(coord) && datastore.city[coord].nodeTile != null);

        if (neighboringStreets.Count() > 0) {
            var randomNeighboringStreet = neighboringStreets.getRandomElement();
            if (randomNeighboringStreet == nCoord) {
                shop.rotation = Rotation.ZERO;
            }
            if (randomNeighboringStreet == sCoord) {
                shop.rotation = Rotation.ONEEIGHTY;
            }
            if (randomNeighboringStreet == eCoord) {
                shop.rotation = Rotation.NINETY;
            }
            if (randomNeighboringStreet == wCoord) {
                shop.rotation = Rotation.TWOSEVENTY;
            }
        }

        shop.setShopType(ShopType.COFFEE);

        datastore.city[origin].occupier.assignSpriteFromPath("Sprites/cyan");
    }

    void PlaceRoad(Vector2Int origin) {
        var road = GameObject.Instantiate(
            prefabs.road,
            datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0)),
            Quaternion.identity
        );
        datastore.city[origin] = new CityTile() {
            occupier = road,
            nodeTile = road.GetComponent<Tile>()
        };
    }

    void ReorientRoad(Vector2Int origin) {
        var nCoord = new Vector2Int(origin.x, origin.y + 1);
        var sCoord = new Vector2Int(origin.x, origin.y - 1);
        var eCoord = new Vector2Int(origin.x + 1, origin.y);
        var wCoord = new Vector2Int(origin.x - 1, origin.y);

        var n = datastore.city.ContainsKey(nCoord) && datastore.city[nCoord].nodeTile != null; // check if n road tile exists. this is ugly
        var s = datastore.city.ContainsKey(sCoord) && datastore.city[sCoord].nodeTile != null;
        var e = datastore.city.ContainsKey(eCoord) && datastore.city[eCoord].nodeTile != null;
        var w = datastore.city.ContainsKey(wCoord) && datastore.city[wCoord].nodeTile != null;

        if (n && s && !e && !w) {
            datastore.city[origin].nodeTile.roadType = RoadType.Straight;
            datastore.city[origin].nodeTile.tileRotation = Rotation.NINETY;
        }
        if (e && w && !n && !s) {
            datastore.city[origin].nodeTile.roadType = RoadType.Straight;
            datastore.city[origin].nodeTile.tileRotation = Rotation.ZERO;
        }
        if (!n && s && e && !w) {
            datastore.city[origin].nodeTile.roadType = RoadType.Corner;
            datastore.city[origin].nodeTile.tileRotation = Rotation.ZERO;
        }
        if (!n && s && !e && w) {
            datastore.city[origin].nodeTile.roadType = RoadType.Corner;
            datastore.city[origin].nodeTile.tileRotation = Rotation.NINETY;
        }
        if (n && !s && !e && w) {
            datastore.city[origin].nodeTile.roadType = RoadType.Corner;
            datastore.city[origin].nodeTile.tileRotation = Rotation.ONEEIGHTY;
        }
        if (n && !s && e && !w) {
            datastore.city[origin].nodeTile.roadType = RoadType.Corner;
            datastore.city[origin].nodeTile.tileRotation = Rotation.TWOSEVENTY;
        }
    }

    bool TileIsVacant(Vector2Int origin) {
        return !datastore.city.ContainsKey(origin);
    }

    bool TileIsOccupiedByLot(Vector2Int origin) {
        // this DEF needs to get updated
        return datastore.city.ContainsKey(origin) && datastore.city[origin].nodeTile == null;
    }

    bool TileExistsAt(Vector2Int cell) {
        return datastore.validTiles.GetTile(new Vector3Int(cell.x, cell.y, 0)) != null;
    }

    bool TileChunkExistsAt(Vector2Int origin, Vector2Int size) {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                // +x and -y because we're checking from top left corner
                var validTileInMap = datastore.validTiles.GetTile(new Vector3Int(origin.x + x, origin.y - y, 0)) != null;

                if (!validTileInMap) {
                    Debug.Log($"Tile does not exist at x={origin.x + x},y={origin.y - y}");
                    return false;
                }
            }
        }

        return true;
    }

    bool TileChunkIsVacantAt(Vector2Int origin, Vector2Int size) {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                // +x and -y because we're checking from top left corner
                var occupiedInCity = datastore.city.ContainsKey(new Vector2Int(origin.x + x, origin.y - y));

                if (occupiedInCity) {
                    Debug.Log($"Tile is not vacant at x={origin.x + x},y={origin.y - y}");
                    return false;
                }
            }
        }

        return true;
    }
}