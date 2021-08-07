using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotel : MonoBehaviour
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
        Vector2 offset = DirectionUtils.directionToCoordinates[dir];
        Tile neighboringTile = setup.getTile((Vector2)transform.position + offset);
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceiveConnectionAttempt(dir, Rotate(entranceNode.location), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceiveConnectionAttempt(dir, Rotate(exitNode.location), exitNode);
            exitNode.connections.Add(otherNode);

        }
    }

    public void SpawnPedestrian(ShopType shopType)
    {
        GameObject pedestrianObj = Object.Instantiate(pedestrianPrefab, exitNode.transform);
        Pedestrian pedestrian = pedestrianObj.GetComponent<Pedestrian>();
        pedestrian.currentNode = exitNode;
        pedestrian.homeNode = entranceNode;
        pedestrian.desiredShopType = shopType;
        pedestrian.CalculateItinerary();
    }

    private NodeLocation Rotate(NodeLocation location)
    {
        return DirectionUtils.rotationMapping[rotation][location];
    }
}
