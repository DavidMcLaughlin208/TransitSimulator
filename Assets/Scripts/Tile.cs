using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    Setup setup;
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

    public void ConnectToNeighboringTiles()
    {

        //Tile neighboringTile = setup.getTile((Vector2)transform.position + DirectionUtils.directionToCoordinates[Direction.NORTH]);
        //if (neighboringTile != null)
        //{
        //    List<Direction> connectDirs = DirectionUtils.nodeConnectionDirections[]
        //    neighboringTile.ReceiveConnectionAttempt(Direction.NORTH, NodeLocation.TR, tr.GetComponent<Node>());
        //    neighboringTile.ReceiveConnectionAttempt(Direction.NORTH, NodeLocation.TR, tr.GetComponent<Node>());
        //}



        //
        List<NodeLocation> allLocations = new List<NodeLocation>(nodeMap.Keys);
        for (int i = 0; i < allLocations.Count; i++)
        {
            NodeLocation location = allLocations[i];
            List<Direction> nodeDesiredDirections = DirectionUtils.nodeConnectionDirections[location];
            for (int j = 0; j < nodeDesiredDirections.Count; j++)
            {
                Direction dir = nodeDesiredDirections[j];
                Tile neighboringTile = setup.getTile((Vector2)transform.position + DirectionUtils.directionToCoordinates[dir]);
                if (neighboringTile != null)
                {
                    neighboringTile.ReceiveConnectionAttempt(dir, location, nodeMap[location].GetComponent<Node>());
                }
            }
        }

    }

    public void ReceiveConnectionAttempt(Direction direction, NodeLocation location, Node node)
    {
        Dictionary<NodeLocation, NodeLocation> locationMapping = DirectionUtils.connectionMapping[direction];
        if (locationMapping.ContainsKey(location)) {
            NodeLocation connectLocation = locationMapping[location];
            if (nodeMap.ContainsKey(connectLocation))
            {
                Node nodeToConnect = nodeMap[connectLocation];
                nodeToConnect.connections.Add(node);
            }
        }
        
        
       
    }


}
