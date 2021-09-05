using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public class Car : MonoBehaviour
{
    public static float brakingDistance = 0.7f;
    public static float targetStoppingDistance = 0.35f;
    public static int intersectionNodeLookaheadCount = 2;

    public Datastore datastore;
    public RoadNode homeNode;
    public RoadNode currentNode;
    public RoadNode targetNode;
    public DestinationType desiredDestType;
    public float maxSpeed = 1f;
    public float speed = 0f;
    private float minSpeed = 0.3f;
    private float acceleration = 1f;
    private float brakingForce = 25f;
    public bool headingHome = true;
    public List<RoadNode> itinerary = new List<RoadNode>();
    public Curve currentCurve = new Curve();

    public List<(RoadNode, bool)> intersectionsQueued = new List<(RoadNode, bool)>();
    public List<IntersectionTile> currentlyLockedTiles = new List<IntersectionTile>();

    public GameObject originGO;
    public GameObject intermediateGO;
    public GameObject targetGO;
    public Vector2 previousPos;

    public SpriteRenderer carBodySprite;

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

    public void Awake()
    {
        datastore = GameObject.Find("God").GetComponent<Datastore>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<SpriteRenderer>().color = ColorUtils.GetColorForDestType(desiredDestType);
        carBodySprite.color = ColorUtils.GetColorForDestType(desiredDestType);
        originGO = transform.Find("OriginPoint").gameObject;
        intermediateGO = transform.Find("IntermediatePoint").gameObject;
        targetGO = transform.Find("TargetPoint").gameObject;
        //CalculateItinerary();
        //SetNewCurve();

        datastore.gameEvents
            .Receive<CityChangedEvent>()
            .Subscribe(_ => {
                CalculateItinerary();
                if (currentCurve.currentPlace == 0)
                {
                    SetNewCurve();
                }
            });
    }

    // Update is called once per frame
    void Update()
    {
        if (itinerary.Count > 0)
        {
            speed = CalculateSpeed(speed);
            
            transform.position = currentCurve.GetCurrentPosition();
            float lerpIncrease = 1 / (currentCurve.distance / speed) * Time.deltaTime;
            currentCurve.currentPlace = Mathf.Min(currentCurve.currentPlace + lerpIncrease, 1f);
            if (currentNode.IsIntersectionNode() && currentlyLockedTiles.Count > 0)
            {
                DirectionUtils.IntersectionUtils.Turn turn = GetTurn(currentNode, targetNode);
                List<float> thresholds = DirectionUtils.IntersectionUtils.intersectionTileReleaseMapping[turn];
                if (thresholds.Count < currentlyLockedTiles.Count)
                {
                    //TODO:Once destinations are not in the middle of intersections (so once we implement parking lots)
                    // This should not be an issue and we can remove this conditional check. In the GetTurn function in
                    // this class we return STRAIGHT as a default if
                    // a destination is in the middle of an intersection and that can cause a mismatch here
                }
                else
                {
                    float currentThreshold = thresholds[currentlyLockedTiles.Count - 1];
                    if (currentCurve.currentPlace > currentThreshold)
                    {
                        currentNode.owningTile.ReleaseTiles(new List<IntersectionTile>() { currentlyLockedTiles[0] });
                        currentlyLockedTiles.RemoveAt(0);
                    }
                }
            }


            float turnStrength = 15;
            float offset = 90f;
            if (previousPos != (Vector2)transform.position)
            {
                Vector2 direction = previousPos - (Vector2)transform.position;
                direction.Normalize();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle + offset, Vector3.forward);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnStrength);
            }

            if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.02)
            {
                SetNewCurrentNode((RoadNode) targetNode);
                SetNewCurve();
                if (itinerary.Count <= 1)
                {
                    if (targetNode.destType == this.desiredDestType || targetNode == homeNode)
                    {
                        targetNode.owningBuilding.GetComponent<CarDestination>().ReceiveCar(this);
                        return;
                    }
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
        
        float tempDistance = Vector2.Distance(currentCurve.originPoint, currentCurve.targetPoint);
        Vector2 direction = DirectionUtils.RoadUtils.nodeLocationVectors[((RoadNode)itinerary[0]).location];
        Vector2 intermediate = currentCurve.originPoint + direction * (tempDistance / 2);// Mathf.Sqrt(2));
        currentCurve.intermediatePoint = intermediate;


        currentCurve.distance = Utils.getLengthOfQuadraticCurve(currentCurve, 20);
    }

    public void SetNewCurrentNode(RoadNode newNode)
    {
        // Leave carQueue and intersection queues related to node we are leaving
        if (currentNode != null)
        {
            ((RoadNode)currentNode).RemoveCar(this);
            if (intersectionsQueued.Count > 0 && intersectionsQueued[0].Item1 == currentNode)
            {
                currentNode.RemoveCarFromIntersectionQueue(this, currentlyLockedTiles);
                currentlyLockedTiles.Clear();
                intersectionsQueued.RemoveAt(0);
            }
            if (itinerary.Count > 0)
            {
                itinerary.RemoveAt(0);
            }
        }

        // Update current node
        currentNode = newNode;
        ((RoadNode)currentNode).AddCar(this);

        // Lookahead for intersections and place self in queue
        if (itinerary.Count >= intersectionNodeLookaheadCount && itinerary[intersectionNodeLookaheadCount - 1].IsIntersectionNode())
        {
            itinerary[intersectionNodeLookaheadCount - 1].PlaceCarInIntersectionQueue(this);
            intersectionsQueued.Add((itinerary[intersectionNodeLookaheadCount - 1], false));
        }
    }

    public float CalculateSpeed(float currentSpeed)
    {
        float calcSpeed = currentSpeed;
        bool decelerated = false;

        // Check position of all cars in the current node and next node and brake if they are too close
        List<Car> neighboringCars = currentNode.GetCarsAfterCar(this);
        neighboringCars.AddRange(targetNode.cars);

        float minDistanceForBraking = 100;
        float minDistanceForAllNearbyCars = 100;
        float nearbyCarCalcSpeed = 100;
        for (int i = 0; i < neighboringCars.Count; i++)
        {
            Car neighboringCar = neighboringCars[i];
            float distance = Vector2.Distance(transform.position, neighboringCar.transform.position);
            minDistanceForAllNearbyCars = Mathf.Min(minDistanceForAllNearbyCars, distance);
            if (distance < minDistanceForBraking && distance < brakingDistance)
            {
                minDistanceForBraking = distance;
                float scaledBrakingForce = (brakingDistance - distance) * brakingForce * Time.deltaTime;
                nearbyCarCalcSpeed = Mathf.Max(calcSpeed - scaledBrakingForce, 0);
                if (distance > targetStoppingDistance)
                {
                    nearbyCarCalcSpeed = Mathf.Max(nearbyCarCalcSpeed, minSpeed);
                } 
                decelerated = true;
            }
        }

        // If approaching an intersection the car is not cleared for calculate desired speed
        // by applying braking force
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

        // We have calculated desired speed by applying braking force based on close
        // neighboring cars and intersections, here we actually use the slower of the two speeds
        // This ensures over time the car will stop appropriately at the closer obstacle
        calcSpeed = Mathf.Min(nearbyCarCalcSpeed, approachingIntersectionCalcSpeed);


        if (!decelerated)
        {
            calcSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        
        return calcSpeed;
    }

    public void NotifyClearedForIntersection(Tile tile, List<IntersectionTile> lockedTiles)
    {
        (RoadNode, bool) intersection = intersectionsQueued.Where(t => t.Item1.owningTile == tile).ToList()[0];
        int index = intersectionsQueued.IndexOf(intersection);
        intersectionsQueued[index] = (intersection.Item1, true);
        this.currentlyLockedTiles = lockedTiles;
    }

    public DirectionUtils.IntersectionUtils.Turn GetTurn(Tile tile)
    {
        (RoadNode, bool) intersection = intersectionsQueued.Where(t => t.Item1.owningTile == tile).ToList()[0];
        int index = itinerary.IndexOf(intersection.Item1);

        // This is a temporary rule to prevent errors. Ideally the index would not be at the end of itinerary
        // meaning a destination wont be in the middle of an intersection, But for testing purposes it is right now
        // So this is just to prevent errors
        if (index == -1 || index == itinerary.Count - 1)
        {
            return DirectionUtils.IntersectionUtils.Turn.STRAIGHT;
        }

        RoadNode nextNode = itinerary[index + 1];
        return GetTurn(intersection.Item1, nextNode);
    }

    private DirectionUtils.IntersectionUtils.Turn GetTurn(RoadNode node1, RoadNode node2)
    {
        return DirectionUtils.IntersectionUtils.GetTurnTypeForNodeLocations((node1.location, node2.location));
    }

    public Direction GetDirection(Tile tile)
    {
        (RoadNode, bool) intersection = intersectionsQueued.Where(t => t.Item1.owningTile == tile).ToList()[0];
        return DirectionUtils.IntersectionUtils.locationToDirectionMapping[intersection.Item1.location];
    }

    public void CalculateItinerary()
    {
        Dictionary<Node, int> scores = new Dictionary<Node, int>();
        List<Node> queue = new List<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        Node nodeToStartFrom = targetNode != null ? targetNode : currentNode;

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
                    if (targetNode != null)
                    {
                        this.itinerary.Insert(0, currentNode);
                    } else
                    {
                        for (int p = 1; p < intersectionNodeLookaheadCount; p++)
                        {
                            if (itinerary.Count >= p)
                            {
                                if (itinerary[p].IsIntersectionNode())
                                {
                                    itinerary[p].PlaceCarInIntersectionQueue(this);
                                    this.intersectionsQueued.Add((itinerary[p], false));
                                }
                            }
                        }
                    }
                    targetNode = itinerary[1];
                    SetNewCurve();
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
