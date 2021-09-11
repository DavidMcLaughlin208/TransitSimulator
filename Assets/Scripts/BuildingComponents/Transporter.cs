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

        lot.trainStationNode = GameObject.Instantiate(prefabs.trainStationNode, this.transform).GetComponent<TrainNode>();

        // only bind entrance and exits into train station monodirectionally
        lot.pedestrianEntranceNode.connections = lot.pedestrianEntranceNode.connections.Append(lot.trainStationNode).Distinct().ToList();
        lot.trainStationNode.connections = new List<Node>() {lot.pedestrianExitNode};

        lot.trainStationNode.owningBuilding = building;
        lot.trainStationNode.owningStation = this;

        datastore.gameEvents
            .Receive<TrainNetworkChangedEvent>()
            .Where(e => lineNumsConnected.Contains(e.lineChanged))
            .Subscribe(e => {
                var connectedStations = new List<Transporter?>() {prevStation, nextStation}
                    .Where(station => station != null)
                    .Select(station => (Node) station.lot.trainStationNode).ToList();

                lot.trainStationNode.connections = lot.trainStationNode.connections.Concat(connectedStations).ToList();
            });
    }

    public Vector2 TakeTrainToTarget(Node target, Vector2 curPosition) {
        float step = 100f * datastore.deltaTime; // calculate distance to move
        return Vector2.MoveTowards(curPosition, target.transform.position, step);
    }

    public void ReceivePedestrian(Pedestrian pedestrian)
    {
        pedestrian.headingHome = false;
        pedestrian.transform.position = lot.pedestrianExitNode.transform.position;
        pedestrian.currentNode = lot.pedestrianExitNode;
        pedestrian.CalculateItinerary();
    }
}