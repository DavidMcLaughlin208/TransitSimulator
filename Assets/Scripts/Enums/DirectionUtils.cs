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

public enum Rotation
{
    ZERO,
    NINETY,
    ONEEIGHTY,
    TWOSEVENTY
}

public class DirectionUtils
{
    public static Dictionary<Rotation, int> directionToIntMapping = new Dictionary<Rotation, int>()
    {
        {Rotation.ZERO, 0}, {Rotation.NINETY, 270}, {Rotation.ONEEIGHTY, 180}, {Rotation.TWOSEVENTY, 90}
    };

    public static Dictionary<Rotation, Dictionary<Direction, Direction>> directionRotationMapping = new Dictionary<Rotation, Dictionary<Direction, Direction>>()
    {
        {
            Rotation.ZERO, new Dictionary<Direction, Direction>()
            {
                {Direction.NORTH, Direction.NORTH},
                {Direction.EAST, Direction.EAST},
                {Direction.SOUTH, Direction.SOUTH},
                {Direction.WEST, Direction.WEST}
            }
        },
        {
            Rotation.NINETY, new Dictionary<Direction, Direction>()
            {
                {Direction.NORTH, Direction.EAST},
                {Direction.EAST, Direction.SOUTH},
                {Direction.SOUTH, Direction.WEST},
                {Direction.WEST, Direction.NORTH}
            }
        },
        {
            Rotation.ONEEIGHTY, new Dictionary<Direction, Direction>()
            {
                {Direction.NORTH, Direction.SOUTH},
                {Direction.EAST, Direction.WEST},
                {Direction.SOUTH, Direction.NORTH},
                {Direction.WEST, Direction.EAST}
            }
        },
        {
            Rotation.TWOSEVENTY, new Dictionary<Direction, Direction>()
            {
                {Direction.NORTH, Direction.WEST},
                {Direction.EAST, Direction.NORTH},
                {Direction.SOUTH, Direction.EAST},
                {Direction.WEST, Direction.SOUTH}
            }
        }
    };

    public static Dictionary<Direction, Vector2> directionToCoordinatesMapping = new Dictionary<Direction, Vector2>() {
        {Direction.NORTH, Vector2.up}, {Direction.EAST, Vector2.right}, {Direction.SOUTH, Vector2.down}, {Direction.WEST, Vector2.left}
    };

    // When a tile receives a connection attempt from another tile this mapping is referenced to determine which internal pedestrian node should be connected
    public static Dictionary<Direction, Dictionary<NodeLocation, NodeLocation>> externalConnectionMapping = new Dictionary<Direction, Dictionary<NodeLocation, NodeLocation>>()
    {
        {Direction.NORTH, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TR, NodeLocation.BR}, {NodeLocation.TL, NodeLocation.BL} } },
        {Direction.EAST, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TR, NodeLocation.TL}, {NodeLocation.BR, NodeLocation.BL} } },
        {Direction.SOUTH, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.BR, NodeLocation.TR}, {NodeLocation.BL, NodeLocation.TL} } },
        {Direction.WEST, new Dictionary<NodeLocation, NodeLocation>() { {NodeLocation.TL, NodeLocation.TR}, {NodeLocation.BL, NodeLocation.BR} } }
    };

    // nodeExternalConnectionDirections is referenced when pedestrian nodes attempt to connect to nodes in adjacent tiles
    public static Dictionary<NodeLocation, List<Direction>> nodeExternalConnectionDirections = new Dictionary<NodeLocation, List<Direction>>()
    {
        {NodeLocation.TR, new List<Direction>() {Direction.NORTH, Direction.EAST}},
        {NodeLocation.TL, new List<Direction>() {Direction.NORTH, Direction.WEST}},
        {NodeLocation.BR, new List<Direction>() {Direction.SOUTH, Direction.EAST}},
        {NodeLocation.BL, new List<Direction>() {Direction.SOUTH, Direction.WEST}},
    };

    // internalConnectionMapping is referenced when connecting pedestrian nodes within the same tile based on roadtype
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

    public static NodeLocation Rotate(NodeLocation location, Rotation rotation)
    {
        return rotationMapping[rotation][location];
    }

    // rotationMapping is referenced when connecting internal pedestrian nodes and the tile is rotated at any of the four specified angles
    // This translates the node locations so the mappings in internalConnectionMapping connect the correct nodes
    public static Dictionary<Rotation, Dictionary<NodeLocation, NodeLocation>> rotationMapping = new Dictionary<Rotation, Dictionary<NodeLocation, NodeLocation>>()
    {
        {
            Rotation.ZERO, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.TR},
                {NodeLocation.TL, NodeLocation.TL},
                {NodeLocation.BR, NodeLocation.BR},
                {NodeLocation.BL, NodeLocation.BL}
            }
        },
        {
            Rotation.NINETY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.BR},
                {NodeLocation.BR, NodeLocation.BL},
                {NodeLocation.BL, NodeLocation.TL},
                {NodeLocation.TL, NodeLocation.TR}
            }
        },
        {
            Rotation.ONEEIGHTY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.BL},
                {NodeLocation.BR, NodeLocation.TL},
                {NodeLocation.BL, NodeLocation.TR},
                {NodeLocation.TL, NodeLocation.BR}
            }
        },
        {
            Rotation.TWOSEVENTY, new Dictionary<NodeLocation, NodeLocation>()
            {
                {NodeLocation.TR, NodeLocation.TL},
                {NodeLocation.BR, NodeLocation.TR},
                {NodeLocation.BL, NodeLocation.BR},
                {NodeLocation.TL, NodeLocation.BL}
            }
        }
    };
}

