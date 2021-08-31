using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Lot : MonoBehaviour, INodeConnector {
    // we can remove Rotation once we start attaching entrance and exits to any random neighboring road tile
    private Rotation backingRotation;
    public Rotation rotation
    {
        get { return backingRotation; }
        set
        {
            this.pedestrianExitNode.location = DirectionUtils.PedestrianUtils.Rotate(PedestrianNodeLocation.TL, value);
            this.pedestrianEntranceNode.location = DirectionUtils.PedestrianUtils.Rotate(PedestrianNodeLocation.TR, value);
            this.carEntranceNode.location = DirectionUtils.RoadUtils.Rotate(RoadNodeLocation.NIN, value);
            this.carExitNode.location = DirectionUtils.RoadUtils.Rotate(RoadNodeLocation.NOUT, value);
            pedNodeMap.Clear();
            roadNodeMap.Clear();
            pedNodeMap[pedestrianEntranceNode.location] = pedestrianEntranceNode;
            pedNodeMap[pedestrianExitNode.location] = pedestrianExitNode;

            roadNodeMap[carEntranceNode.location] = carEntranceNode;
            roadNodeMap[carExitNode.location] = carExitNode;
            this.transform.rotation = Quaternion.Euler(0, 0, DirectionUtils.directionToIntMapping[value]);
            this.backingRotation = value;
        }
    }
    // hide these when building == null
    public PedestrianNode pedestrianExitNode;
    public PedestrianNode pedestrianEntranceNode;
    public RoadNode carExitNode;
    public RoadNode carEntranceNode;

    public Tile connectedTile;

    public bool pedestrianConnectionsEnabled = true;
    public bool carConnectionsEnabled = false;

    public Dictionary<PedestrianNodeLocation, PedestrianNode> pedNodeMap = new Dictionary<PedestrianNodeLocation, PedestrianNode>();
    public Dictionary<RoadNodeLocation, RoadNode> roadNodeMap = new Dictionary<RoadNodeLocation, RoadNode>();

    public void Awake () {
        pedestrianExitNode = this.transform.Find("PedestrianExit").GetComponent<PedestrianNode>();
        pedestrianExitNode.location = PedestrianNodeLocation.TL;
        pedestrianExitNode.destType = DestinationType.NONE;

        pedestrianEntranceNode = this.transform.Find("PedestrianEntrance").GetComponent<PedestrianNode>();
        pedestrianEntranceNode.location = PedestrianNodeLocation.TR;
        pedestrianEntranceNode.destType = DestinationType.NONE;

        carEntranceNode = this.transform.Find("CarEntrance").GetComponent<RoadNode>();
        carEntranceNode.location = RoadNodeLocation.NIN;
        carEntranceNode.destType = DestinationType.NONE;

        carExitNode = this.transform.Find("CarExit").GetComponent<RoadNode>();
        carExitNode.location = RoadNodeLocation.NOUT;
        carExitNode.destType = DestinationType.NONE;

        pedNodeMap[pedestrianEntranceNode.location] = pedestrianEntranceNode;
        pedNodeMap[pedestrianExitNode.location] = pedestrianExitNode;

        roadNodeMap[carEntranceNode.location] = carEntranceNode;
        roadNodeMap[carExitNode.location] = carExitNode;
    }

    public void Start() {
        
    }

    public void ResetConnections() {
        pedestrianEntranceNode.connections.Clear();
        pedestrianExitNode.connections.Clear();
        carEntranceNode.connections.Clear();
        carExitNode.connections.Clear();
    }

    public void ConnectToStreet(Tile neighboringTile)
    {
        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        if (neighboringTile != null)
        {
            INodeConnector nodeConnector = neighboringTile;
            if (pedestrianConnectionsEnabled)
            {
                Node otherNode = nodeConnector.GetPedestrianNodeForConnection(dir, pedestrianEntranceNode.location, pedestrianEntranceNode);
                pedestrianEntranceNode.connections.Add(otherNode);
                //otherNode.connections.Add(pedestrianEntranceNode);
                otherNode = nodeConnector.GetPedestrianNodeForConnection(dir, pedestrianExitNode.location, pedestrianExitNode);
                pedestrianExitNode.connections.Add(otherNode);
                //otherNode.connections.Add(pedestrianExitNode);
            }

            if (carConnectionsEnabled)
            {
                nodeConnector.ReceiveRoadNodeConnectionAttempt(dir, carExitNode.location, carExitNode);
                //RoadNode externalNode = nodeConnector.GetRoadNodeForConnection(dir, carEntranceNode.location, carEntranceNode);
                //if (externalNode != null)
                //{
                //    externalNode.connections.Add(carEntranceNode);
                //}
            }
        }
    }

    Node INodeConnector.GetPedestrianNodeForConnection(Direction direction, PedestrianNodeLocation location, Node externalNode)
    {
        if (!pedestrianConnectionsEnabled || externalNode.owningTile != this.connectedTile) {
            return null;
        }
        Dictionary<PedestrianNodeLocation, PedestrianNodeLocation> locationMappingForDirection = DirectionUtils.PedestrianUtils.externalConnectionMapping[direction];
        if (locationMappingForDirection.ContainsKey(location))
        {
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
        RoadNode nodeForConnection = ((INodeConnector)this).GetRoadNodeForConnection(direction, location, externalNode);
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

    public Dictionary<Type, Component?> GetBuildingComponents() {
        return new List<Type>() {typeof(Generator), typeof(Destination), typeof(PedestrianDestination), typeof(CarDestination)}
            .ToDictionary(
                i => i,
                i => this.transform.GetComponentInChildren(i)
            );
    }
}