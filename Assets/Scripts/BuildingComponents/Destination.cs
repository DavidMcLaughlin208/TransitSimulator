using UnityEngine;
using System.Collections.Generic;

public class Destination : MonoBehaviour {

    public Prefabs prefabs;

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

    public void Awake () {
        prefabs = GameObject.Find("God").GetComponent<Prefabs>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.entranceNode.owningBuilding = building;
        lot.exitNode.owningBuilding = building;
    }

    public void ReceivePedestrian(Pedestrian pedestrian) {
        pedestrian.headingHome = true;
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
