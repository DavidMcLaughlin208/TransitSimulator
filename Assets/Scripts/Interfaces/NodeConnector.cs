using System;
using UnityEngine;

public interface INodeConnector
{
    // Direction is relative to the tile the call is coming from. So if a tile is connecting to another tile
    // to its right the direction would be East
    public Node GetPedestrianNodeForConnection(Direction direction, PedestrianNodeLocation location, Node externalNode);

    public void ReceiveRoadNodeConnectionAttempt(Direction direction, RoadNodeLocation location, Node externalNode);

    public RoadNode GetRoadNodeForConnection(Direction direction, RoadNodeLocation location, Node externalNode);
}
