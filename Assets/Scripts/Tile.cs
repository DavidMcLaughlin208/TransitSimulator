using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    Setup setup;
    public RoadType roadType;
    public Rotation roadRotation;
    public GameObject tl;
    public GameObject tr;
    public GameObject br;
    public GameObject bl;
    public int x;
    public int y;
    public Dictionary<NodeLocation, Node> nodeMap = new Dictionary<NodeLocation, Node>();

    void Awake()
    {
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        nodeMap.Add(NodeLocation.TL, tl.GetComponent<Node>());
        nodeMap.Add(NodeLocation.TR, tr.GetComponent<Node>());
        nodeMap.Add(NodeLocation.BL, bl.GetComponent<Node>());
        nodeMap.Add(NodeLocation.BR, br.GetComponent<Node>());
        tl.GetComponent<Node>().location = NodeLocation.TL;
        tr.GetComponent<Node>().location = NodeLocation.TR;
        br.GetComponent<Node>().location = NodeLocation.BR;
        bl.GetComponent<Node>().location = NodeLocation.BL;
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
        List<NodeLocation> allLocations = new List<NodeLocation>(nodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            NodeLocation location = allLocations[i];
            Node currentNode = nodeMap[Rotate(location)];
            List<NodeLocation> desiredConnectionLocations = DirectionUtils.internalConnectionMapping[roadType][location];
            for (int j = 0; j < desiredConnectionLocations.Count; j++)
            {
                NodeLocation connectionLocation = desiredConnectionLocations[j];
                Node nodeToConnect = nodeMap[Rotate(connectionLocation)];
                nodeToConnect.connections.Add(currentNode);
            }
        }
    }

    public void ConnectToNeighboringTiles()
    {

        List<NodeLocation> allLocations = new List<NodeLocation>(nodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            NodeLocation location = allLocations[i];
            List<Direction> nodeDesiredDirections = DirectionUtils.nodeExternalConnectionDirections[location];
            for (int j = 0; j < nodeDesiredDirections.Count; j++)
            {
                Direction dir = nodeDesiredDirections[j];
                Tile neighboringTile = setup.getTile((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                if (neighboringTile != null)
                {
                    neighboringTile.ReceiveConnectionAttempt(dir, location, nodeMap[location].GetComponent<Node>());
                }
            }
        }

    }

    // Direction is relative to the tile the call is coming from. So if a tile is connecting to another tile
    // to its right the direction would be East
    public Node ReceiveConnectionAttempt(Direction direction, NodeLocation location, Node externalNode)
    {
        Dictionary<NodeLocation, NodeLocation> locationMappingForDirection = DirectionUtils.externalConnectionMapping[direction];
        if (locationMappingForDirection.ContainsKey(location)) {
            NodeLocation nodeToConnectToLocation = locationMappingForDirection[location];
            if (nodeMap.ContainsKey(nodeToConnectToLocation))
            {
                Node nodeToConnect = nodeMap[nodeToConnectToLocation];
                nodeToConnect.connections.Add(externalNode);
                nodeToConnect.RecalculateLinePos();
                return nodeToConnect;
            }
        }
        return null;
    }

    public void RecalculateNodeLines()
    {
        List<NodeLocation> allLocations = new List<NodeLocation>(nodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            NodeLocation location = allLocations[i];
            Node currentNode = nodeMap[location];
            currentNode.RecalculateLinePos();
        }
    }

    private NodeLocation Rotate(NodeLocation location)
    {
        return DirectionUtils.rotationMapping[roadRotation][location];
    }


}
