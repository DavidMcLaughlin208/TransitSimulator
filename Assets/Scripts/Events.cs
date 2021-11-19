using UnityEngine;
using System.Collections.Generic;

//  _                   _
// (_)                 | |
//  _ _ __  _ __  _   _| |_
// | | '_ \| '_ \| | | | __|
// | | | | | |_) | |_| | |_
// |_|_| |_| .__/ \__,_|\__|
//         | |
//         |_|

public class ClickEvent {
    public Vector3Int cell;
}

public class MouseUpEvent{
    public Vector2 mouseLocation;
}

public class MouseMoveEvent {}

public class HoverEvent {
    public Vector3Int cell;
}

public class KeyEvent {
    public Vector3Int cell;
    public KeyCode keyCode;
    public List<KeyCode> heldKeys;
}

//                               _
//                              | |
//   __ _  __ _ _ __ ___   ___  | | ___   ___  _ __
//  / _` |/ _` | '_ ` _ \ / _ \ | |/ _ \ / _ \| '_ \
// | (_| | (_| | | | | | |  __/ | | (_) | (_) | |_) |
//  \__, |\__,_|_| |_| |_|\___| |_|\___/ \___/| .__/
//   __/ |                                    | |
//  |___/                                     |_|
public class CityChangedEvent {}
public class PedestrianSpawnedEvent {
    public List<Pedestrian> pedestrians;
}
public class PedestrianDespawnedEvent {
    public List<Pedestrian> pedestrians;
}
public class PedestrianTripCompletedEvent {
    public Pedestrian pedestrian;
}
public class TrainNetworkChangedEvent {
    public int lineChanged;
}

//                            _       
//                           | |      
//   ___    __ _   _ __    __| |  ___ 
//  / __|  / _` | | '__|  / _` | / __|
// | (__  | (_| | | |    | (_| | \__ \
//  \___|  \__,_| |_|     \__,_| |___/
public class CardDrawnEvent {}