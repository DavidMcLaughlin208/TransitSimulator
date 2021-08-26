using UnityEngine;
using UnityEngine.Tilemaps;
using UniRx;
using System;

public class God : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    void Awake() {
        datastore = this.gameObject.AddAndGetComponent<Datastore>();
        prefabs = this.gameObject.AddAndGetComponent<Prefabs>();
        datastore.canvasParent = GameObject.Instantiate(prefabs.canvas);

        this.gameObject.AddComponent<MouseAndKeyboard>();
        this.gameObject.AddComponent<Placer>();
        this.gameObject.AddComponent<ToolPicker>();
    }

    void Start() {
        var baseTilemap = GameObject.Instantiate(prefabs.baseTilemapPrefab);
        var activeLevel = GameObject.Instantiate(prefabs.levelPrefab);
        activeLevel.transform.SetParent(baseTilemap.transform);

        datastore.validTiles = activeLevel.GetComponent<Tilemap>();

        Observable.Interval(TimeSpan.FromMilliseconds(1000)).Subscribe(_ => datastore.tickCounter.Value++);
    }
}
