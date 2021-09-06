using UnityEngine;

public class Residence : MonoBehaviour {
    public Datastore datastore;

    public Lot lot;
    public Building building;
    public DestinationType housingType;
    public CarDestination attachedParkingLot;

    public int capacity = 0;

    public void Awake () {
        var god = GameObject.Find("God");
        datastore = god.GetComponent<Datastore>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.pedestrianEntranceNode.owningBuilding = building;
        lot.pedestrianExitNode.owningBuilding = building;
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
        pedestrian.transform.position = lot.pedestrianExitNode.transform.position;
        pedestrian.currentNode = lot.pedestrianExitNode;
        pedestrian.CalculateItinerary();
    }
}