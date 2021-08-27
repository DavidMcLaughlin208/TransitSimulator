using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Block {

}

public static class BlockOrientations {

    // Tilemap +X,+Y quadrant is up and to the right
    // O in diagram is origin (0,0)

    //  O X X
    //  X
    public static List<Vector2Int> L = new List<Vector2Int>() {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(2,0),
        new Vector2Int(0,-1),
    };

    //  O
    //  X X X
    public static List<Vector2Int> J = new List<Vector2Int>() {
        new Vector2Int(0,0),
        new Vector2Int(0,-1),
        new Vector2Int(1,-1),
        new Vector2Int(2,-1),
    };

    //  O X
    //  X X
    public static List<Vector2Int> O = new List<Vector2Int>() {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(1,-1),
    };

    //  O X X X
    public static List<Vector2Int> I = new List<Vector2Int>() {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(2,0),
        new Vector2Int(3,0),
    };

    //  X
    //  O X
    //  X
    public static List<Vector2Int> T = new List<Vector2Int>() {
        new Vector2Int(0, 1),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
    };

    //   O X
    // X X
    public static List<Vector2Int> S = new List<Vector2Int>() {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
    };

    // X O
    //   X X
    public static List<Vector2Int> Z = new List<Vector2Int>() {
        new Vector2Int(0, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
    };

    public static List<Vector2Int> single = new List<Vector2Int>() {
        new Vector2Int(0,0)
    };

    public static List<List<Vector2Int>> allOrientations = new List<List<Vector2Int>>() {
        BlockOrientations.I,
        BlockOrientations.J,
        BlockOrientations.L,
        BlockOrientations.O,
        BlockOrientations.T,
        BlockOrientations.S,
        BlockOrientations.Z,
    };

    public static List<Vector2Int> RotateClockwise(this List<Vector2Int> blockOrientation) {
        return blockOrientation.Select(i => {
            return new Vector2Int(i.y, i.x * -1);
        }).ToList();
    }

    public static List<Vector2Int> RotateCounterClockwise(this List<Vector2Int> blockOrientation) {
        return blockOrientation.Select(i => {
            return new Vector2Int(i.y * -1, i.x);
        }).ToList();
    }
}