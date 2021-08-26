using UnityEngine;
using UnityEngine.Tilemaps;
using UniRx;
using System.Collections.Generic;

public class Datastore : MonoBehaviour {

    //                                          _     _
    //                                         (_)   | |
    //   __ _  __ _ _ __ ___   ___    __ _ _ __ _  __| |
    //  / _` |/ _` | '_ ` _ \ / _ \  / _` | '__| |/ _` |
    // | (_| | (_| | | | | | |  __/ | (_| | |  | | (_| |
    //  \__, |\__,_|_| |_| |_|\___|  \__, |_|  |_|\__,_|
    //   __/ |                        __/ |
    //  |___/                        |___/

    public Tilemap validTiles;
    public Dictionary<Vector2Int, CityTile> city = new Dictionary<Vector2Int, CityTile>();
    public Vector2Int lotScale = new Vector2Int(3, 3);
    public Dictionary<PedestrianNodeLocation, PedestrianNode> pedNodeMap = new Dictionary<PedestrianNodeLocation, PedestrianNode>();
    public Dictionary<RoadNodeLocation, RoadNode> roadNodeMap = new Dictionary<RoadNodeLocation, RoadNode>();

    //                               _
    //                              | |
    //   __ _  __ _ _ __ ___   ___  | | ___   ___  _ __
    //  / _` |/ _` | '_ ` _ \ / _ \ | |/ _ \ / _ \| '_ \
    // | (_| | (_| | | | | | |  __/ | | (_) | (_) | |_) |
    //  \__, |\__,_|_| |_| |_|\___| |_|\___/ \___/| .__/
    //   __/ |                                    | |
    //  |___/                                     |_|
    public IntReactiveProperty tickCounter = new IntReactiveProperty(0);
    public FloatReactiveProperty spawnChance = new FloatReactiveProperty(0.1f);

    //  _                   _
    // (_)                 | |
    //  _ _ __  _ __  _   _| |_
    // | | '_ \| '_ \| | | | __|
    // | | | | | |_) | |_| | |_
    // |_|_| |_| .__/ \__,_|\__|
    //         | |
    //         |_|
    public ReactiveProperty<ToolType?> activeTool = new ReactiveProperty<ToolType?>();
    public ReactiveProperty<DestinationType?> activeToolColor = new ReactiveProperty<DestinationType?>();
    public MessageBroker inputEvents = new MessageBroker();

    //        _
    //       (_)
    //  _   _ _
    // | | | | |
    // | |_| | |
    //  \__,_|_|
    public GameObject canvasParent;

}