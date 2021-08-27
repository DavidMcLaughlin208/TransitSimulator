using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Car : MonoBehaviour
{
    public static float brakingDistance = 0.7f;

    public RoadNode homeNode;
    public RoadNode currentNode;
    public RoadNode targetNode;
    public DestinationType desiredDestType;
    public float maxSpeed = 1f;
    public float speed = 0f;
    private float minSpeed = 0.3f;
    private float acceleration = 1f;
    private float brakingForce = 25f;
    public bool headingHome = false;
    public List<RoadNode> itinerary = new List<RoadNode>();
    public Curve currentCurve;

    public List<(RoadNode, bool)> intersectionsQueued = new List<(RoadNode, bool)>();

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

        public Vector2 GetCurrentPosition()
        {
            Vector2 line1Lerp = Vector2.Lerp(originPoint, intermediatePoint, currentPlace);
            Vector2 line2Lerp = Vector2.Lerp(intermediatePoint, targetPoint, currentPlace);
            return Vector2.Lerp(line1Lerp, line2Lerp, currentPlace);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForDestType(desiredDestType);
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
            
            transform.position = currentCurve.GetCurrentPosition();
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
                if (targetNode.IsIntersectionNode())
                {
                    
                }

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
        
        float tempDistance = Vector2.Distance(currentCurve.originPoint, currentCurve.targetPoint);
        Vector2 direction = DirectionUtils.RoadUtils.nodeLocationVectors[((RoadNode)itinerary[0]).location];
        Vector2 intermediate = currentCurve.originPoint + direction * (tempDistance / 2);// Mathf.Sqrt(2));
        currentCurve.intermediatePoint = intermediate;


        currentCurve.distance = Utils.getLengthOfQuadraticCurve(currentCurve, 20);
    }

    public void SetNewCurrentNode(RoadNode newNode)
    {
        if (currentNode != null)
        {
            ((RoadNode)currentNode).RemoveCar(this);
            if (intersectionsQueued.Count > 0 && intersectionsQueued[0].Item1 == currentNode)
            {
                currentNode.RemoveCarFromIntersectionQueue(this);
                intersectionsQueued.RemoveAt(0);
            }
            itinerary.RemoveAt(0);
        }
        currentNode = newNode;
        ((RoadNode)currentNode).AddCar(this);

        if (itinerary.Count >= 3 && itinerary[2].IsIntersectionNode())
        {
            itinerary[2].PlaceCarInIntersectionQueue(this);
            intersectionsQueued.Add((itinerary[2], false));
        }
    }

    public float CalculateSpeed(float currentSpeed)
    {
        float calcSpeed = currentSpeed;
        bool decelerated = false;


        List<Car> neighboringCars = currentNode.GetCarsAfterCar(this);
        neighboringCars.AddRange(targetNode.cars);
        float minDistance = 100;
        float nearbyCarCalcSpeed = 100f;
        for (int i = 0; i < neighboringCars.Count; i++)
        {
            Car neighboringCar = neighboringCars[i];
            float distance = Vector2.Distance(transform.position, neighboringCar.transform.position);
            if (distance < minDistance && distance < brakingDistance)
            {
                minDistance = distance;
                float scaledBrakingForce = (brakingDistance - distance) * brakingForce * Time.deltaTime;
                nearbyCarCalcSpeed = Mathf.Max(calcSpeed - scaledBrakingForce, 0);
                if (distance > 0.35)
                {
                    nearbyCarCalcSpeed = Mathf.Max(nearbyCarCalcSpeed, minSpeed);
                }
                decelerated = true;
            }
        }
        float approachingIntersectionCalcSpeed = 100f;
        for (int i = 0; i < intersectionsQueued.Count; i++)
        {
            
            RoadNode intersectionNode = intersectionsQueued[0].Item1;
            bool clearedForIntersection = intersectionsQueued[0].Item2;
            if (!clearedForIntersection)
            {
                float distance = Vector2.Distance(transform.position, intersectionNode.transform.position);
                if (distance < brakingDistance)
                {
                    float scaledBrakingForce = (brakingDistance - distance) * brakingForce * Time.deltaTime;
                    approachingIntersectionCalcSpeed = Mathf.Max(calcSpeed - scaledBrakingForce, 0);
                    decelerated = true;
                    Vector3 targetStoppingLocation = DirectionUtils.RoadUtils.nodeLocationVectors[intersectionNode.location] * -1 * 0.15f + (Vector2) intersectionNode.transform.position;
                    if (Vector3.Distance(transform.position, targetStoppingLocation) > 0.2)
                    {
                        approachingIntersectionCalcSpeed = Mathf.Max(approachingIntersectionCalcSpeed, minSpeed);
                    }
                }
            }
        }
        calcSpeed = Mathf.Min(nearbyCarCalcSpeed, approachingIntersectionCalcSpeed);
        
        
        if (!decelerated)
        {
            calcSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        
        return calcSpeed;
    }

    public void NotifyClearedForIntersection(Tile tile)
    {
        (RoadNode, bool) intersection = intersectionsQueued.Where(t => t.Item1.owningTile == tile).ToList()[0];
        int index = intersectionsQueued.IndexOf(intersection);
        intersectionsQueued[index] = (intersection.Item1, true);
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
                if ((!headingHome && neighbor.destType == desiredDestType) || (headingHome && neighbor == homeNode))
                {
                    this.itinerary = ReconstructPath(cameFrom, neighbor);
                    targetNode = itinerary[1];
                    for (int m = 0; m < 2; m++)
                    {
                        if (this.itinerary[m].IsIntersectionNode())
                        {
                            itinerary[m].PlaceCarInIntersectionQueue(this);
                            intersectionsQueued.Add((itinerary[m], false));
                        }
                    }

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
