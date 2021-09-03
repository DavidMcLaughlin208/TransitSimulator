using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Tile owningTile;
    public List<Node> connections = new List<Node>();
    public LineRenderer line;
    public string color = "";
    public DestinationType destType = DestinationType.NONE;
    public Building owningBuilding = null;

    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        RecalculateLinePos();
    }

    public void RecalculateLinePos()
    {
        line.positionCount = connections.Count * 2;
        Vector3[] linePositions = new Vector3[line.positionCount];
        for (int i = 0; i < connections.Count; i++)
        {
            linePositions[i * 2] = transform.position;
            Node other = connections[i];
             linePositions[i * 2 + 1] = other.transform.position;
        }
        line.SetPositions(linePositions);
    }
}
