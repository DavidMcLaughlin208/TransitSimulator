﻿using System;
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

    public static class PedestrianUtils
    {

        // When a tile receives a connection attempt from another tile this mapping is referenced to determine which internal pedestrian node should be connected
        public static Dictionary<Direction, Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>> externalConnectionMapping = new Dictionary<Direction, Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>>()
        {
            {Direction.NORTH, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>() { {PedestrianNodeLocation.TR, PedestrianNodeLocation.BR}, {PedestrianNodeLocation.TL, PedestrianNodeLocation.BL} } },
            {Direction.EAST, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>() { {PedestrianNodeLocation.TR, PedestrianNodeLocation.TL}, {PedestrianNodeLocation.BR, PedestrianNodeLocation.BL} } },
            {Direction.SOUTH, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>() { {PedestrianNodeLocation.BR, PedestrianNodeLocation.TR}, {PedestrianNodeLocation.BL, PedestrianNodeLocation.TL} } },
            {Direction.WEST, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>() { {PedestrianNodeLocation.TL, PedestrianNodeLocation.TR}, {PedestrianNodeLocation.BL, PedestrianNodeLocation.BR} } }
        };

        // nodeExternalConnectionDirections is referenced when pedestrian nodes attempt to connect to nodes in adjacent tiles
        public static Dictionary<PedestrianNodeLocation, List<Direction>> nodeExternalConnectionDirections = new Dictionary<PedestrianNodeLocation, List<Direction>>()
        {
            {PedestrianNodeLocation.TR, new List<Direction>() {Direction.NORTH, Direction.EAST}},
            {PedestrianNodeLocation.TL, new List<Direction>() {Direction.NORTH, Direction.WEST}},
            {PedestrianNodeLocation.BR, new List<Direction>() {Direction.SOUTH, Direction.EAST}},
            {PedestrianNodeLocation.BL, new List<Direction>() {Direction.SOUTH, Direction.WEST}},
        };

        // internalConnectionMapping is referenced when connecting pedestrian nodes within the same tile based on roadtype
        public static Dictionary<RoadType, Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>> internalConnectionMapping = new Dictionary<RoadType, Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>>()
        {
            {
                RoadType.Straight, new Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>() {
                    {PedestrianNodeLocation.TR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL}},
                    {PedestrianNodeLocation.TL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TR}},
                    {PedestrianNodeLocation.BR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BL}},
                    {PedestrianNodeLocation.BL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BR}}
                }
            },
            {
                RoadType.Intersection, new Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>() {
                    {PedestrianNodeLocation.TR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL, PedestrianNodeLocation.BR}},
                    {PedestrianNodeLocation.TL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TR, PedestrianNodeLocation.BL}},
                    {PedestrianNodeLocation.BR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BL, PedestrianNodeLocation.TR}},
                    {PedestrianNodeLocation.BL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BR, PedestrianNodeLocation.TL}}
                }
            },
            {
                RoadType.TJunction, new Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>() {
                    {PedestrianNodeLocation.TR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL}},
                    {PedestrianNodeLocation.TL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TR}},
                    {PedestrianNodeLocation.BR, new List<PedestrianNodeLocation>() {}},
                    {PedestrianNodeLocation.BL, new List<PedestrianNodeLocation>() {}}
                }
            },
            {
                RoadType.Corner, new Dictionary<PedestrianNodeLocation, List<PedestrianNodeLocation>>() {
                    {PedestrianNodeLocation.TR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL}},
                    {PedestrianNodeLocation.TL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TR, PedestrianNodeLocation.BL}},
                    {PedestrianNodeLocation.BR, new List<PedestrianNodeLocation>() {}},
                    {PedestrianNodeLocation.BL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL}}
                }
            }

        };

        public static PedestrianNodeLocation Rotate(PedestrianNodeLocation location, Rotation rotation)
        {
            return rotationMapping[rotation][location];
        }

        // rotationMapping is referenced when connecting internal pedestrian nodes and the tile is rotated at any of the four specified angles
        // This translates the node locations so the mappings in internalConnectionMapping connect the correct nodes
        public static Dictionary<Rotation, Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>> rotationMapping = new Dictionary<Rotation, Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>>()
        {
            {
                Rotation.ZERO, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>()
                {
                    {PedestrianNodeLocation.TR, PedestrianNodeLocation.TR},
                    {PedestrianNodeLocation.TL, PedestrianNodeLocation.TL},
                    {PedestrianNodeLocation.BR, PedestrianNodeLocation.BR},
                    {PedestrianNodeLocation.BL, PedestrianNodeLocation.BL}
                }
            },
            {
                Rotation.NINETY, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>()
                {
                    {PedestrianNodeLocation.TR, PedestrianNodeLocation.BR},
                    {PedestrianNodeLocation.BR, PedestrianNodeLocation.BL},
                    {PedestrianNodeLocation.BL, PedestrianNodeLocation.TL},
                    {PedestrianNodeLocation.TL, PedestrianNodeLocation.TR}
                }
            },
            {
                Rotation.ONEEIGHTY, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>()
                {
                    {PedestrianNodeLocation.TR, PedestrianNodeLocation.BL},
                    {PedestrianNodeLocation.BR, PedestrianNodeLocation.TL},
                    {PedestrianNodeLocation.BL, PedestrianNodeLocation.TR},
                    {PedestrianNodeLocation.TL, PedestrianNodeLocation.BR}
                }
            },
            {
                Rotation.TWOSEVENTY, new Dictionary<PedestrianNodeLocation, PedestrianNodeLocation>()
                {
                    {PedestrianNodeLocation.TR, PedestrianNodeLocation.TL},
                    {PedestrianNodeLocation.BR, PedestrianNodeLocation.TR},
                    {PedestrianNodeLocation.BL, PedestrianNodeLocation.BR},
                    {PedestrianNodeLocation.TL, PedestrianNodeLocation.BL}
                }
            }
        };
    }

    public static class RoadUtils
    {
        public static Dictionary<RoadType, Dictionary<RoadNodeLocation, List<RoadNodeLocation>>> internalConnectionMapping = new Dictionary<RoadType, Dictionary<RoadNodeLocation, List<RoadNodeLocation>>>()
        {
            {
                RoadType.Straight, new Dictionary<RoadNodeLocation, List<RoadNodeLocation>>
                {
                    {RoadNodeLocation.EIN, new List<RoadNodeLocation> {{RoadNodeLocation.WOUT}}},
                    {RoadNodeLocation.WIN, new List<RoadNodeLocation> {{RoadNodeLocation.EOUT}}}
                }
            },
            {
                RoadType.Intersection, new Dictionary<RoadNodeLocation, List<RoadNodeLocation>>
                {
                    {RoadNodeLocation.NIN, new List<RoadNodeLocation> { { RoadNodeLocation.WOUT },{ RoadNodeLocation.SOUT }, { RoadNodeLocation.EOUT } }},
                    {RoadNodeLocation.EIN, new List<RoadNodeLocation> { { RoadNodeLocation.WOUT },{ RoadNodeLocation.SOUT }, { RoadNodeLocation.NOUT } }},
                    {RoadNodeLocation.SIN, new List<RoadNodeLocation> { { RoadNodeLocation.WOUT },{ RoadNodeLocation.EOUT }, { RoadNodeLocation.NOUT } }},
                    {RoadNodeLocation.WIN, new List<RoadNodeLocation> { { RoadNodeLocation.EOUT },{ RoadNodeLocation.SOUT }, { RoadNodeLocation.NOUT } }}
                }
            }
        };
    }
}

