using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Lot : MonoBehaviour {
    // we can remove Rotation once we start attaching entrance and exits to any random neighboring road tile
    public Rotation rotation;
    // hide these when building == null
    public PedestrianNode pedestrianExitNode;
    public PedestrianNode pedestrianEntranceNode;
    public RoadNode carExitNode;
    public RoadNode carEntranceNode;

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

        carExitNode = this.transform.Find("CarEntrance").GetComponent<RoadNode>();
        carExitNode.location = RoadNodeLocation.NOUT;
        carExitNode.destType = DestinationType.NONE;
    }

    public void Start() {
        this.transform.rotation = Quaternion.Euler(0, 0, DirectionUtils.directionToIntMapping[rotation]);
    }

    public void ResetConnections() {
        pedestrianEntranceNode.connections.Clear();
        pedestrianExitNode.connections.Clear();
    }

    public void ConnectToStreet(Tile neighboringTile)
    {
        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(pedestrianEntranceNode.location, rotation), pedestrianEntranceNode);
            pedestrianEntranceNode.connections.Add(otherNode);
            otherNode.connections.Add(pedestrianEntranceNode);
            otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(pedestrianExitNode.location, rotation), pedestrianExitNode);
            pedestrianExitNode.connections.Add(otherNode);
            otherNode.connections.Add(pedestrianExitNode);


            neighboringTile.ReceiveRoadNodeConnectionAttempt(dir, DirectionUtils.RoadUtils.Rotate(carExitNode.location, rotation), carExitNode);
            RoadNode externalNode = neighboringTile.GetRoadNodeForConnection(dir, DirectionUtils.RoadUtils.Rotate(carEntranceNode.location, rotation), carEntranceNode);
            if (externalNode != null)
            {
                externalNode.connections.Add(carEntranceNode);
            }
        }
    }

    public Dictionary<Type, Component?> GetBuildingComponents() {
        return new List<Type>() {typeof(Generator), typeof(Destination)}
            .ToDictionary(
                i => i,
                i => this.transform.GetComponentInChildren(i)
            );
    }
}