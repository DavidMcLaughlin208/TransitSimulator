using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour {

    public GameObject baseTilemapPrefab;
    public GameObject levelPrefab;

    public GameObject canvas;

    public GameObject road;
    public GameObject lot;
    public GameObject building;

    public GameObject pedestrian;
    public GameObject car;
    public GameObject train;

    public GameObject trainStationNode;
    public GameObject trainLineConnector;

    void Awake() {
        baseTilemapPrefab = Resources.Load<GameObject>("Prefabs/BaseTilemap");
        levelPrefab = Resources.Load<GameObject>("Prefabs/Levels/MiniDowntown");

        canvas = Resources.Load<GameObject>("Prefabs/UI/Canvas");

        road = Resources.Load<GameObject>("Prefabs/CityObjects/Road");
        lot = Resources.Load<GameObject>("Prefabs/CityObjects/Lot");
        building = Resources.Load<GameObject>("Prefabs/CityObjects/Building");

        pedestrian = Resources.Load<GameObject>("Prefabs/CityObjects/Pedestrian");
        car = Resources.Load<GameObject>("Prefabs/CityObjects/Car");
        train = Resources.Load<GameObject>("Prefabs/CityObjects/Train");

        trainStationNode = Resources.Load<GameObject>("Prefabs/CityObjects/TrainNode");
        trainLineConnector = Resources.Load<GameObject>("Prefabs/UI/TrainLineConnector");
    }
}