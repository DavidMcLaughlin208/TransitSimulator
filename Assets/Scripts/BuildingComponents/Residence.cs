using UnityEngine;

public class Residence : MonoBehaviour {
    public Datastore datastore;

    public Lot lot;
    public Building building;
    public DestinationType housingType;

    public int capacity = 0;

    public void Awake () {
        var god = GameObject.Find("God");
        datastore = god.GetComponent<Datastore>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.entranceNode.owningBuilding = building;
        lot.exitNode.owningBuilding = building;
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
        pedestrian.transform.position = lot.exitNode.transform.position;
        pedestrian.currentNode = lot.exitNode;
        pedestrian.CalculateItinerary();
    }
}