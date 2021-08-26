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
    bool tempSwitchForHotelsAndShops = false;

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

    public void Start() {
        //  _     _            _
        // | |   | |          | |
        // | |__ | | ___   ___| | __
        // | '_ \| |/ _ \ / __| |/ /
        // | |_) | | (_) | (__|   <
        // |_.__/|_|\___/ \___|_|\_\

        datastore.inputEvents // place a block on click!
            .Receive<ClickEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.BLOCK_PLACER)
            .Where(_ => preview != null && preview.validPlacement)
            .Subscribe(e => {
                preview.lotOrigins
                    .Also(coord => PlaceLot(coord));
                preview.roadCoords
                    .Also(i => PlaceRoad(i))
                    .Also(i => ReorientRoad(i))
                    .Also(i => datastore.city[i].nodeTile.DisableUnusedRoadNodes())
                    .Also(i => datastore.city[i].nodeTile.EstablishNodeConnections())
                    .Also(i => datastore.city[i].nodeTile.RecalculateNodeLines())
                    .Also(i => PlaceRandomDestination(i, DestinationType.COFFEE))
                    .Also(i => PlaceRandomCar(i, DestinationType.COFFEE));
                preview.lotOrigins
                    .Also(i => ConnectLotToStreet(i));
                // var hotels = preview.lotOrigins.getManyRandomElements(2).Also(i => PlaceHotel(i));
                // var shops = preview.lotOrigins.Except(hotels).Also(i => PlaceShop(i));
                // hotels.Also(i => datastore.city[i].occupier.GetComponent<Lot>().GetComponentInChildren<Generator>().SpawnPedestrian(DestinationType.COFFEE));
                nextBlockOrientation = BlockOrientations.allOrientations.Except(new List<List<Vector2Int>>() {nextBlockOrientation}).getRandomElement();
                preview.Cleanup();
            });

        datastore.inputEvents // show the block preview on hover!
            .Receive<HoverEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.BLOCK_PLACER)
            .Where(e => TileExistsAt(new Vector2Int(e.cell.x, e.cell.y)))
            .Subscribe(e => {
                RefreshBlockPreview(new Vector2Int(e.cell.x, e.cell.y));
            });

        datastore.inputEvents // clean up the block preview when hovering over water!
            .Receive<HoverEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.BLOCK_PLACER)
            .Where(e => !TileExistsAt(new Vector2Int(e.cell.x, e.cell.y)))
            .Where(_ => preview != null)
            .Subscribe(_ => preview.Cleanup());

        datastore.inputEvents // rotate the block when the user presses E or R!
            .Receive<KeyEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.BLOCK_PLACER)
            .Subscribe(e => {
                if (e.keyCode == KeyCode.R) {
                    nextBlockOrientation = nextBlockOrientation.RotateClockwise();
                    RefreshBlockPreview(new Vector2Int(e.cell.x, e.cell.y));
                }
                if (e.keyCode == KeyCode.E) {
                    nextBlockOrientation = nextBlockOrientation.RotateCounterClockwise();
                    RefreshBlockPreview(new Vector2Int(e.cell.x, e.cell.y));
                }
            });

        datastore.activeTool // clean up the preview whenever the block placer is not active!
            .Where(e => e != ToolType.BLOCK_PLACER)
            .Where(_ => preview != null)
            .Subscribe(e => {
                preview.Cleanup();
            });

        //  _           _ _     _ _
        // | |         (_) |   | (_)
        // | |__  _   _ _| | __| |_ _ __   __ _
        // | '_ \| | | | | |/ _` | | '_ \ / _` |
        // | |_) | |_| | | | (_| | | | | | (_| |
        // |_.__/ \__,_|_|_|\__,_|_|_| |_|\__, |
        //                                 __/ |
        //                                |___/

        datastore.inputEvents // click on an empty lot to add a shop
            .Receive<ClickEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.SHOP_PLACER)
            .Where(e => TileIsOccupiedByLot(e.cell.ToVec2()))
            .Subscribe(e => {
                var lot = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>();
                var lotComponents = lot.GetBuildingComponents();
                if (lotComponents.Values.All(i => i == null)) { // if this lot is empty
                    PlaceShop(e.cell.ToVec2(), datastore.activeToolColor.Value ?? DestinationType.COFFEE);
                }
            });

        datastore.inputEvents // click on an empty lot to add a hotel
            .Receive<ClickEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.HOTEL_PLACER)
            .Where(e => TileIsOccupiedByLot(e.cell.ToVec2()))
            .Subscribe(e => {
                var lot = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>();
                var lotComponents = lot.GetBuildingComponents();
                if (lotComponents.Values.All(i => i == null)) { // if this lot is empty
                    PlaceHotel(e.cell.ToVec2());
                    var generator =
                    lot.GetBuildingComponents()[typeof(Generator)].GetComponent<Generator>();
                    generator.SpawnPedestrian(DestinationType.COFFEE);
                    generator.SpawnPedestrian(DestinationType.TEA);
                    generator.SpawnPedestrian(DestinationType.BEER);
                }
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

    void RefreshBlockPreview(Vector2Int mouseCell) {
        if (preview == null) {
            preview = new PlacementPreview();
        }

        preview.lotOrigins = nextBlockOrientation
            .Select(coord => new Vector2Int(
                coord.x * datastore.lotScale.x,
                coord.y * datastore.lotScale.y
            ))
            .Select(coord => new Vector2Int(
                coord.x + mouseCell.x, coord.y + mouseCell.y
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
            lot.GetComponent<SpriteRenderer>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.green];
        } else {
            lot.GetComponent<SpriteRenderer>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.magenta];
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
        building.GetComponent<Building>().parentLot.gameObject.assignSpriteFromPath("Sprites/brblack");
    }

    void PlaceShop(Vector2Int origin, DestinationType destType) {
        var building = PlaceAnonBuilding(origin);
        var shop = building.AddAndGetComponent<Destination>();
        building.gameObject.name = "Coffee Shop";
        shop.destType = destType;
        building.GetComponent<Building>().parentLot.gameObject.GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForDestType(destType);
    }

    // If there is no road tile at the provided origin a new road tile is created
    // If a road tile is already present at the provided origin we reset the tile like it is new
    void PlaceRoad(Vector2Int origin) {
        if (!datastore.city.Keys.Contains(origin))
        {
            var road = GameObject.Instantiate(
                prefabs.road,
                datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0)),
                Quaternion.identity
            );
            datastore.city[origin] = new CityTile()
            {
                occupier = road,
                nodeTile = road.GetComponent<Tile>()
            };
        }
        else
        {
            if (datastore.city[origin].occupier.GetComponent<Tile>() != null)
            {
                Tile existingTile = datastore.city[origin].occupier.GetComponent<Tile>();
                existingTile.ResetTile();
            }
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

    void PlaceRandomCar(Vector2Int origin, DestinationType destType)
    {
        if (datastore.city.Keys.Contains(origin) && datastore.city[origin].occupier != null && datastore.city[origin].occupier.GetComponent<Tile>() != null)
        {
            if (Random.Range(0, 10) > 6)
            {
                Tile road = datastore.city[origin].occupier.GetComponent<Tile>();
                List<RoadNode> allRoadNodes = new List<RoadNode>(road.roadNodeMap.Values);
                for (int i = 0; i < allRoadNodes.Count; i++)
                {
                    RoadNode roadNode = allRoadNodes[i];
                    if (roadNode.disabled || roadNode.destType == destType)
                    {
                        continue;
                    }
                    GameObject carObj = Object.Instantiate(prefabs.car, transform);
                    carObj.transform.position = roadNode.transform.position;
                    Car car = carObj.GetComponent<Car>();
                    car.SetNewCurrentNode((RoadNode)roadNode);
                    car.homeNode = (RoadNode)roadNode;
                    car.desiredDestType = destType;
                    break;
                }
            }
        }
    }

    void PlaceRandomDestination(Vector2Int origin, DestinationType destType)
    {
        if (datastore.city.Keys.Contains(origin) && datastore.city[origin].occupier != null && datastore.city[origin].occupier.GetComponent<Tile>() != null)
        {
            if (Random.Range(0, 10) > 8)
            {
                Tile road = datastore.city[origin].occupier.GetComponent<Tile>();
                List<RoadNode> allRoadNodes = new List<RoadNode>(road.roadNodeMap.Values);
                for (int i = 0; i < allRoadNodes.Count; i++)
                {
                    RoadNode roadNode = allRoadNodes[i];
                    if (roadNode.disabled)
                    {
                        continue;
                    }
                    roadNode.destType = destType;
                }
            }
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