using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Componentize!
// Spawner (hotel)
// Queuer (shop)
// Destination (shop + landmark + house)
// Transporter (ferries and trains)
public class Hotel : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    public int x;
    public int y;
    public Rotation rotation;
    public PedestrianNode exitNode;
    public PedestrianNode entranceNode;
    public List<Pedestrian> presentOccupants = new List<Pedestrian>();
    public List<Pedestrian> allOccupants = new List<Pedestrian>();
    public GameObject pedestrianPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        datastore = GameObject.Find("God").GetComponent<Datastore>();
        prefabs = GameObject.Find("God").GetComponent<Prefabs>();
        exitNode = this.transform.Find("Exit").GetComponent<PedestrianNode>();
        exitNode.location = PedestrianNodeLocation.TL;
        // exitNode.shopType = ShopType.NONE;
        entranceNode = this.transform.Find("Entrance").GetComponent<PedestrianNode>();
        entranceNode.location = PedestrianNodeLocation.TR;
        // entranceNode.shopType = ShopType.NONE;
        entranceNode.owningBuilding = this.GetComponent<Building>();
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

    void Start()
    {
        this.transform.rotation = Quaternion.Euler(0, 0, DirectionUtils.directionToIntMapping[rotation]);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ConnectToStreets()
    {
        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        Vector2 offset = DirectionUtils.directionToCoordinatesMapping[dir] * datastore.lotScale * 1.01f / 2f;
        Tile neighboringTile = GetTileFromDatastore((Vector2)transform.position + offset);
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(entranceNode.location, rotation), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(exitNode.location, rotation), exitNode);
            exitNode.connections.Add(otherNode);

        }
    }

    public void SpawnPedestrian(DestinationType destinationType)
    {
        GameObject pedestrianObj = Object.Instantiate(prefabs.pedestrian, transform);
        pedestrianObj.transform.position = exitNode.transform.position;
        Pedestrian pedestrian = pedestrianObj.GetComponent<Pedestrian>();
        pedestrian.currentNode = exitNode;
        pedestrian.homeNode = entranceNode;
        pedestrian.desiredDestType = destinationType;
        pedestrian.CalculateItinerary();
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.transform.position = this.exitNode.transform.position;
        pedestrian.headingHome = false;
        pedestrian.currentNode = this.exitNode;
        pedestrian.CalculateItinerary();
    }
}
