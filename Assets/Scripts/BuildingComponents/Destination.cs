using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destination : MonoBehaviour {

    public Prefabs prefabs;
    public Datastore datastore;

    public Lot lot;
    public Building building;

    private DestinationType type; // backing field for destType
    public DestinationType destType {
        get { return type; }
        set {
            this.lot.entranceNode.destType = value;
            type = value;
        }
    }

    public Queue<Pedestrian> pedQueue = new Queue<Pedestrian>();
    public bool servicePending = false;

    public void Awake () {
        var god = GameObject.Find("God");
        prefabs = god.GetComponent<Prefabs>();
        datastore = god.GetComponent<Datastore>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.entranceNode.owningBuilding = building;
        lot.exitNode.owningBuilding = building;
    }

    public void Update () {
        if (!servicePending && pedQueue.Count > 0) {
            StartCoroutine(QueueForFrames(pedQueue.Dequeue(), datastore.baseQueueTime));
        }
    }

    public void ReceivePedestrian(Pedestrian pedestrian) {
        pedQueue.Enqueue(pedestrian);
    }

    IEnumerator QueueForFrames(Pedestrian pedestrian, int seconds) {
        servicePending = true;
        var initStamp = datastore.tickCounter.Value;
        yield return new WaitUntil(() => datastore.tickCounter.Value - initStamp >= datastore.baseQueueTime);
        pedestrian.headingHome = true;
        pedestrian.transform.position = lot.exitNode.transform.position;
        pedestrian.currentNode = lot.exitNode;
        pedestrian.CalculateItinerary();
        servicePending = false;
    }
}

public enum DestinationType
{
    NONE,
    COFFEE,
    TEA,
    BEER
}

public static class DestinationUtils {
    public static List<DestinationType> allDestTypes = new List<DestinationType> {DestinationType.COFFEE, DestinationType.TEA, DestinationType.BEER};
}
