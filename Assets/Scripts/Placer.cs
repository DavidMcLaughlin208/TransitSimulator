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
                topLeftOrigins
                    .Also(i => ConnectLotToStreet(i));
                var hotels = topLeftOrigins.getManyRandomElements(2).Also(i => PlaceHotel(i));
                var shops = topLeftOrigins.Except(hotels).Also(i => PlaceShop(i));
                hotels.Also(i => datastore.city[i].occupier.GetComponent<Lot>().GetComponentInChildren<Generator>().SpawnPedestrian(DestinationType.COFFEE));
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

    void ConnectLotToStreet(Vector2Int origin) {
        var lot = datastore.city[origin].occupier.GetComponent<Lot>();

        var neighboringStreets = DirectionUtils.allDirections
            .ToDictionary(
                dir => dir,
                dir =>
                    datastore.city.GetAllBorderingNeighborsInDirection(dir, origin)
                        .Where(cityTile => cityTile.nodeTile != null) // filter down to just road tiles, TODO fix this
                        .ToList())
            .Where(kvp => kvp.Value.Count() > 0) // filter to directions that have road tiles
            .ToDictionary(i => i.Key, i => i.Value);

        var randomDirection = neighboringStreets.Keys.getRandomElement();

        lot.rotation = DirectionUtils.generalRotationMapping[randomDirection];

        var randomNeighboringStreet = neighboringStreets[randomDirection].getRandomElement();
        Tile neighboringTile = randomNeighboringStreet.nodeTile;
        lot.ConnectToStreet(neighboringTile);
    }

    GameObject PlaceAnonBuilding(Vector2Int origin) {
        var lot = datastore.city[origin].occupier.GetComponent<Lot>();
        var building = GameObject.Instantiate(
            prefabs.building,
            lot.transform.position,
            Quaternion.identity);
        building.transform.parent = lot.transform;
        building.GetComponent<Building>().parentLot = lot; // this double-link is not great
        return building;
    }

    void PlaceHotel(Vector2Int origin) {
        var building = PlaceAnonBuilding(origin);
        building.gameObject.name = "Hotel";
        building.AddComponent<Generator>();
        building.GetComponent<Building>().parentLot.gameObject.assignSpriteFromPath("Sprites/brred");
    }

    void PlaceShop(Vector2Int origin) {
        var building = PlaceAnonBuilding(origin);
        var shop = building.AddAndGetComponent<Destination>();
        building.gameObject.name = "Coffee Shop";
        shop.destType = DestinationType.COFFEE;
        building.GetComponent<Building>().parentLot.gameObject.assignSpriteFromPath("Sprites/cyan");
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
        var missingNeighbors = DirectionUtils.allDirections
            .Where(dir => {
                var coord = origin + Vector2Int.RoundToInt(DirectionUtils.directionToCoordinatesMapping[dir]);
                return !datastore.city.ContainsKey(coord) || datastore.city[coord].nodeTile == null;
            })
            .ToList();
        var roadTypeAndRotation = DirectionUtils.RoadUtils.GetRoadTypeAndRotationForMissingNeighbors(missingNeighbors);
        datastore.city[origin].nodeTile.roadType = roadTypeAndRotation.roadType;
        datastore.city[origin].nodeTile.tileRotation = roadTypeAndRotation.rotation;
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