using System.Data;
using System;
using UnityEngine;
using UniRx;
using System.Linq;

public class Placer : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    public void Awake()
    {
        datastore = this.gameObject.GetComponent<Datastore>();
        prefabs = this.gameObject.GetComponent<Prefabs>();
    }

    public void Start()
    {
        datastore.inputEvents.Receive<ClickEvent>().Subscribe(e =>
        {
            Debug.Log($"Clicked on {e.cell}");

            var topLeftOrigins = BlockOrientations.L
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
                topLeftOrigins.ForEach(coord => {
                    PlaceLot(coord);
                });
                topLeftOrigins.ForEach(coord => {
                    OutlineWithRoad(coord);
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

    void OutlineWithRoad(Vector2Int lotOrigin) {
        for (int x = -1; x < datastore.lotScale.x + 1; x++) {
            for (int y = -1; y < datastore.lotScale.y + 1; y++) {
                // +x and -y because we're checking from top left corner
                var coord = new Vector2Int(lotOrigin.x + x, lotOrigin.y - y);
                var validPlacement = TileExistsAt(coord) && !datastore.city.ContainsKey(coord);
                if (validPlacement) {
                    PlaceRoad(coord);
                }
            }
        }
    }

    void PlaceRoad(Vector2Int origin) {
        var road = GameObject.Instantiate(
            prefabs.road,
            datastore.validTiles.GetCellCenterWorld(new Vector3Int(origin.x, origin.y, 0)),
            Quaternion.identity
        );
        datastore.city[origin] = new CityTile() {
            occupier = road
        };
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