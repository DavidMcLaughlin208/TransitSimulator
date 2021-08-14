using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public static float brakingDistance = 0.5f;

    public RoadNode homeNode;
    public RoadNode currentNode;
    public RoadNode targetNode;
    public ShopType desiredShopType;
    public float maxSpeed = 1f;
    public float speed = 0f;
    public float acceleration = 0.01f;
    public float brakingForce = 1f;
    public bool headingHome = false;
    public List<RoadNode> itinerary = new List<RoadNode>();
    public Curve currentCurve;

    public GameObject originGO;
    public GameObject intermediateGO;
    public GameObject targetGO;
    public Vector2 previousPos;

    public struct Curve
    {
        public Vector2 originPoint;
        public Vector2 targetPoint;
        public Vector2 intermediatePoint;
        public float currentPlace;
        public float distance;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForShopType(desiredShopType);
        originGO = transform.Find("OriginPoint").gameObject;
        intermediateGO = transform.Find("IntermediatePoint").gameObject;
        targetGO = transform.Find("TargetPoint").gameObject;
        CalculateItinerary();
        currentCurve = new Curve();
        SetNewCurve();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (itinerary.Count > 0)
        {
            //originGO.transform.position = itinerary[0].transform.position;
            //targetGO.transform.position = itinerary[1].transform.position;
            //intermediateGO.transform.position = currentCurve.intermediatePoint;
            speed = CalculateSpeed(speed);
            
            transform.position = CalculateLerpedPosition(currentCurve);
            float lerpIncrease = 1 / (currentCurve.distance / speed) * Time.deltaTime;
            currentCurve.currentPlace = Mathf.Min(currentCurve.currentPlace + lerpIncrease, 1f);
            //transform.position = Vector2.MoveTowards(transform.position, target.transform.position, step);


            float turnStrength = 15;
            float offset = 90f;
            if (previousPos != (Vector2)transform.position)
            {
                Vector2 direction = previousPos - (Vector2)transform.position;
                direction.Normalize();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle + offset, Vector3.forward);
                //transform.rotation = rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnStrength);
            }

            if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.02)
            {
                SetNewCurrentNode((RoadNode) targetNode);
                SetNewCurve();
                if (itinerary.Count <= 1)
                {
                    headingHome = !headingHome;
                    CalculateItinerary();
                    SetNewCurve();
                }
                targetNode = itinerary[1];

            }
        }
        previousPos = transform.position;
    }

    public void SetNewCurve()
    {
        if (itinerary.Count < 2)
        {
            return;
        }
        currentCurve.originPoint = itinerary[0].transform.position;
        currentCurve.targetPoint = itinerary[1].transform.position;
        currentCurve.currentPlace = 0;
        
        float distance = Vector2.Distance(currentCurve.originPoint, currentCurve.targetPoint);
        currentCurve.distance = distance;
        Vector2 direction = DirectionUtils.RoadUtils.nodeLocationVectors[((RoadNode)itinerary[0]).location];
        Vector2 intermediate = currentCurve.originPoint + direction * (distance / 2);// Mathf.Sqrt(2));
        currentCurve.intermediatePoint = intermediate;
    }

    public void SetNewCurrentNode(RoadNode newNode)
    {
        if (currentNode != null)
        {
            ((RoadNode)currentNode).RemoveCar(this);
            itinerary.RemoveAt(0);
        }
        currentNode = newNode;
        ((RoadNode)currentNode).AddCar(this);
    }

    public Vector2 CalculateLerpedPosition(Curve curve)
    {
        Vector2 line1Lerp = Vector2.Lerp(currentCurve.originPoint, currentCurve.intermediatePoint, currentCurve.currentPlace);
        Vector2 line2Lerp = Vector2.Lerp(currentCurve.intermediatePoint, currentCurve.targetPoint, currentCurve.currentPlace);
        return Vector2.Lerp(line1Lerp, line2Lerp, currentCurve.currentPlace);
    }

    public float CalculateSpeed(float currentSpeed)
    {
        float calcSpeed = currentSpeed;


        List<Car> neighboringCars = currentNode.getCarsAfterCar(this);
        neighboringCars.AddRange(targetNode.cars);
        for (int i = 0; i < neighboringCars.Count; i++)
        {
            Car neighboringCar = neighboringCars[i];
            float distance = Vector2.Distance(transform.position, neighboringCar.transform.position);
            if (distance < brakingDistance)
            {
                float scaledBrakingForce = (brakingDistance - distance) * brakingForce;
                calcSpeed = Mathf.Max(calcSpeed - scaledBrakingForce, 0);
                return calcSpeed;
            }
        }
        calcSpeed = Mathf.Min(currentSpeed + acceleration, maxSpeed);
        return calcSpeed;
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
                }
                else
                {
                    scores[neighbor] = newScore;
                    cameFrom[neighbor] = curNode;
                }
                if ((!headingHome && neighbor.shopType == desiredShopType) || (headingHome && neighbor == homeNode))
                {
                    this.itinerary = ReconstructPath(cameFrom, neighbor);
                    targetNode = itinerary[1];
                    return;
                }
            }
        }
        this.itinerary = new List<RoadNode>();
    }

    private List<RoadNode> ReconstructPath(Dictionary<Node, Node> cameFrom, Node neighbor)
    {
        List<RoadNode> itinerary = new List<RoadNode>() { (RoadNode)neighbor };
        Node current = neighbor;
        int attemptCount = 0;
        while (current != currentNode && attemptCount < 10000)
        {
            itinerary.Insert(0, (RoadNode)cameFrom[current]);
            current = cameFrom[current];
            attemptCount++;
        }
        return itinerary;
    }
}
