using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    public Node homeNode;
    public Node currentNode;
    public List<Node> itinerary = new List<Node>();
    public ShopType desiredShopType;
    public float speed = 0.1f;
    public bool headingHome = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForShopType(desiredShopType);
    }

    // Update is called once per frame
    void Update()
    {
        if (itinerary.Count > 0)
        {
            Node target = itinerary[0];
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, step);
            if (Vector2.Distance(transform.position, target.transform.position) < 0.05)
            {                
                currentNode = target;
                itinerary.RemoveAt(0);
                if (itinerary.Count == 0) {
                    if (!headingHome && ((Shop)currentNode.owningBuilding).shopType == desiredShopType) {
                        currentNode.owningBuilding.ReceivePedestrian(this);
                    } else if (headingHome && currentNode == homeNode)
                    {
                        currentNode.owningBuilding.ReceivePedestrian(this);
                    }
                
                    
                }
            }
        }
    }

    public void CalculateItinerary()
    {
        Dictionary<Node, int> scores = new Dictionary<Node, int>();
        List<Node> queue = new List<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        for (int i = 0; i < currentNode.connections.Count; i++)
        {
            Node neighbor = currentNode.connections[i];
            queue.Add(neighbor);
            scores[neighbor] = 0;
            cameFrom[neighbor] = currentNode;
            
        }

        while (queue.Count > 0)
        {
            Node curNode = queue[0];
            queue.RemoveAt(0);
            seenNodes.Add(curNode);
            for (int i = 0; i < curNode.connections.Count; i++)
            {
                Node neighbor = curNode.connections[i];
                if (!seenNodes.Contains(neighbor))
                {
                    queue.Add(neighbor);
                }
                seenNodes.Add(neighbor);
                int newScore = scores[curNode] + 1;
                if (scores.ContainsKey(neighbor))
                {
                    int curScore = scores[neighbor];
                    if (newScore < curScore)
                    {
                        scores[neighbor] = newScore;
                        cameFrom[neighbor] = curNode;
                    }
                } else
                {
                    scores[neighbor] = newScore;
                    cameFrom[neighbor] = curNode;
                }
                if ((!headingHome && neighbor.shopType == desiredShopType) || (headingHome && neighbor == homeNode))
                {
                    this.itinerary = ReconstructPath(cameFrom, neighbor);
                    return;
                }
            }
        }
        this.itinerary = new List<Node>();
    }

    private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node neighbor)
    {
        List<Node> itinerary = new List<Node>() { neighbor };
        Node current = neighbor;
        int attemptCount = 0;
        while (current != currentNode && attemptCount < 10000)
        {
            itinerary.Insert(0, cameFrom[current]);
            current = cameFrom[current];
            attemptCount++;
        }
        return itinerary;
    }
}
