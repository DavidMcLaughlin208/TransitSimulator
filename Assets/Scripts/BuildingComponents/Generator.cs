using UnityEngine;
using UniRx;
using System;

public class Generator : MonoBehaviour {

    public Prefabs prefabs;
    public Datastore datastore;

    public Lot lot;
    public Building building;

    public System.Random random;

    public void Awake () {
        var god = GameObject.Find("God");
        prefabs = god.GetComponent<Prefabs>();
        datastore = god.GetComponent<Datastore>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.entranceNode.owningBuilding = building;
        lot.exitNode.owningBuilding = building;

        random = new System.Random();
    }

    public void Start () {
        datastore.tickCounter.Subscribe(_ => {
            var nextType = DestinationUtils.allDestTypes.getRandomElement();
            if (random.Next(101) < (datastore.spawnChance.Value * 100)) {
                SpawnPedestrian(nextType);
            }
        });
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