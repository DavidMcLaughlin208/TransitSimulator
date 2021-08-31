using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarDestination : Destination
{

    // Use this for initialization
    void Start()
    {
        lot.pedestrianConnectionsEnabled = false;
        lot.carConnectionsEnabled = true;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!carServicePending && carQueue.Count > 0)
        {
            StartCoroutine(QueueForSeconds(carQueue.Dequeue(), datastore.baseQueueTime));
        }
    }

    IEnumerator QueueForSeconds(Car car, int seconds)
    {
        carServicePending = true;
        yield return new WaitForSeconds(seconds);
        car.headingHome = true;
        car.transform.position = lot.carExitNode.transform.position;
        //car.currentNode = lot.pedestrianExitNode;
        car.CalculateItinerary();
        carServicePending = false;
    }
}
