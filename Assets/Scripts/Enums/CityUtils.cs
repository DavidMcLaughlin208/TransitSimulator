using System;
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

    public static Dictionary<Vector2Int, CityTile?> GetAllTilesInBounds(
        this Dictionary<Vector2Int, CityTile> city,
        Vector2Int topLeftOrigin,
        Vector2Int inclusiveSize
    ) {
        var allCoords = Utils.EndExclusiveRange2D(
            0, inclusiveSize.x + 1, 0, inclusiveSize.y + 1
        ).Select(i => new Vector2Int(topLeftOrigin.x + i.x, topLeftOrigin.y - i.y));

        return allCoords.ToDictionary(i => i, i => city.ContainsKey(i) ? city[i] : null);
    }

    // TODO should this distance be calculated by nodes in mesh? Right now it's only spatial distance (based on transform centerpoint)
    public static List<Lot> GetAllOccupiedLotsByDistance(
        this Dictionary<Vector2Int, CityTile> city,
        Vector2 searchOrigin
    ) {
        // I opted to chain all these calls through multiple variables for easier debugging in the future.

        var groupedCityTiles = city.Values
            .GroupBy(i => i.occupier); // for all city tiles in city, group up all city tiles with the same occupier...
        var firstCityTiles = groupedCityTiles
            .Select(group => group.First()); // and select the first (arbitrarily).
        var allLots = firstCityTiles
            .Where(cityTile => cityTile.nodeTile == null && cityTile.occupier.GetComponent<Lot>() != null); // filter down to all lot occupiers...
        var orderedLots = allLots
            .OrderBy(cityTile =>
                Vector2.Distance(searchOrigin, (Vector2) cityTile.occupier.transform.position) // sort by distance to searchOrigin...
            );
        var actualLots = orderedLots
            .Select(cityTile => cityTile.occupier.GetComponent<Lot>()); // and finally retrieve the lot from each transform.
        return actualLots.ToList();
    }

    public static List<Vector2Int> GetImmediateNeighbors(
        this Dictionary<Vector2Int, CityTile> city,
        Vector2Int searchOrigin
    ) {
        return DirectionUtils.allDirections
                .Select(dir => searchOrigin + Vector2Int.RoundToInt(DirectionUtils.directionToCoordinatesMapping[dir]))
                .Where(coord => city.Keys.Contains(coord))
                .ToList();
    }
}