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

    public List<Pedestrian> pedsWaitingAtStation = new List<Pedestrian>();

    public void Awake () {
        var god = GameObject.Find("God");
        prefabs = god.GetComponent<Prefabs>();
        datastore = god.GetComponent<Datastore>();
        network = god.GetComponent<TrainNetwork>();

        building = this.GetComponent<Building>();
        lot = building.parentLot;

        lot.trainStationNode = GameObject.Instantiate(prefabs.trainStationNode, this.transform).GetComponent<TrainNode>();

        // only bind entrance and exits into train station monodirectionally
        lot.pedestrianEntranceNode.connections = lot.pedestrianEntranceNode.connections.Append(lot.trainStationNode).Distinct().ToList();
        lot.trainStationNode.connections = new List<Node>() {lot.pedestrianExitNode};
        // DON'T set pedestrian entrance and exit to building nodes. This allows the Pedestrian code to be a bit more general,
        // as TrainStations have an extra node that other buildings do not

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

    public void ReceiveTrain(Train train) {
        var pedsWaitingToBoard = pedsWaitingAtStation
            .Where(ped => {
                return train.itinerary.Contains(ped.itinerary[0]);
            }).ToList();
        var pedsToDeboard = train.passengers.Where(ped => {
                return !train.itinerary.Contains(ped.itinerary[0]);
            }).ToList();

        train.passengers = train.passengers.Concat(pedsWaitingToBoard).Except(pedsToDeboard).Distinct().ToList();
        pedsWaitingAtStation = pedsWaitingAtStation.Concat(pedsToDeboard).Except(pedsWaitingToBoard).Distinct().ToList();
        pedsWaitingAtStation.ForEach(ped => ped.transform.position = this.transform.position);
    }

    public void ReceivePedestrian(Pedestrian pedestrian) {
        pedsWaitingAtStation.Add(pedestrian);
        pedestrian.waitingInBuilding = true;
    }

    public void ReleasePedestrian(Pedestrian pedestrian) {
        pedsWaitingAtStation.Remove(pedestrian);
        pedestrian.waitingInBuilding = false;
    }
}