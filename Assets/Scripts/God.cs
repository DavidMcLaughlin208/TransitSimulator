using Transit;
using UnityEngine;
using UnityEngine.Tilemaps;

public class God : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    void Awake() {
        // dependencies need to be placed earlier in this initialization code
        // I thought Awake for God would complete before Awake for any of these components would be called
        // but it looks like it either spins off another process or waits until Awake for a new component is complete
        datastore = this.gameObject.AddAndGetComponent<Datastore>();
        prefabs = this.gameObject.AddAndGetComponent<Prefabs>();
        datastore.canvasParent = GameObject.Instantiate(prefabs.canvas);

        this.gameObject.AddComponent<MouseAndKeyboard>();
        this.gameObject.AddComponent<TrainNetwork>();
        this.gameObject.AddComponent<Placer>();
        this.gameObject.AddComponent<Deck>();
        this.gameObject.AddComponent<ToolPicker>();
        this.gameObject.AddComponent<GameUI>();
        this.gameObject.AddComponent<CardUI>();
        this.gameObject.AddComponent<PedestrianWatcher>();
    }

    void Start() {
        var baseTilemap = GameObject.Instantiate(prefabs.baseTilemapPrefab);
        var activeLevel = GameObject.Instantiate(prefabs.levelPrefab);
        activeLevel.transform.SetParent(baseTilemap.transform);

        datastore.validTiles = activeLevel.GetComponent<Tilemap>();
    }
}
