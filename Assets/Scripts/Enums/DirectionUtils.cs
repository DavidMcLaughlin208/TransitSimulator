using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public static List<Direction> allDirections = new List<Direction>() {
        Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST
    };

    public static Dictionary<Direction, Rotation> generalRotationMapping = new Dictionary<Direction, Rotation>() {
        {Direction.NORTH, Rotation.ZERO},
        {Direction.SOUTH, Rotation.ONEEIGHTY},
        {Direction.EAST, Rotation.NINETY},
        {Direction.WEST, Rotation.TWOSEVENTY},
    };

    // This is reversed because Unity counts Z-rotation counter clockwise and I coded it clockwise before I realized this
    // TODO: Make our rotation logic counter clockwise
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
                    {PedestrianNodeLocation.TR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TL, PedestrianNodeLocation.BR}},
                    {PedestrianNodeLocation.TL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.TR, PedestrianNodeLocation.BL}},
                    {PedestrianNodeLocation.BR, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BL, PedestrianNodeLocation.TR}},
                    {PedestrianNodeLocation.BL, new List<PedestrianNodeLocation>() {PedestrianNodeLocation.BR, PedestrianNodeLocation.TL}}
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
            },
            {
                RoadType.TJunction, new Dictionary<RoadNodeLocation, List<RoadNodeLocation>>
                {
                    {RoadNodeLocation.NIN, new List<RoadNodeLocation> { } },
                    {RoadNodeLocation.EIN, new List<RoadNodeLocation> { { RoadNodeLocation.WOUT },{ RoadNodeLocation.SOUT } }},
                    {RoadNodeLocation.SIN, new List<RoadNodeLocation> { { RoadNodeLocation.WOUT },{ RoadNodeLocation.EOUT } }},
                    {RoadNodeLocation.WIN, new List<RoadNodeLocation> { { RoadNodeLocation.EOUT },{ RoadNodeLocation.SOUT } }}
                }
            },
            {
                RoadType.Corner, new Dictionary<RoadNodeLocation, List<RoadNodeLocation>>
                {
                    {RoadNodeLocation.NIN, new List<RoadNodeLocation> { } },
                    {RoadNodeLocation.EIN, new List<RoadNodeLocation> { { RoadNodeLocation.SOUT } }},
                    {RoadNodeLocation.SIN, new List<RoadNodeLocation> { { RoadNodeLocation.EOUT } }},
                    {RoadNodeLocation.WIN, new List<RoadNodeLocation> { } }
                }
            }
        };

        public static Dictionary<RoadNodeLocation, List<Direction>> nodeExternalConnectionDirections = new Dictionary<RoadNodeLocation, List<Direction>>()
        {
            {RoadNodeLocation.NOUT, new List<Direction>() {Direction.NORTH}},
            {RoadNodeLocation.EOUT, new List<Direction>() {Direction.EAST}},
            {RoadNodeLocation.SOUT, new List<Direction>() {Direction.SOUTH}},
            {RoadNodeLocation.WOUT, new List<Direction>() {Direction.WEST}},
        };

        public static Dictionary<Direction, Dictionary<RoadNodeLocation, RoadNodeLocation>> externalConnectionMapping = new Dictionary<Direction, Dictionary<RoadNodeLocation, RoadNodeLocation>>()
        {
            {
                Direction.NORTH, new Dictionary<RoadNodeLocation, RoadNodeLocation>() { { RoadNodeLocation.NOUT, RoadNodeLocation.SIN } }
            },
            {
                Direction.EAST, new Dictionary<RoadNodeLocation, RoadNodeLocation>() { { RoadNodeLocation.EOUT, RoadNodeLocation.WIN } }
            },
            {
                Direction.SOUTH, new Dictionary<RoadNodeLocation, RoadNodeLocation>() { { RoadNodeLocation.SOUT, RoadNodeLocation.NIN } }
            },
            {
                Direction.WEST, new Dictionary<RoadNodeLocation, RoadNodeLocation>() { { RoadNodeLocation.WOUT, RoadNodeLocation.EIN } }
            }
        };

        public static RoadNodeLocation Rotate(RoadNodeLocation location, Rotation rotation)
        {
            return rotationMapping[rotation][location];
        }

        public static Dictionary<Rotation, Dictionary<RoadNodeLocation, RoadNodeLocation>> rotationMapping = new Dictionary<Rotation, Dictionary<RoadNodeLocation, RoadNodeLocation>>()
        {
            {
                Rotation.ZERO, new Dictionary<RoadNodeLocation, RoadNodeLocation> {
                    {RoadNodeLocation.NIN, RoadNodeLocation.NIN},
                    {RoadNodeLocation.NOUT, RoadNodeLocation.NOUT},
                    {RoadNodeLocation.EIN, RoadNodeLocation.EIN},
                    {RoadNodeLocation.EOUT, RoadNodeLocation.EOUT},
                    {RoadNodeLocation.SIN, RoadNodeLocation.SIN},
                    {RoadNodeLocation.SOUT, RoadNodeLocation.SOUT},
                    {RoadNodeLocation.WIN, RoadNodeLocation.WIN},
                    {RoadNodeLocation.WOUT, RoadNodeLocation.WOUT},
                }
            },
            {
                Rotation.NINETY, new Dictionary<RoadNodeLocation, RoadNodeLocation> {
                    {RoadNodeLocation.NIN, RoadNodeLocation.EIN},
                    {RoadNodeLocation.NOUT, RoadNodeLocation.EOUT},
                    {RoadNodeLocation.EIN, RoadNodeLocation.SIN},
                    {RoadNodeLocation.EOUT, RoadNodeLocation.SOUT},
                    {RoadNodeLocation.SIN, RoadNodeLocation.WIN},
                    {RoadNodeLocation.SOUT, RoadNodeLocation.WOUT},
                    {RoadNodeLocation.WIN, RoadNodeLocation.NIN},
                    {RoadNodeLocation.WOUT, RoadNodeLocation.NOUT},
                }
            },
            {
                Rotation.ONEEIGHTY, new Dictionary<RoadNodeLocation, RoadNodeLocation> {
                    {RoadNodeLocation.NIN, RoadNodeLocation.SIN},
                    {RoadNodeLocation.NOUT, RoadNodeLocation.SOUT},
                    {RoadNodeLocation.EIN, RoadNodeLocation.WIN},
                    {RoadNodeLocation.EOUT, RoadNodeLocation.WOUT},
                    {RoadNodeLocation.SIN, RoadNodeLocation.NIN},
                    {RoadNodeLocation.SOUT, RoadNodeLocation.NOUT},
                    {RoadNodeLocation.WIN, RoadNodeLocation.EIN},
                    {RoadNodeLocation.WOUT, RoadNodeLocation.EOUT},
                }
            },
            {
                Rotation.TWOSEVENTY, new Dictionary<RoadNodeLocation, RoadNodeLocation> {
                    {RoadNodeLocation.NIN, RoadNodeLocation.WIN},
                    {RoadNodeLocation.NOUT, RoadNodeLocation.WOUT},
                    {RoadNodeLocation.EIN, RoadNodeLocation.NIN},
                    {RoadNodeLocation.EOUT, RoadNodeLocation.NOUT},
                    {RoadNodeLocation.SIN, RoadNodeLocation.EIN},
                    {RoadNodeLocation.SOUT, RoadNodeLocation.EOUT},
                    {RoadNodeLocation.WIN, RoadNodeLocation.SIN},
                    {RoadNodeLocation.WOUT, RoadNodeLocation.SOUT},
                }
            },
        };

        public static Dictionary<RoadType, List<RoadNodeLocation>> disabledNodeMapping = new Dictionary<RoadType, List<RoadNodeLocation>>()
        {
            {
                RoadType.Straight, new List<RoadNodeLocation> { {RoadNodeLocation.NIN}, {RoadNodeLocation.NOUT}, {RoadNodeLocation.SIN}, {RoadNodeLocation.SOUT} }
            },
            {
                RoadType.Intersection, new List<RoadNodeLocation> { }
            },
            {
                RoadType.TJunction, new List<RoadNodeLocation> { {RoadNodeLocation.NIN}, {RoadNodeLocation.NOUT} }
            },
            {
                RoadType.Corner, new List<RoadNodeLocation> { {RoadNodeLocation.NIN}, {RoadNodeLocation.NOUT}, {RoadNodeLocation.WIN}, {RoadNodeLocation.WOUT} }
            },
        };

        public static Dictionary<Direction[], RoadTypeAndRotation> missingNeighborsToRoadTypeAndOrientationMap = new Dictionary<Direction[], RoadTypeAndRotation>(new ArrayComparer())
        {
            {new List<Direction> {{Direction.NORTH}, {Direction.SOUTH} }.ToArray(), new RoadTypeAndRotation(RoadType.Straight, Rotation.ZERO) },
            {new List<Direction> {{Direction.EAST}, {Direction.WEST} }.ToArray(), new RoadTypeAndRotation(RoadType.Straight, Rotation.NINETY) },
            {new List<Direction> {{Direction.NORTH}, {Direction.WEST} }.ToArray(), new RoadTypeAndRotation(RoadType.Corner, Rotation.ZERO) },
            {new List<Direction> {{Direction.NORTH}, {Direction.EAST} }.ToArray(), new RoadTypeAndRotation(RoadType.Corner, Rotation.NINETY) },
            {new List<Direction> {{Direction.EAST}, {Direction.SOUTH} }.ToArray(), new RoadTypeAndRotation(RoadType.Corner, Rotation.ONEEIGHTY) },
            {new List<Direction> {{Direction.SOUTH}, {Direction.WEST} }.ToArray(), new RoadTypeAndRotation(RoadType.Corner, Rotation.TWOSEVENTY) },
            {new List<Direction> {}.ToArray(), new RoadTypeAndRotation(RoadType.Intersection, Rotation.ZERO) },
            {new List<Direction> {{Direction.NORTH}}.ToArray(), new RoadTypeAndRotation(RoadType.TJunction, Rotation.ZERO) },
            {new List<Direction> {{Direction.EAST}}.ToArray(), new RoadTypeAndRotation(RoadType.TJunction, Rotation.NINETY) },
            {new List<Direction> {{Direction.SOUTH}}.ToArray(), new RoadTypeAndRotation(RoadType.TJunction, Rotation.ONEEIGHTY) },
            {new List<Direction> {{Direction.WEST}}.ToArray(), new RoadTypeAndRotation(RoadType.TJunction, Rotation.TWOSEVENTY) },

        };

        public struct RoadTypeAndRotation {
            public RoadType roadType;
            public Rotation rotation;

            public RoadTypeAndRotation(RoadType roadType, Rotation rotation)
            {
                this.roadType = roadType;
                this.rotation = rotation;
            }
        }

        public static RoadTypeAndRotation GetRoadTypeAndRotationForMissingNeighbors(List<Direction> missingNeighbors)
        {
            missingNeighbors.Sort();
            return missingNeighborsToRoadTypeAndOrientationMap[missingNeighbors.ToArray()];
        }

        public static Dictionary<RoadNodeLocation, Vector2> nodeLocationVectors = new Dictionary<RoadNodeLocation, Vector2>()
        {
            {RoadNodeLocation.NIN, new Vector2(0, -1)},
            {RoadNodeLocation.NOUT, new Vector2(0, 1)},
            {RoadNodeLocation.EIN, new Vector2(-1, 0)},
            {RoadNodeLocation.EOUT, new Vector2(1, 0)},
            {RoadNodeLocation.SIN, new Vector2(0, 1)},
            {RoadNodeLocation.SOUT, new Vector2(0, -1)},
            {RoadNodeLocation.WIN, new Vector2(1, 0)},
            {RoadNodeLocation.WOUT, new Vector2(-1, 0)}
        };
    }

    sealed class ArrayComparer : EqualityComparer<Direction[]>
    {
        public override bool Equals(Direction[] x, Direction[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }


        public override int GetHashCode(Direction[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + (int)obj[i];
                }
            }
            return result;
        }
    }

    public static class IntersectionUtils
    {
        public enum Turn
        {
            STRAIGHT,
            RIGHT,
            LEFT
        }

        public static Dictionary<Turn, List<IntersectionTile>> turnsToTileLockMapping = new Dictionary<Turn, List<IntersectionTile>>()
        {
            {Turn.STRAIGHT, new List<IntersectionTile>() {IntersectionTile.BR, IntersectionTile.TR} },
            {Turn.RIGHT, new List<IntersectionTile>() {IntersectionTile.BR} },
            {Turn.LEFT, new List<IntersectionTile>() {IntersectionTile.BR, IntersectionTile.TR, IntersectionTile.TL} }
        };

        public static Dictionary<Direction, Dictionary<IntersectionTile, IntersectionTile>> rotatedTurnTileLockMapping = new Dictionary<Direction, Dictionary<IntersectionTile, IntersectionTile>>()
        {
            {Direction.SOUTH, new Dictionary<IntersectionTile, IntersectionTile>()
                {
                    {IntersectionTile.TL, IntersectionTile.TL},
                    {IntersectionTile.TR, IntersectionTile.TR},
                    {IntersectionTile.BL, IntersectionTile.BL},
                    {IntersectionTile.BR, IntersectionTile.BR}
                }
            },
            {Direction.WEST, new Dictionary<IntersectionTile, IntersectionTile>()
                {
                    {IntersectionTile.TL, IntersectionTile.TR},
                    {IntersectionTile.TR, IntersectionTile.BR},
                    {IntersectionTile.BL, IntersectionTile.TL},
                    {IntersectionTile.BR, IntersectionTile.BL}
                }
            },
            {Direction.NORTH, new Dictionary<IntersectionTile, IntersectionTile>()
                {
                    {IntersectionTile.TL, IntersectionTile.BR},
                    {IntersectionTile.TR, IntersectionTile.BL},
                    {IntersectionTile.BL, IntersectionTile.TR},
                    {IntersectionTile.BR, IntersectionTile.TL}
                }
            },
            {Direction.EAST, new Dictionary<IntersectionTile, IntersectionTile>()
                {
                    {IntersectionTile.TL, IntersectionTile.BL},
                    {IntersectionTile.TR, IntersectionTile.TL},
                    {IntersectionTile.BL, IntersectionTile.BR},
                    {IntersectionTile.BR, IntersectionTile.TR}
                }
            },
        };

        public static List<IntersectionTile> GetTileLockForTurnTypeAndDirection(Turn turnType, Direction dir)
        {
            List<IntersectionTile> unrotatedTiles = turnsToTileLockMapping[turnType];
            List<IntersectionTile> rotatedTiles = unrotatedTiles.Select(tile => rotatedTurnTileLockMapping[dir][tile]).ToList();
            return rotatedTiles;
        }

        public static Dictionary<RoadNodeLocation, Direction> locationToDirectionMapping = new Dictionary<RoadNodeLocation, Direction>()
        {
            {RoadNodeLocation.SIN, Direction.SOUTH },
            {RoadNodeLocation.WIN, Direction.WEST },
            {RoadNodeLocation.NIN, Direction.NORTH },
            {RoadNodeLocation.EIN, Direction.EAST }
        };

        public static Dictionary<(RoadNodeLocation, RoadNodeLocation), Turn> nodeToTurnMapping = new Dictionary<(RoadNodeLocation, RoadNodeLocation), Turn>()
        {
            {(RoadNodeLocation.SIN, RoadNodeLocation.NOUT), Turn.STRAIGHT },
            {(RoadNodeLocation.SIN, RoadNodeLocation.EOUT), Turn.RIGHT },
            {(RoadNodeLocation.SIN, RoadNodeLocation.WOUT), Turn.LEFT },

            {(RoadNodeLocation.WIN, RoadNodeLocation.EOUT), Turn.STRAIGHT },
            {(RoadNodeLocation.WIN, RoadNodeLocation.SOUT), Turn.RIGHT },
            {(RoadNodeLocation.WIN, RoadNodeLocation.NOUT), Turn.LEFT },

            {(RoadNodeLocation.NIN, RoadNodeLocation.SOUT), Turn.STRAIGHT },
            {(RoadNodeLocation.NIN, RoadNodeLocation.WOUT), Turn.RIGHT },
            {(RoadNodeLocation.NIN, RoadNodeLocation.EOUT), Turn.LEFT },

            {(RoadNodeLocation.EIN, RoadNodeLocation.WOUT), Turn.STRAIGHT },
            {(RoadNodeLocation.EIN, RoadNodeLocation.NOUT), Turn.RIGHT },
            {(RoadNodeLocation.EIN, RoadNodeLocation.SOUT), Turn.LEFT }
        };

        public static Turn GetTurnTypeForNodeLocations((RoadNodeLocation, RoadNodeLocation) nodeTuple)
        {
            if (nodeToTurnMapping.ContainsKey(nodeTuple))
            {
                return nodeToTurnMapping[nodeTuple];
            } else
            {
                Debug.Log("Lacking Tuple: " + nodeTuple.Item1 + ", " + nodeTuple.Item2);
                return Turn.STRAIGHT;
            }
            
        }

        public static Dictionary<Turn, List<float>> intersectionTileReleaseMapping = new Dictionary<Turn, List<float>>()
        {
            {Turn.STRAIGHT, new List<float>() {1f, 0.50f } },
            {Turn.LEFT, new List<float>() {1f, 0.66f, 0.33f} },
            {Turn.RIGHT, new List<float>() {1f} },
        };

    }
}

