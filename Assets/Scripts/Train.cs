using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using System;

public class Train : MonoBehaviour
{
    public Datastore datastore;
    public TrainNetwork trainNetwork;

    public List<TrainNode> lineStations;
    public List<TrainNode> itinerary = new List<TrainNode>();
    public TrainNode currentNode;
    public bool headingHome;
    public bool waitingAtStation = false;
    public int lineNum;

    public List<Pedestrian> passengers = new List<Pedestrian>();

    public float speed = 2f;

    public void Awake() {
        var god = GameObject.Find("God");
        datastore = god.GetComponent<Datastore>();
        trainNetwork = god.GetComponent<TrainNetwork>();
    }

    void Start() {
        lineStations = trainNetwork.lines[lineNum].Select(station => station.lot.trainStationNode).ToList();
        GetComponent<SpriteRenderer>().color = trainNetwork.lineColors[lineNum];
        itinerary = CalculateItinerary();

        datastore.tickCounter.Subscribe(_ => UpdateOnTick());

        datastore.gameEvents
            .Receive<TrainNetworkChangedEvent>()
            .Where(e => e.lineChanged == lineNum)
            .Subscribe(_ => {
                lineStations = trainNetwork.lines[lineNum].Select(station => station.lot.trainStationNode).ToList();
                itinerary = CalculateItinerary();
            });
    }

    void UpdateOnTick()
    {
        if (itinerary.Count > 0 && !waitingAtStation)
        {
            TrainNode target = itinerary.First();
            transform.position = DriveTowardTarget(target, transform.position);

            if (Vector2.Distance(transform.position, target.transform.position) < 0.05)
            {
                currentNode = target;
                itinerary.RemoveAt(0);

                if (itinerary.Count == 0) {
                    if (currentNode == lineStations.First()) {
                        itinerary = lineStations.Skip(1).ToList();
                    } else {
                        itinerary = lineStations.AsEnumerable().Reverse().Skip(1).ToList();
                    }
                }

                target.owningStation.ReceiveTrain(this);
            }
        }
    }

    List<TrainNode> CalculateItinerary() {
        var newItinerary = new List<TrainNode>();
        var curStationIndex = lineStations.IndexOf(currentNode);
        if (curStationIndex == 0) {
            newItinerary = lineStations.Skip(1).ToList();
        } else if (curStationIndex == lineStations.Count - 1) {
            newItinerary = lineStations.AsEnumerable().Reverse().Skip(1).ToList();
        } else {
            var rnd = new System.Random().Next(0, 1);
            if (rnd == 0) {
                newItinerary = lineStations.Skip(curStationIndex + 1).ToList();
            } else {
                newItinerary = lineStations.Take(curStationIndex).AsEnumerable().Reverse().ToList();
            }
        }

        return newItinerary;
    }

    Vector2 DriveTowardTarget(Node target, Vector2 curPosition) {
        float step = speed * datastore.deltaTime; // calculate distance to move
        return Vector2.MoveTowards(curPosition, target.transform.position, step);
    }
}
