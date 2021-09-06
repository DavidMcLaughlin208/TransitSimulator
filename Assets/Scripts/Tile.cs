using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour, INodeConnector
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

    // Intersection related datastructures
    public Dictionary<Direction, List<Car>> intersectionQueue = new Dictionary<Direction, List<Car>>();
    public Direction activeIntersectionDirection = Direction.NORTH;
    public bool intersectionLocked = false;
    //public List<Car> carsWithIntersectionLock = new List<Car>();
    public Dictionary<IntersectionTile, bool> intersectionInnerTileLocks = new Dictionary<IntersectionTile, bool>();

    public Vector2Int coordinateLocation;
  
    public Dictionary<PedestrianNodeLocation, PedestrianNode> pedNodeMap = new Dictionary<PedestrianNodeLocation, PedestrianNode>();
    public Dictionary<RoadNodeLocation, RoadNode> roadNodeMap = new Dictionary<RoadNodeLocation, RoadNode>();

    public List<Car> carsLockingIntersection = new List<Car>();

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

        // Setup queues for intersection coming from each direction
        intersectionQueue.Add(Direction.NORTH, new List<Car>());
        intersectionQueue.Add(Direction.EAST, new List<Car>());
        intersectionQueue.Add(Direction.SOUTH, new List<Car>());
        intersectionQueue.Add(Direction.WEST, new List<Car>());

        intersectionInnerTileLocks.Add(IntersectionTile.TL, false);
        intersectionInnerTileLocks.Add(IntersectionTile.TR, false);
        intersectionInnerTileLocks.Add(IntersectionTile.BL, false);
        intersectionInnerTileLocks.Add(IntersectionTile.BR, false);
    }

    public void Update()
    {
        // Sort Cars by distance
        List<Direction> allDirections = new List<Direction>(intersectionQueue.Keys);
        for (int i = 0; i < allDirections.Count; i++)
        {
            Direction dir = allDirections[i];
            List<Car> queue = intersectionQueue[dir];
            queue.Sort(CompareCarsByDistance);
        }

        // Notify cars cleared for intersection
        for (int i = 0; i < intersectionQueue.Keys.Count; i++)
        {
            List<Car> currentQueue = intersectionQueue[activeIntersectionDirection];
            if (currentQueue.Count == 0)
            {
                CycleActiveIntersectionDirection();
                continue;
            }
            Car firstCar = currentQueue[0];
            if (carsLockingIntersection.Contains(firstCar))
            {
                CycleActiveIntersectionDirection();
                continue;
            }
                
            intersectionLocked = true;
            DirectionUtils.IntersectionUtils.Turn turnType = firstCar.GetTurn(this);
            Direction dir = firstCar.GetDirection(this);
            List<IntersectionTile> desiredTilesToLock = DirectionUtils.IntersectionUtils.GetTileLockForTurnTypeAndDirection(turnType, dir);
            int alreadyLockedTiles = desiredTilesToLock.Where(intersectionTile => intersectionInnerTileLocks[intersectionTile]).Count();
            if (alreadyLockedTiles > 0)
            {
                CycleActiveIntersectionDirection();
                break;
            }
            else
            {
                desiredTilesToLock.ForEach(intersectionTile => intersectionInnerTileLocks[intersectionTile] = true);
                firstCar.NotifyClearedForIntersection(this, desiredTilesToLock);
                carsLockingIntersection.Add(firstCar);
            }
                


            CycleActiveIntersectionDirection();
            break;
        }

        // Lock inner tiles

        // notify additional cars for simultaneous intersection clearance
    }

    public void CycleActiveIntersectionDirection()
    {
        int index = DirectionUtils.allDirections.IndexOf(activeIntersectionDirection);
        if (index == DirectionUtils.allDirections.Count - 1)
        {
            activeIntersectionDirection = DirectionUtils.allDirections[0];
        } else
        {
            activeIntersectionDirection = DirectionUtils.allDirections[index + 1];
        }
    }

    public int CompareCarsByDistance(Car left, Car right)
    {
        float leftDistance = Vector3.Distance(left.transform.position, this.transform.position);
        float rightDistance = Vector3.Distance(right.transform.position, this.transform.position);
        if (leftDistance == rightDistance)
        {
            return 0;
        } else if (leftDistance > rightDistance)
        {
            return 1;
        } else
        {
            return -1;
        }
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

    CityTile GetCityTileFromDatastore(Vector2 coord)
    {
        var tileCoord3 = datastore.validTiles.WorldToCell(coord);
        var tileCoord2 = new Vector2Int(tileCoord3.x, tileCoord3.y);
        if (datastore.city.ContainsKey(tileCoord2))
        {
            return datastore.city[tileCoord2];
        }
        else
        {
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
                CityTile neighboringTile = GetCityTileFromDatastore((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                if (neighboringTile == null)
                {
                    continue;
                }
                INodeConnector nodeConnector = null;
                if (neighboringTile.occupier.GetComponent<Lot>() != null)
                {
                    nodeConnector = neighboringTile.occupier.GetComponent<Lot>();
                }
                if (neighboringTile.nodeTile != null)
                {
                    nodeConnector = neighboringTile.nodeTile;
                }

                if (nodeConnector != null)
                {
                    var connectingNode =
                        nodeConnector.GetPedestrianNodeForConnection(dir, location, pedNodeMap[location].GetComponent<PedestrianNode>());
                    if (connectingNode != null)
                    {
                        pedNodeMap[location].GetComponent<PedestrianNode>().connections.Add(connectingNode);
                    }
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
                    CityTile neighboringTile = GetCityTileFromDatastore((Vector2)transform.position + DirectionUtils.directionToCoordinatesMapping[dir]);
                    if (neighboringTile == null)
                    {
                        continue;
                    }
                    INodeConnector nodeConnector = null;
                    if (neighboringTile.occupier.GetComponent<Lot>() != null)
                    {
                        nodeConnector = neighboringTile.occupier.GetComponent<Lot>();
                    }
                    if (neighboringTile.nodeTile != null)
                    {
                        nodeConnector = neighboringTile.nodeTile;
                    }

                    if (nodeConnector != null)
                    {
                        nodeConnector.ReceiveRoadNodeConnectionAttempt(dir, location, currentRoadNode);
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
    Node INodeConnector.GetPedestrianNodeForConnection(Direction direction, PedestrianNodeLocation location, Node externalNode)
    {
        Dictionary<PedestrianNodeLocation, PedestrianNodeLocation> locationMappingForDirection = DirectionUtils.PedestrianUtils.externalConnectionMapping[direction];
        if (locationMappingForDirection.ContainsKey(location)) {
            PedestrianNodeLocation nodeToConnectToLocation = locationMappingForDirection[location];
            if (pedNodeMap.ContainsKey(nodeToConnectToLocation))
            {
                Node nodeToConnect = pedNodeMap[nodeToConnectToLocation];
                // nodeToConnect.connections.Add(externalNode);
                // nodeToConnect.RecalculateLinePos();
                return nodeToConnect;
            }
        }
        return null;
    }

    void INodeConnector.ReceiveRoadNodeConnectionAttempt(Direction direction, RoadNodeLocation location, Node externalNode)
    {
        RoadNode nodeForConnection = ((INodeConnector) this).GetRoadNodeForConnection(direction, location, externalNode);
        if (nodeForConnection != null)
        {
            externalNode.connections.Add(nodeForConnection);
        }
    }

    RoadNode INodeConnector.GetRoadNodeForConnection(Direction direction, RoadNodeLocation location, Node externalNode) 
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
            return roadNodeToConnectTo;
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

    public void PlaceCarInIntersectionQueue(RoadNode node, Car car)
    {
        List<Car> directionalQueue = intersectionQueue[locationToDirectionMapping[node.location]];
        if (directionalQueue.IndexOf(car) == -1)
        {
            directionalQueue.Add(car);
        }
    }

    public void RemoveCarFromIntersectionQueue(RoadNode node, Car car, List<IntersectionTile> tilesToRelease)
    {
        List<Car> directionalQueue = intersectionQueue[locationToDirectionMapping[node.location]];
        directionalQueue.Remove(car);
        intersectionLocked = false;
        tilesToRelease.ForEach(intersectionTile => intersectionInnerTileLocks[intersectionTile] = false);
        carsLockingIntersection.Remove(car);
    }

    public void ReleaseTiles(List<IntersectionTile> tilesToRelease)
    {
        tilesToRelease.ForEach(intersectionTile => intersectionInnerTileLocks[intersectionTile] = false);
    }

    public bool IsIntersection()
    {
        return roadType == RoadType.Intersection || roadType == RoadType.TJunction;
    }

    public static Dictionary<RoadNodeLocation, Direction> locationToDirectionMapping = new Dictionary<RoadNodeLocation, Direction>()
    {
        {RoadNodeLocation.NIN, Direction.NORTH}, {RoadNodeLocation.EIN, Direction.EAST}, {RoadNodeLocation.SIN, Direction.SOUTH}, {RoadNodeLocation.WIN, Direction.WEST}
    };


}
