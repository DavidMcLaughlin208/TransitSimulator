using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotel : Building
{
    public Setup setup;
    public int x;
    public int y;
    public Rotation rotation;
    public GameObject exit;
    public GameObject entrance;
    public Node exitNode;
    public Node entranceNode;
    public List<Pedestrian> presentOccupants = new List<Pedestrian>();
    public List<Pedestrian> allOccupants = new List<Pedestrian>();
    public GameObject pedestrianPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        exitNode = exit.GetComponent<Node>();
        exitNode.location = NodeLocation.TL;
        exitNode.shopType = ShopType.NONE;
        entranceNode = entrance.GetComponent<Node>();
        entranceNode.location = NodeLocation.TR;
        entranceNode.shopType = ShopType.NONE;
        entranceNode.owningBuilding = this;

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
        Vector2 offset = DirectionUtils.directionToCoordinatesMapping[dir];
        Tile neighboringTile = setup.getTile((Vector2)transform.position + offset);
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceiveConnectionAttempt(dir, DirectionUtils.Rotate(entranceNode.location, rotation), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceiveConnectionAttempt(dir, DirectionUtils.Rotate(exitNode.location, rotation), exitNode);
            exitNode.connections.Add(otherNode);

        }
    }

    public void SpawnPedestrian(ShopType shopType)
    {
        GameObject pedestrianObj = Object.Instantiate(pedestrianPrefab, transform);
        pedestrianObj.transform.position = exitNode.transform.position;
        Pedestrian pedestrian = pedestrianObj.GetComponent<Pedestrian>();
        pedestrian.currentNode = exitNode;
        pedestrian.homeNode = entranceNode;
        pedestrian.desiredShopType = shopType;
        pedestrian.CalculateItinerary();
    }

    public override void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.transform.position = this.exitNode.transform.position;
        pedestrian.headingHome = false;
        pedestrian.currentNode = this.exitNode;
        pedestrian.CalculateItinerary();
    }
}
