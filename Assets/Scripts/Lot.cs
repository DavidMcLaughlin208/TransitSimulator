using UnityEngine;

public class Lot : MonoBehaviour {
    // we can remove Rotation once we start attaching entrance and exits to any random neighboring road tile
    public Rotation rotation;
    // hide these when building == null
    public PedestrianNode exitNode;
    public PedestrianNode entranceNode;

    public void Awake () {
        exitNode = this.transform.Find("Exit").GetComponent<PedestrianNode>();
        exitNode.location = PedestrianNodeLocation.TL;
        exitNode.destType = DestinationType.NONE;

        entranceNode = this.transform.Find("Entrance").GetComponent<PedestrianNode>();
        entranceNode.location = PedestrianNodeLocation.TR;
        entranceNode.destType = DestinationType.NONE;
    }

    public void ConnectToStreet(Tile neighboringTile)
    {
        Direction dir = DirectionUtils.directionRotationMapping[rotation][Direction.NORTH];
        if (neighboringTile != null)
        {
            Node otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(entranceNode.location, rotation), entranceNode);
            entranceNode.connections.Add(otherNode);
            otherNode = neighboringTile.ReceivePedestrianNodeConnectionAttempt(dir, DirectionUtils.PedestrianUtils.Rotate(exitNode.location, rotation), exitNode);
            exitNode.connections.Add(otherNode);
        }
    }
}