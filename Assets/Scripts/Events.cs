using UnityEngine;

public class ClickEvent {
    public Vector3Int cell;
}

public class MouseUpEvent
{
    public Vector2 mouseLocation;
}

public class MouseMoveEvent { }

public class HoverEvent {
    public Vector3Int cell;
}

public class KeyEvent {
    public Vector3Int cell;
    public KeyCode keyCode;
}