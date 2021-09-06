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
            this.lot.carEntranceNode.destType = value;
            this.lot.pedestrianEntranceNode.destType = value;
            type = value;
        }
    }

    public Queue<Pedestrian> pedQueue = new Queue<Pedestrian>();
    public bool pedServicePending = false;

    public Queue<Car> carQueue = new Queue<Car>();
    public bool carServicePending = false;

    public void Awake () {
        var god = GameObject.Find("God");
        prefabs = god.GetComponent<Prefabs>();
        datastore = god.GetComponent<Datastore>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.pedestrianEntranceNode.owningBuilding = building;
        lot.pedestrianExitNode.owningBuilding = building;

        lot.carEntranceNode.owningBuilding = building;
        lot.carExitNode.owningBuilding = building;
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
