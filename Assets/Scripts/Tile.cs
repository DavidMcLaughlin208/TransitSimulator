using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    Setup setup;
    public RoadType roadType;
    public Rotation roadRotation;
    public GameObject pedTL;
    public GameObject pedTR;
    public GameObject pedBR;
    public GameObject pedBL;

    public int x;
    public int y;
    public Dictionary<PedestrianNodeLocation, Node> pedNodeMap = new Dictionary<PedestrianNodeLocation, Node>();

    void Awake()
    {
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        pedNodeMap.Add(PedestrianNodeLocation.TL, pedTL.GetComponent<Node>());
        pedNodeMap.Add(PedestrianNodeLocation.TR, pedTR.GetComponent<Node>());
        pedNodeMap.Add(PedestrianNodeLocation.BL, pedBL.GetComponent<Node>());
        pedNodeMap.Add(PedestrianNodeLocation.BR, pedBR.GetComponent<Node>());
        pedTL.GetComponent<Node>().location = PedestrianNodeLocation.TL;
        pedTR.GetComponent<Node>().location = PedestrianNodeLocation.TR;
        pedBR.GetComponent<Node>().location = PedestrianNodeLocation.BR;
        pedBL.GetComponent<Node>().location = PedestrianNodeLocation.BL;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectInterally()
    {
        List<PedestrianNodeLocation> allLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            PedestrianNodeLocation location = allLocations[i];
            Node currentNode = pedNodeMap[DirectionUtils.PedestrianUtils.Rotate(location, roadRotation)];
            List<PedestrianNodeLocation> desiredConnectionLocations = DirectionUtils.PedestrianUtils.internalConnectionMapping[roadType][location];
            for (int j = 0; j < desiredConnectionLocations.Count; j++)
            {
                PedestrianNodeLocation connectionLocation = desiredConnectionLocations[j];
                Node nodeToConnect = pedNodeMap[DirectionUtils.PedestrianUtils.Rotate(connectionLocation, roadRotation)];
                nodeToConnect.connections.Add(currentNode);
            }
        }
    }

    public void ConnectToNeighboringTiles()
    {

        List<PedestrianNodeLocation> allLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            PedestrianNodeLocation location = allLocations[i];
            List<Direction> nodeDesiredDirections = DirectionUtils.PedestrianUtils.nodeExternalConnectionDirections[location];
            for (int j = 0; j < nodeDesiredDirections.Count; j++)
            {
                Direction dir = nodeDesiredDirections[j];
                Tile neighboringTile = setup.getTile((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                if (neighboringTile != null)
                {
                    neighboringTile.ReceiveConnectionAttempt(dir, location, pedNodeMap[location].GetComponent<Node>());
                }
            }
        }

    }

    // Direction is relative to the tile the call is coming from. So if a tile is connecting to another tile
    // to its right the direction would be East
    public Node ReceiveConnectionAttempt(Direction direction, PedestrianNodeLocation location, Node externalNode)
    {
        Dictionary<PedestrianNodeLocation, PedestrianNodeLocation> locationMappingForDirection = DirectionUtils.PedestrianUtils.externalConnectionMapping[direction];
        if (locationMappingForDirection.ContainsKey(location)) {
            PedestrianNodeLocation nodeToConnectToLocation = locationMappingForDirection[location];
            if (pedNodeMap.ContainsKey(nodeToConnectToLocation))
            {
                Node nodeToConnect = pedNodeMap[nodeToConnectToLocation];
                nodeToConnect.connections.Add(externalNode);
                nodeToConnect.RecalculateLinePos();
                return nodeToConnect;
            }
        }
        return null;
    }

    public void RecalculateNodeLines()
    {
        List<PedestrianNodeLocation> allLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            PedestrianNodeLocation location = allLocations[i];
            Node currentNode = pedNodeMap[location];
            currentNode.RecalculateLinePos();
        }
    }


}
