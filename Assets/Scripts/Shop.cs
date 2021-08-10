using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : Building
{
    public Setup setup;
    public int x;
    public int y;
    public List<Pedestrian> occupants = new List<Pedestrian>();
    public Rotation rotation;
    public GameObject exit;
    public GameObject entrance;
    public PedestrianNode exitNode;
    public PedestrianNode entranceNode;
    public ShopType shopType = ShopType.NONE;
    // Start is called before the first frame update
    void Awake()
    {
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        exitNode = exit.GetComponent<PedestrianNode>();
        exitNode.location = PedestrianNodeLocation.TL;
        entranceNode = entrance.GetComponent<PedestrianNode>();
        entranceNode.location = PedestrianNodeLocation.TR;
        entranceNode.owningBuilding = this;
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

    public void ConnectToStreets()
    {
        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        Vector2 offset = DirectionUtils.directionToCoordinatesMapping[dir];
        Tile neighboringTile = setup.getTile((Vector2)transform.position + offset);
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(entranceNode.location, rotation), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(exitNode.location, rotation), exitNode);
            exitNode.connections.Add(otherNode);

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
        pedestrian.transform.position = this.exit.transform.position;
        pedestrian.headingHome = true;
        pedestrian.CalculateItinerary();
    }
}
