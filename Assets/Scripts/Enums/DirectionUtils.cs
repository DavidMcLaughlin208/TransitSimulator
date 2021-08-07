using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}

public class DirectionUtils
{
    public static Dictionary<Direction, Vector2> directionToCoordinates = new Dictionary<Direction, Vector2>() {
        {Direction.NORTH, Vector2.up}, {Direction.EAST, Vector2.right}, {Direction.SOUTH, Vector2.down}, {Direction.WEST, Vector2.left}
    };

    public static Dictionary<Direction, Dictionary<NodeLocation, NodeLocation>> connectionMapping = new Dictionary<Direction, Dictionary<NodeLocation, NodeLocation>>()
    {
        {Direction.NORTH, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TR, NodeLocation.BR}, {NodeLocation.TL, NodeLocation.BL} } },
        {Direction.EAST, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TR, NodeLocation.TL}, {NodeLocation.BR, NodeLocation.BL} } },
        {Direction.SOUTH, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.BR, NodeLocation.TR}, {NodeLocation.BL, NodeLocation.TL} } },
        {Direction.WEST, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TL, NodeLocation.TR}, {NodeLocation.BL, NodeLocation.BR} } }
    };

    public static Dictionary<NodeLocation, List<Direction>> nodeConnectionDirections = new Dictionary<NodeLocation, List<Direction>>()
    {
        {NodeLocation.TR, new List<Direction>() {Direction.NORTH, Direction.EAST}},
        {NodeLocation.TL, new List<Direction>() {Direction.NORTH, Direction.WEST}},
        {NodeLocation.BR, new List<Direction>() {Direction.SOUTH, Direction.EAST}},
        {NodeLocation.BL, new List<Direction>() {Direction.SOUTH, Direction.WEST}},
    };
}

