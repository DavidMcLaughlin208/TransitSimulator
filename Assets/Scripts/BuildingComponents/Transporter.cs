using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;

public class Transporter : MonoBehaviour {
    public Prefabs prefabs;
    public Datastore datastore;
    public TrainNetwork network;

    public Lot lot;
    public Building building;

    public List<int> lineNumsConnected = new List<int>();
    public Transporter? prevStation = null;
    public Transporter? nextStation = null;

    public void Awake () {
        var god = GameObject.Find("God");
        prefabs = god.GetComponent<Prefabs>();
        datastore = god.GetComponent<Datastore>();
        network = god.GetComponent<TrainNetwork>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.pedestrianEntranceNode.owningBuilding = building;
        lot.pedestrianExitNode.owningBuilding = building;

        datastore.gameEvents
            .Receive<TrainNetworkChangedEvent>()
            .Where(e => lineNumsConnected.Contains(e.lineChanged))
            .Subscribe(e => {
                var connectedExits = new List<Transporter?>() {prevStation, nextStation}
                    .Where(station => station != null)
                    .Select(station => (Node) station.lot.pedestrianExitNode).ToList();
                var connectedEntrances = new List<Transporter?>() {prevStation, nextStation}
                    .Where(station => station != null)
                    .Select(station => (Node) station.lot.pedestrianEntranceNode).ToList();

                lot.pedestrianEntranceNode.connections = lot.pedestrianEntranceNode.connections.Concat(connectedExits).Distinct().ToList();
                lot.pedestrianExitNode.connections = lot.pedestrianExitNode.connections.Concat(connectedEntrances).Distinct().ToList();
            });
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
        pedestrian.transform.position = lot.pedestrianExitNode.transform.position;
        pedestrian.currentNode = lot.pedestrianExitNode;
        pedestrian.CalculateItinerary();
    }
}