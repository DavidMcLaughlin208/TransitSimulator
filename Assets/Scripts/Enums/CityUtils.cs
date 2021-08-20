using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class CityUtils {
    public static List<CityTile> GetAllBorderingNeighborsInDirection(this Dictionary<Vector2Int, CityTile> city, Direction direction, Vector2Int origin) {
        var unitInDirection = Vector2Int.RoundToInt(DirectionUtils.directionToCoordinatesMapping[direction]);

        var firstBound = city.SearchForEdgeInDirection(DirectionUtils.directionRotationMapping[Rotation.TWOSEVENTY][direction], origin);
        var secondBound = city.SearchForEdgeInDirection(DirectionUtils.directionRotationMapping[Rotation.NINETY][direction], origin);
        var neighborIndex = city.SearchForEdgeInDirection(direction, origin) + unitInDirection;

        // To determine which X or Y component to take from the bounds or the neighborIndex, use absolute value:
        // the components will be the same for N/S, and E/W.
        unitInDirection = new Vector2Int(Mathf.Abs(unitInDirection.x), Mathf.Abs(unitInDirection.y));

        if (firstBound != null && secondBound != null && neighborIndex != null) {
            var firstNeighbor = new Vector2Int(
                neighborIndex.x * unitInDirection.x + firstBound.x * unitInDirection.y,
                firstBound.y * unitInDirection.x + neighborIndex.y * unitInDirection.y
            );
            var lastNeighbor = new Vector2Int(
                neighborIndex.x * unitInDirection.x + secondBound.x * unitInDirection.y,
                secondBound.y * unitInDirection.x + neighborIndex.y * unitInDirection.y
            );

            var line = Utils.EndExclusiveRange2D(
                Mathf.Min(firstNeighbor.x, lastNeighbor.x),
                Mathf.Max(firstNeighbor.x, lastNeighbor.x) + 1,
                Mathf.Min(firstNeighbor.y, lastNeighbor.y),
                Mathf.Max(firstNeighbor.y, lastNeighbor.y) + 1
            );

            return line
                .Where(coord => city.Keys.Contains(coord))
                .Select(i => city[i])
                .ToList();
        }

        return new List<CityTile>();
    }

    public static Vector2Int SearchForEdgeInDirection(this Dictionary<Vector2Int, CityTile> city, Direction direction, Vector2Int origin, int maxSearchIterations=10) {
        var offsetDirection = Vector2Int.RoundToInt(DirectionUtils.directionToCoordinatesMapping[direction]);
        var originOccupier = city[origin].occupier;
        for (int i = 0; i < maxSearchIterations; i++) {
            var offset = origin + (offsetDirection * i);
            if (city[offset].occupier != originOccupier) {
                return offset - offsetDirection; // return the edge tile from the originOccupier
            }
        }
        return origin;
    }
}