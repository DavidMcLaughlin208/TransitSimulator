﻿using UnityEngine;
using System.Collections;

public class PedestrianDestination : Destination
{
    // Use this for initialization
    void Start()
    {

    }

    public void Update () {
        if (!pedServicePending && pedQueue.Count > 0) {
            StartCoroutine(QueueForSeconds(pedQueue.Dequeue(), datastore.baseQueueTime));
        }
    }

    public void ReceivePedestrian(Pedestrian pedestrian) {
        pedQueue.Enqueue(pedestrian);
    }

    IEnumerator QueueForSeconds(Pedestrian pedestrian, int seconds) {
        pedServicePending = true;
        yield return new WaitForSeconds(seconds);
        pedestrian.headingHome = true;
        pedestrian.transform.position = lot.pedestrianExitNode.transform.position;
        pedestrian.currentNode = lot.pedestrianExitNode;
        pedestrian.CalculateItinerary();
        pedServicePending = false;
    }
}