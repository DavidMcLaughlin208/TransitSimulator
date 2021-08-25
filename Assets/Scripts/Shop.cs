using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : Building
{
    Datastore datastore;
    Prefabs prefabs;
    Setup setup;

    public int x;
    public int y;
    public List<Pedestrian> occupants = new List<Pedestrian>();
    public Rotation rotation;
    public PedestrianNode exitNode;
    public PedestrianNode entranceNode;
    public RoadNode carEntranceNode;
    public RoadNode carExitNode;
    public ShopType shopType = ShopType.NONE;
    // Start is called before the first frame update
    void Awake()
    {
        //datastore = GameObject.Find("God").GetComponent<Datastore>();
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        //prefabs = GameObject.Find("God").GetComponent<Prefabs>();
        exitNode = this.transform.Find("Exit").GetComponent<PedestrianNode>();
        exitNode.location = PedestrianNodeLocation.TL;
        entranceNode = this.transform.Find("Entrance").GetComponent<PedestrianNode>();
        entranceNode.location = PedestrianNodeLocation.TR;
        entranceNode.owningBuilding = this;

        //carEntranceNode = this.transform.Find("CarEntrance").GetComponent<RoadNode>();
        //carEntranceNode.shopType = this.shopType;
        //carExitNode = this.transform.Find("CarExit").GetComponent<RoadNode>();
    }

    void Start()
    {
        this.transform.rotation = Quaternion.Euler(0, 0, DirectionUtils.directionToIntMapping[rotation]);
        GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForShopType(shopType);
    }

    // Update is called once per frame
    void Update()
    {

    }

    Tile GetTileFromDatastore(Vector2 coord) {
        var tileCoord3 = datastore.validTiles.WorldToCell(coord);
        var tileCoord2 = new Vector2Int(tileCoord3.x, tileCoord3.y);
        if (datastore.city.ContainsKey(tileCoord2)) {
            return datastore.city[tileCoord2].nodeTile;
        } else {
            return null;
        }
    }

    public void ConnectToStreets()
    {
        //Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        //Vector2 offset = DirectionUtils.directionToCoordinatesMapping[dir] * datastore.lotScale * 1.01f / 2f;
        //Tile neighboringTile = GetTileFromDatastore((Vector2)transform.position + offset);

        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        Vector2 offset = DirectionUtils.directionToCoordinatesMapping[dir];
        Tile neighboringTile = setup.getTile((Vector2)transform.position + offset);
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(entranceNode.location, rotation), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(exitNode.location, rotation), exitNode);
            exitNode.connections.Add(otherNode);

            //neighboringTile.ReceiveRoadNodeConnectionAttempt(dir, DirectionUtils.RoadUtils.Rotate(carExitNode.location, rotation), carExitNode);
            //RoadNode streetEntranceConnectionNode = neighboringTile.roadNodeMap
            //this.carExitNode.connections.Add(streetEntranceConnectionNode);

        }
    }

    public void setShopType(ShopType shopType)
    {
        this.shopType = shopType;
        entranceNode.shopType = shopType;
    }

    public override void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.currentNode = this.exitNode;
        pedestrian.transform.position = this.exitNode.transform.position;
        pedestrian.headingHome = true;
        pedestrian.CalculateItinerary();
    }
}
