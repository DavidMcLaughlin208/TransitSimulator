using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{
    Datastore datastore;

    public RoadType roadType;
    public Rotation tileRotation;
    public GameObject pedTL;
    public GameObject pedTR;
    public GameObject pedBR;
    public GameObject pedBL;

    public GameObject roadNIN;
    public GameObject roadNOUT;
    public GameObject roadEIN;
    public GameObject roadEOUT;
    public GameObject roadSIN;
    public GameObject roadSOUT;
    public GameObject roadWIN;
    public GameObject roadWOUT;

    public List<Car> intersectionQueue = new List<Car>();

    public int x;
    public int y;
    public Dictionary<PedestrianNodeLocation, PedestrianNode> pedNodeMap = new Dictionary<PedestrianNodeLocation, PedestrianNode>();
    public Dictionary<RoadNodeLocation, RoadNode> roadNodeMap = new Dictionary<RoadNodeLocation, RoadNode>();

    public void Awake()
    {
        datastore = GameObject.Find("God").GetComponent<Datastore>();
        // Setup pedestrian node locations and mapping
        pedNodeMap.Add(PedestrianNodeLocation.TL, pedTL.GetComponent<PedestrianNode>());
        pedNodeMap.Add(PedestrianNodeLocation.TR, pedTR.GetComponent<PedestrianNode>());
        pedNodeMap.Add(PedestrianNodeLocation.BL, pedBL.GetComponent<PedestrianNode>());
        pedNodeMap.Add(PedestrianNodeLocation.BR, pedBR.GetComponent<PedestrianNode>());
        pedNodeMap[PedestrianNodeLocation.TL].location = PedestrianNodeLocation.TL;
        pedNodeMap[PedestrianNodeLocation.TR].location = PedestrianNodeLocation.TR;
        pedNodeMap[PedestrianNodeLocation.BR].location = PedestrianNodeLocation.BR;
        pedNodeMap[PedestrianNodeLocation.BL].location = PedestrianNodeLocation.BL;

        // Setup road node locations and mapping
        roadNodeMap.Add(RoadNodeLocation.NIN, roadNIN.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.NOUT, roadNOUT.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.EIN, roadEIN.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.EOUT, roadEOUT.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.SIN, roadSIN.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.SOUT, roadSOUT.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.WIN, roadWIN.GetComponent<RoadNode>());
        roadNodeMap.Add(RoadNodeLocation.WOUT, roadWOUT.GetComponent<RoadNode>());
        roadNodeMap[RoadNodeLocation.NIN].location = RoadNodeLocation.NIN;
        roadNodeMap[RoadNodeLocation.NOUT].location = RoadNodeLocation.NOUT;
        roadNodeMap[RoadNodeLocation.EIN].location = RoadNodeLocation.EIN;
        roadNodeMap[RoadNodeLocation.EOUT].location = RoadNodeLocation.EOUT;
        roadNodeMap[RoadNodeLocation.SIN].location = RoadNodeLocation.SIN;
        roadNodeMap[RoadNodeLocation.SOUT].location = RoadNodeLocation.SOUT;
        roadNodeMap[RoadNodeLocation.WIN].location = RoadNodeLocation.WIN;
        roadNodeMap[RoadNodeLocation.WOUT].location = RoadNodeLocation.WOUT;
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

    public void EstablishNodeConnections()
    {
        ConnectPedestrianNodesInterally();
        ConnectPedestrianNodesExternally();
        ConnectRoadNodesInternally();
        ConnectRoadNodesExternally();
    }

    public void ConnectPedestrianNodesInterally()
    {
        List<PedestrianNodeLocation> allLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            PedestrianNodeLocation location = allLocations[i];
            Node currentNode = pedNodeMap[DirectionUtils.PedestrianUtils.Rotate(location, tileRotation)];
            List<PedestrianNodeLocation> desiredConnectionLocations = DirectionUtils.PedestrianUtils.internalConnectionMapping[roadType][location];
            for (int j = 0; j < desiredConnectionLocations.Count; j++)
            {
                PedestrianNodeLocation connectionLocation = desiredConnectionLocations[j];
                Node nodeToConnect = pedNodeMap[DirectionUtils.PedestrianUtils.Rotate(connectionLocation, tileRotation)];
                nodeToConnect.connections.Add(currentNode);
            }
        }
    }

    public void ConnectPedestrianNodesExternally()
    {
        List<PedestrianNodeLocation> allLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            PedestrianNodeLocation location = allLocations[i];
            List<Direction> nodeDesiredDirections = DirectionUtils.PedestrianUtils.nodeExternalConnectionDirections[location];
            for (int j = 0; j < nodeDesiredDirections.Count; j++)
            {
                Direction dir = nodeDesiredDirections[j];
                Tile neighboringTile = GetTileFromDatastore((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                if (neighboringTile != null)
                {
                    neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, location, pedNodeMap[location].GetComponent<PedestrianNode>());
                }
            }
        }
    }

    public void ConnectRoadNodesInternally()
    {
        List<RoadNodeLocation> allRoadLocations = new List<RoadNodeLocation>(roadNodeMap.Keys);
        for (int i = 0; i < allRoadLocations.Count; i++)
        {
            RoadNodeLocation location = allRoadLocations[i];
            Node currentNode = roadNodeMap[DirectionUtils.RoadUtils.Rotate(location, tileRotation)];
            if (DirectionUtils.RoadUtils.internalConnectionMapping[roadType].ContainsKey(location))
            {
                List<RoadNodeLocation> desiredConnectionLocations = DirectionUtils.RoadUtils.internalConnectionMapping[roadType][location];
                for (int j = 0; j < desiredConnectionLocations.Count; j++)
                {
                    RoadNodeLocation connectionLocation = desiredConnectionLocations[j];
                    Node nodeToConnect = roadNodeMap[DirectionUtils.RoadUtils.Rotate(connectionLocation, tileRotation)];
                    currentNode.connections.Add(nodeToConnect);

                }
            }
        }
    }

    public void ConnectRoadNodesExternally()
    {
        List<RoadNodeLocation> allLocations = new List<RoadNodeLocation>(roadNodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            RoadNodeLocation location = allLocations[i];
            RoadNode currentRoadNode = roadNodeMap[location].GetComponent<RoadNode>();
            if (currentRoadNode.disabled)
            {
                continue;
            }
            if (DirectionUtils.RoadUtils.nodeExternalConnectionDirections.ContainsKey(location))
            {
                List<Direction> nodeDesiredDirections = DirectionUtils.RoadUtils.nodeExternalConnectionDirections[location];
                for (int j = 0; j < nodeDesiredDirections.Count; j++)
                {
                    Direction dir = nodeDesiredDirections[j];
                    Tile neighboringTile = GetTileFromDatastore((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                    if (neighboringTile != null)
                    {
                        neighboringTile.ReceiveRoadNodeConnectionAttempt(dir, location, currentRoadNode);
                    }
                }
            }
        }
    }

    public void RemoveAllNodeConnections()
    {
        List<RoadNode> allRoadNodes = new List<RoadNode>(roadNodeMap.Values);
        for (int i = 0; i < allRoadNodes.Count; i++)
        {
            RoadNode roadNode = allRoadNodes[i];
            roadNode.connections.Clear();
        }

        List<PedestrianNode> allPedNodes = new List<PedestrianNode>(pedNodeMap.Values);
        for (int i = 0; i < allPedNodes.Count; i++)
        {
            PedestrianNode pedestrianNode = allPedNodes[i];
            pedestrianNode.connections.Clear();
        }
    }

    public void DisableUnusedRoadNodes()
    {
        List<RoadNodeLocation> disabledLocations = DirectionUtils.RoadUtils.disabledNodeMapping[roadType];
        for (int i = 0; i < disabledLocations.Count; i++)
        {
            RoadNodeLocation location = disabledLocations[i];
            RoadNodeLocation rotatedLocation = DirectionUtils.RoadUtils.Rotate(location, tileRotation);
            roadNodeMap[rotatedLocation].disabled = true;
        }
    }

    public void SetAllRoadNodesEnabled()
    {
        List<RoadNode> roadNodes = new List<RoadNode>(roadNodeMap.Values);
        for (int i = 0; i < roadNodes.Count; i++)
        {
            RoadNode roadNode = roadNodes[i];
            roadNode.disabled = false;
        }
    }

    public void ResetTile()
    {
        RemoveAllNodeConnections();
        SetAllRoadNodesEnabled();
    }

    // Direction is relative to the tile the call is coming from. So if a tile is connecting to another tile
    // to its right the direction would be East
    public Node ReceivePedestrianNodeConnectionAttempt(Direction direction, PedestrianNodeLocation location, Node externalNode)
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

    public Node ReceiveRoadNodeConnectionAttempt(Direction direction, RoadNodeLocation location, Node externalNode)
    {
        Dictionary<RoadNodeLocation, RoadNodeLocation> locationMappingForDirection = DirectionUtils.RoadUtils.externalConnectionMapping[direction];
        if (locationMappingForDirection.ContainsKey(location))
        {
            RoadNodeLocation nodeToConnectToLocation = locationMappingForDirection[location];
            RoadNode roadNodeToConnectTo = roadNodeMap[nodeToConnectToLocation];
            if (roadNodeToConnectTo.disabled)
            {
                return null;
            }
            externalNode.connections.Add(roadNodeToConnectTo);
        }

        return null;
    }

    public void RecalculateNodeLines()
    {
        List<PedestrianNodeLocation> allPedLocations = new List<PedestrianNodeLocation>(pedNodeMap.Keys);
        for (int i = 0; i < allPedLocations.Count; i++)
        {
            PedestrianNodeLocation location = allPedLocations[i];
            Node currentNode = pedNodeMap[location];
            currentNode.RecalculateLinePos();
        }
        List<RoadNodeLocation> allRoadLocations = new List<RoadNodeLocation>(roadNodeMap.Keys);
        for (int i = 0; i < allRoadLocations.Count; i++)
        {
            RoadNodeLocation location = allRoadLocations[i];
            Node currentNode = roadNodeMap[location];
            currentNode.RecalculateLinePos();
        }

        
    }

    public void CalculateRoadType(Dictionary<Direction, bool> neighbors)
    {
        List<Direction> missingTiles = new List<Direction>(neighbors.Keys).Where(key => !neighbors[key]).ToList();

        DirectionUtils.RoadUtils.RoadTypeAndRotation roadTypeAndRotation = DirectionUtils.RoadUtils.GetRoadTypeAndRotationForMissingNeighbors(missingTiles);
        this.roadType = roadTypeAndRotation.roadType;
        this.tileRotation = roadTypeAndRotation.rotation;
    }

    public void PlaceCarInIntersectionQueue(Car car)
    {
        if (intersectionQueue.IndexOf(car) == -1)
        {
            intersectionQueue.Add(car);
        }
    }

    public void RemoveCarFromIntersectionQueue(Car car)
    {
        intersectionQueue.Remove(car);
    }

    public bool IsCarClearedForIntersection(Car car)
    {
        return intersectionQueue.IndexOf(car) == 0;        
    }

    public bool IsIntersection()
    {
        return roadType == RoadType.Intersection || roadType == RoadType.TJunction;
    }


}
