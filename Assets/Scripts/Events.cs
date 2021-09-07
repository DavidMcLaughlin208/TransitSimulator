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