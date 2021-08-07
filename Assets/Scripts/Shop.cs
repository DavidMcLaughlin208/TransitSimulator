using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public Setup setup;
    public int x;
    public int y;
    public List<Pedestrian> occupants = new List<Pedestrian>();
    public Rotation rotation;
    public GameObject exit;
    public GameObject entrance;
    public Node exitNode;
    public Node entranceNode;
    // Start is called before the first frame update
    void Awake()
    {
        setup = GameObject.Find("Setup").GetComponent<Setup>();
        exitNode = exit.GetComponent<Node>();
        exitNode.location = NodeLocation.TL;
        entranceNode = entrance.GetComponent<Node>();
        entranceNode.location = NodeLocation.TR;
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

    private NodeLocation Rotate(NodeLocation location)
    {
        return DirectionUtils.rotationMapping[rotation][location];
    }
}
