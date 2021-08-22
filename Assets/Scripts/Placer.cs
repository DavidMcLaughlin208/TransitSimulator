using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class Placer : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    List<Vector2Int> roadOutlines = new List<Vector2Int>();
    List<Vector2Int> nextBlockOrientation = BlockOrientations.L;

    PlacementPreview preview = null;
    public class PlacementPreview {
        public bool validPlacement = false;
        public List<Vector2Int> lotOrigins = new List<Vector2Int>();
        public List<GameObject> previewedLots = new List<GameObject>();
        public List<Vector2Int> roadCoords = new List<Vector2Int>();
        public List<GameObject> previewedRoads = new List<GameObject>();

        public void Cleanup() {
            validPlacement = false;
            lotOrigins.Clear();
            previewedLots.ForEach(i => GameObject.Destroy(i));
            previewedLots.Clear();
            previewedRoads.ForEach(i => GameObject.Destroy(i));
            previewedRoads.Clear();
            roadCoords.Clear();
        }
    }

    public void Awake() {
        datastore = this.gameObject.GetComponent<Datastore>();
        prefabs = this.gameObject.GetComponent<Prefabs>();

        nextBlockOrientation = BlockOrientations.allOrientations.getRandomElement();
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
            if (preview.validPlacement) {
                preview.lotOrigins
                    .Also(coord => PlaceLot(coord));
                preview.roadCoords
                    .Also(i => PlaceRoad(i))
                    .Also(i => ReorientRoad(i))
                    .Also(i => datastore.city[i].nodeTile.DisableUnusedRoadNodes())
                    .Also(i => datastore.city[i].nodeTile.EstablishNodeConnections())
                    .Also(i => datastore.city[i].nodeTile.RecalculateNodeLines());
                preview.lotOrigins
                    .Also(i => ConnectLotToStreet(i));
                var hotels = preview.lotOrigins.getManyRandomElements(2).Also(i => PlaceHotel(i));
                var shops = preview.lotOrigins.Except(hotels).Also(i => PlaceShop(i));
                hotels.Also(i => datastore.city[i].occupier.GetComponent<Lot>().GetComponentInChildren<Generator>().SpawnPedestrian(DestinationType.COFFEE));
                nextBlockOrientation = BlockOrientations.allOrientations.Except(new List<List<Vector2Int>>() {nextBlockOrientation}).getRandomElement();
                preview.Cleanup();
            }
        });

        datastore.inputEvents
            .Receive<HoverEvent>()
            .Where(e => TileExistsAt(new Vector2Int(e.cell.x, e.cell.y)))
            .Subscribe(e => {
                if (preview == null) {
                    preview = new PlacementPreview();
                }

                preview.lotOrigins = nextBlockOrientation
                    .Select(coord => new Vector2Int(
                        coord.x * datastore.lotScale.x,
                        coord.y * datastore.lotScale.y
                    ))
                    .Select(coord => new Vector2Int(
                        coord.x + e.cell.x, coord.y + e.cell.y
                    ))
                    .ToList();

                var lotChunks = preview.lotOrigins
                    .SelectMany(coord =>
                        Utils.EndExclusiveRange2D(
                            0,
                            datastore.lotScale.x,
                            0,
                            datastore.lotScale.y)
                        .Select(i => new Vector2Int(coord.x + i.x, coord.y - i.y)) // transform 2d range to +x -y to properly bound down and to the right
                    )
                    .ToList();

                preview.roadCoords = preview.lotOrigins.SelectMany(topLeftCorner => {
                        return Utils
                            .EndExclusiveRange2D(-1, datastore.lotScale.x + 1, -1, datastore.lotScale.y + 1)
                            .Where(coord => // filter to coordinates that are on the edges of the bounding box
                                coord.x == -1
                                || coord.x == datastore.lotScale.x
                                || coord.y == -1
                                || coord.y == datastore.lotScale.y
                            )
                            .Select(coord => new Vector2Int(topLeftCorner.x + coord.x, topLeftCorner.y - coord.y));
                    })
                    .Where(coord => TileExistsAt(coord) && !lotChunks.Contains(coord))
                    .Distinct()
                    .ToList();


                preview.validPlacement = preview.roadCoords.All(coord => { // filter to all valid tiles that are either empty or have a roadtile occupier
                    var tileIsRoadOrEmpty = datastore.city.ContainsKey(coord)
                        ? datastore.city[coord].occupier == null || datastore.city[coord].nodeTile != null
                        : true;
                    return TileExistsAt(coord) && tileIsRoadOrEmpty;
                });

                preview.previewedLots = preview.lotOrigins.Select((origin, index) => {
                    if (preview.previewedLots.Count() != preview.lotOrigins.Count()) {
                        // if the count is a mismatch, just destroy the list and start over
                        preview.previewedLots.ForEach(i => GameObject.Destroy(i));
                        return PreviewLot(origin, preview.validPlacement);
                    } else {
                        return PreviewLot(origin, preview.validPlacement, preview.previewedLots[index]);
                    }
                }).ToList();

                preview.previewedRoads = preview.roadCoords.Select((origin, index) => {
                    if (preview.previewedRoads.Count() != preview.roadCoords.Count()) {
                        // if the count is a mismatch, just destroy the list and start over
                        preview.previewedRoads.ForEach(i => GameObject.Destroy(i));
                        return PreviewRoad(origin, preview.validPlacement);
                    } else {
                        return PreviewRoad(origin, preview.validPlacement, preview.previewedRoads[index]);
                    }
                }).ToList();
            });
    }

    Vector3 GetLotCenterFromTopLeftOrigin(Vector2Int origin) {
        var topLeftCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0));
        var topRightCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x + (datastore.lotScale.x - 1), origin.y, 0));
        var bottomLeftCenter = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y - (datastore.lotScale.y - 1), 0));

        var lotCenterX = ((topRightCenter.x - topLeftCenter.x) / 2f) + topLeftCenter.x;
        var lotCenterY = ((bottomLeftCenter.y - topLeftCenter.y) / 2f) + topLeftCenter.y;

        return new Vector3(lotCenterX, lotCenterY, 0);
    }

    GameObject PreviewLot(Vector2Int origin, bool validPlacement, GameObject instance=null) {
        var lot = instance;
        if (lot == null) {
            lot = GameObject.Instantiate(
                prefabs.lot,
                GetLotCenterFromTopLeftOrigin(origin),
                Quaternion.identity
            );
            lot.transform.localScale = new Vector3(datastore.lotScale.x, datastore.lotScale.y, 1);
        } else {
            lot.transform.position = GetLotCenterFromTopLeftOrigin(origin);
        }
        if (validPlacement) {
            lot.GetComponent<SpriteRenderer>().color = Color.green;
        } else {
            lot.GetComponent<SpriteRenderer>().color = Color.red;
        }
        return lot;
    }

    void PlaceLot(Vector2Int origin) {
        var lot = GameObject.Instantiate(
            prefabs.lot,
            GetLotCenterFromTopLeftOrigin(origin),
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
        if (!datastore.city.Keys.Contains(origin)) {
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
    }

    GameObject PreviewRoad(Vector2Int origin, bool validPlacement, GameObject instance=null) {
        var road = instance;
        if (road == null) {
            road = GameObject.Instantiate(
                prefabs.road,
                GetLotCenterFromTopLeftOrigin(origin),
                Quaternion.identity
            );
        } else {
            road.transform.position = datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0));
        }
        if (validPlacement) {
            road.GetComponent<SpriteRenderer>().color = Color.green;
        } else {
            road.GetComponent<SpriteRenderer>().color = Color.red;
        }
        return road;
    }

    void ReorientRoad(Vector2Int origin) {
        Debug.Log($"Attempting to reorient road at {origin}");
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
                    return false;
                }
            }
        }

        return true;
    }
}