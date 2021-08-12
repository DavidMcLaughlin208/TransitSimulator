using UnityEngine;
using UnityEngine.Tilemaps;
using UniRx;
using System.Collections.Generic;

public class Datastore : MonoBehaviour {
    public Tilemap validTiles;
    public Dictionary<Vector2Int, CityTile> city = new Dictionary<Vector2Int, CityTile>();
    public Vector2Int lotScale = new Vector2Int(3, 3);
    public MessageBroker inputEvents = new MessageBroker();
}