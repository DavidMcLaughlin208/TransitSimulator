using UnityEngine;
using UnityEngine.Tilemaps;
using UniRx;
using System.Collections.Generic;
using System;

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
    public Vector2Int lotScale = new Vector2Int(1, 1);
    public Dictionary<PedestrianNodeLocation, PedestrianNode> pedNodeMap = new Dictionary<PedestrianNodeLocation, PedestrianNode>();
    public Dictionary<RoadNodeLocation, RoadNode> roadNodeMap = new Dictionary<RoadNodeLocation, RoadNode>();

    //                            _       
    //                           | |      
    //   ___    __ _   _ __    __| |  ___ 
    //  / __|  / _` | | '__|  / _` | / __|
    // | (__  | (_| | | |    | (_| | \__ \
    //  \___|  \__,_| |_|     \__,_| |___/
    public int initialDeckSize = 10; // mock value for generating cards into deck
    public ReactiveCollection<GameObject> deck = new ReactiveCollection<GameObject>();
    public ReactiveCollection<GameObject> cardsInHand = new ReactiveCollection<GameObject>();
    public ReactiveCollection<GameObject> cardsInDrawPile = new ReactiveCollection<GameObject>();
    public ReactiveCollection<GameObject> cardsInDiscard = new ReactiveCollection<GameObject>();

    public int drawEnergyCost = 1;

    public IntReactiveProperty energy = new IntReactiveProperty(0);
    public IntReactiveProperty tripsToEnergyConversion = new IntReactiveProperty(25);
    

    //                               _
    //                              | |
    //   __ _  __ _ _ __ ___   ___  | | ___   ___  _ __
    //  / _` |/ _` | '_ ` _ \ / _ \ | |/ _ \ / _ \| '_ \
    // | (_| | (_| | | | | | |  __/ | | (_) | (_) | |_) |
    //  \__, |\__,_|_| |_| |_|\___| |_|\___/ \___/| .__/
    //   __/ |                                    | |
    //  |___/                                     |_|
    IDisposable tickUpdater; // responsible for updating tickCounter for "frame rate"
    public IntReactiveProperty tickCounter = new IntReactiveProperty(0);
    public FloatReactiveProperty tickModifier = new FloatReactiveProperty(1f);
    public float frameSpan = 0.016666667f; // 60 FPS
    public float deltaTime = 1f;
    public FloatReactiveProperty spawnChance = new FloatReactiveProperty(0.1f);
    public int baseCapacity = 10; // base capacity for hotels
    public int baseQueueTime = 180; // base # of frames for shops to take to serve peds. value in (# of seconds / frameSpan).
    public MessageBroker gameEvents = new MessageBroker();
    public int basePedPatience = 3600; // base patience, value in (# of seconds / frameSpan) for now
    public List<Pedestrian> allPedestrians = new List<Pedestrian>();
    public IntReactiveProperty totalPopulation = new IntReactiveProperty(0);
    public IntReactiveProperty completedTrips = new IntReactiveProperty(0);

    //  _                   _
    // (_)                 | |
    //  _ _ __  _ __  _   _| |_
    // | | '_ \| '_ \| | | | __|
    // | | | | | |_) | |_| | |_
    // |_|_| |_| .__/ \__,_|\__|
    //         | |
    //         |_|
    public ReactiveProperty<ToolType?> activeTool = new ReactiveProperty<ToolType?>();
    public ReactiveProperty<TrainLineToolType?> activeLineToolType = new ReactiveProperty<TrainLineToolType?>();
    public ReactiveProperty<DestinationType?> activeToolColor = new ReactiveProperty<DestinationType?>();
    public MessageBroker inputEvents = new MessageBroker();

    //        _
    //       (_)
    //  _   _ _
    // | | | | |
    // | |_| | |
    //  \__,_|_|
    public GameObject canvasParent;
    public ReactiveProperty<GameObject> hoveredCard = new ReactiveProperty<GameObject>(null);
    public ReactiveProperty<GameObject> clickedCard = new ReactiveProperty<GameObject>(null);


    public void Start() {
        tickModifier.Subscribe(modifier => deltaTime = tickModifier.Value == 0 ? 0 : frameSpan * tickModifier.Value);
        tickUpdater = Observable.Interval(TimeSpan.FromMilliseconds(frameSpan)).Subscribe(_ => tickCounter.Value++);

        tickModifier.Subscribe(modifier => {
            tickUpdater.Dispose();
            if (modifier != 0) {
                tickUpdater = Observable.Interval(TimeSpan.FromMilliseconds(frameSpan * modifier)).Subscribe(_ => tickCounter.Value++);
            }
        });
    }
}