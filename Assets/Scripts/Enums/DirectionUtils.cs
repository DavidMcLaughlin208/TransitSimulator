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

public enum RoadType
{
    Straight,
    Intersection,
    TJunction,
    Corner
}

public enum RoadRotation
{
    ZERO,
    NINETY,
    ONEEIGHTY,
    TWOSEVENTY
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

    public static Dictionary<RoadType, Dictionary<NodeLocation, List<NodeLocation>>> internalConnectionMapping = new Dictionary<RoadType, Dictionary<NodeLocation, List<NodeLocation>>>()
    {
        {
            RoadType.Straight, new Dictionary<NodeLocation, List<NodeLocation>>() {
                {NodeLocation.TR, new List<NodeLocation>() {NodeLocation.TL}},
                {NodeLocation.TL, new List<NodeLocation>() {NodeLocation.TR}},
                {NodeLocation.BR, new List<NodeLocation>() {NodeLocation.BL}},
                {NodeLocation.BL, new List<NodeLocation>() {NodeLocation.BR}}
            }
        },
        {
            RoadType.Intersection, new Dictionary<NodeLocation, List<NodeLocation>>() {
                {NodeLocation.TR, new List<NodeLocation>() {NodeLocation.TL, NodeLocation.BR}},
                {NodeLocation.TL, new List<NodeLocation>() {NodeLocation.TR, NodeLocation.BL}},
                {NodeLocation.BR, new List<NodeLocation>() {NodeLocation.BL, NodeLocation.TR}},
                {NodeLocation.BL, new List<NodeLocation>() {NodeLocation.BR, NodeLocation.TL}}
            }
        },
        {
            RoadType.TJunction, new Dictionary<NodeLocation, List<NodeLocation>>() {
                {NodeLocation.TR, new List<NodeLocation>() {NodeLocation.TL}},
                {NodeLocation.TL, new List<NodeLocation>() {NodeLocation.TR}},
                {NodeLocation.BR, new List<NodeLocation>() {}},
                {NodeLocation.BL, new List<NodeLocation>() {}}
            }
        },
        {
            RoadType.Corner, new Dictionary<NodeLocation, List<NodeLocation>>() {
                {NodeLocation.TR, new List<NodeLocation>() {NodeLocation.TL}},
                {NodeLocation.TL, new List<NodeLocation>() {NodeLocation.TR, NodeLocation.BL}},
                {NodeLocation.BR, new List<NodeLocation>() {}},
                {NodeLocation.BL, new List<NodeLocation>() {NodeLocation.TL}}
            }
        }

    };

    public static Dictionary<RoadRotation, Dictionary<NodeLocation, NodeLocation>> rotationMapping = new Dictionary<RoadRotation, Dictionary<NodeLocation, NodeLocation>>()
    {
        {
            RoadRotation.ZERO, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.TR},
                {NodeLocation.TL, NodeLocation.TL},
                {NodeLocation.BR, NodeLocation.BR},
                {NodeLocation.BL, NodeLocation.BL}
            }
        },
        {
            RoadRotation.NINETY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.BR},
                {NodeLocation.BR, NodeLocation.BL},
                {NodeLocation.BL, NodeLocation.TL},
                {NodeLocation.TL, NodeLocation.TR}
            }
        },
        {
            RoadRotation.ONEEIGHTY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.BL},
                {NodeLocation.BR, NodeLocation.TL},
                {NodeLocation.BL, NodeLocation.TR},
                {NodeLocation.TL, NodeLocation.BR}
            }
        },
        {
            RoadRotation.TWOSEVENTY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.TL},
                {NodeLocation.BR, NodeLocation.TR},
                {NodeLocation.BL, NodeLocation.BR},
                {NodeLocation.TL, NodeLocation.BL}
            }
        }
    };
}

