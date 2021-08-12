using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour {

    public GameObject baseTilemapPrefab;
    public GameObject levelPrefab;

    public GameObject road;
    public GameObject lot;

    void Awake() {
        baseTilemapPrefab = Resources.Load<GameObject>("Prefabs/BaseTilemap");
        levelPrefab = Resources.Load<GameObject>("Prefabs/Levels/Downtown");

        road = Resources.Load<GameObject>("Prefabs/CityObjects/Road");
        lot = Resources.Load<GameObject>("Prefabs/CityObjects/Lot");
    }
}