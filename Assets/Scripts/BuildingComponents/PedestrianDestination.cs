using UnityEngine;
using System.Collections;

public class PedestrianDestination : Destination
{
    public CarDestination attachedParkingLot;

    // Use this for initialization
    void Start()
    {
        lot.carConnectionsEnabled = false;
    }

    public void Update()
    {
        if (!pedServicePending && pedQueue.Count > 0)
        {
            StartCoroutine(QueueForFrames(pedQueue.Dequeue(), datastore.baseQueueTime));
        }
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedQueue.Enqueue(pedestrian);
    }

    IEnumerator QueueForFrames(Pedestrian pedestrian, int seconds)
    {
        pedServicePending = true;
        var initStamp = datastore.tickCounter.Value;
        yield return new WaitUntil(() => datastore.tickCounter.Value - initStamp >= datastore.baseQueueTime);
        pedestrian.headingHome = true;
        pedestrian.transform.position = lot.pedestrianExitNode.transform.position;
        pedestrian.currentNode = lot.pedestrianExitNode;
        pedestrian.CalculateItinerary();
        pedServicePending = false;
    }
}
