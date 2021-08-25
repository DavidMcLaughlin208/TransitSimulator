using UnityEngine;

public class Generator : MonoBehaviour {

    public Prefabs prefabs;

    public Lot lot;
    public Building building;

    public void Awake () {
        prefabs = GameObject.Find("God").GetComponent<Prefabs>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.entranceNode.owningBuilding = building;
        lot.exitNode.owningBuilding = building;
    }

    public void SpawnPedestrian(DestinationType destType)
    {
        var pedestrianObj = GameObject.Instantiate(prefabs.pedestrian, transform);
        pedestrianObj.transform.position = lot.exitNode.transform.position;
        Pedestrian pedestrian = pedestrianObj.GetComponent<Pedestrian>();
        pedestrian.currentNode = lot.exitNode;
        pedestrian.homeNode = lot.entranceNode;
        pedestrian.desiredDestType = destType;
        pedestrian.CalculateItinerary();
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
    }
}