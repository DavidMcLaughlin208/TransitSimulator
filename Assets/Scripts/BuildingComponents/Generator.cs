using UnityEngine;
using UniRx;
using System.Collections.Generic;

public class Generator : MonoBehaviour {

    public Prefabs prefabs;
    public Datastore datastore;

    public Lot lot;
    public Building building;

    public System.Random random;

    public Dictionary<DestinationType, int> pedCapacity = new Dictionary<DestinationType, int>() {
        {DestinationType.COFFEE, 0},
        {DestinationType.BEER, 0},
        {DestinationType.TEA, 0},
    };

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
        datastore.tickCounter
            .Where(counterValue => counterValue % Mathf.RoundToInt(60 / datastore.tickModifier.Value) == 0) // do this every 60 ticks
            .Subscribe(_ => {
                Debug.Log("Trying to spawn ped");
                var nextType = DestinationUtils.allDestTypes.getRandomElement();
                if (random.Next(101) < (datastore.spawnChance.Value * 100)) {
                    SpawnPedestrian(nextType);
                }
            });
    }

    public void SpawnPedestrian(DestinationType destType)
    {
        if (pedCapacity[destType] == datastore.baseCapacity) {
            return;
        }
        var pedestrianObj = GameObject.Instantiate(prefabs.pedestrian, transform);
        pedestrianObj.transform.position = lot.exitNode.transform.position;
        Pedestrian pedestrian = pedestrianObj.GetComponent<Pedestrian>();
        pedestrian.currentNode = lot.exitNode;
        pedestrian.homeNode = lot.entranceNode;
        pedestrian.desiredDestType = destType;
        pedestrian.CalculateItinerary();
        pedCapacity[destType]++;
        datastore.allPedestrians.Add(pedestrian);
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
        pedestrian.transform.position = lot.exitNode.transform.position;
        pedestrian.currentNode = lot.exitNode;
        pedestrian.CalculateItinerary();
    }
}