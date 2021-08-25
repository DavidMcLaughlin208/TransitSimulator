using UnityEngine;
using UnityEngine.Tilemaps;

public class God : MonoBehaviour
{
    Datastore datastore;
    Prefabs prefabs;

    void Awake() {
        datastore = this.gameObject.AddAndGetComponent<Datastore>();
        prefabs = this.gameObject.AddAndGetComponent<Prefabs>();
        datastore.canvasParent = GameObject.Find("Canvas");

        this.gameObject.AddComponent<Mouse>();
        this.gameObject.AddComponent<Placer>();
        this.gameObject.AddComponent<ToolPicker>();
    }

    void Start() {
        var baseTilemap = GameObject.Instantiate(prefabs.baseTilemapPrefab);
        var activeLevel = GameObject.Instantiate(prefabs.levelPrefab);
        activeLevel.transform.SetParent(baseTilemap.transform);

        datastore.validTiles = activeLevel.GetComponent<Tilemap>();
    }
}
