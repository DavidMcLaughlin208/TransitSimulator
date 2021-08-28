using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class Utils {

    public static T getRandomElement<T>(this IEnumerable<T> list) {
        var rnd = new System.Random();
        return list.OrderBy(i => rnd.Next()).First();
    }

    public static List<T> getManyRandomElements<T>(this IEnumerable<T> list, int number) {
        var rnd = new System.Random();
        return list.OrderBy(i => rnd.Next()).Take(number).ToList();
    }

    public static List<Vector2Int> EndExclusiveRange2D(int xStart, int xMax, int yStart, int yMax) {
        var values = new List<Vector2Int>();
        for (int x = xStart; x < xMax; x++) {
            for (int y = yStart; y < yMax; y++) {
                values.Add(new Vector2Int(x, y));
            }
        }
        return values;
    }

    public static Vector2Int ToVec2(this Vector3Int source) {
        return new Vector2Int(source.x, source.y);
    }

    public static IEnumerable<TSource> Also<TSource>(this IEnumerable<TSource> source, Action<TSource> selector) {
        source.ToList().ForEach(i => selector(i));
        return source;
    }

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) {
        return source.Select((item, index) => (item, index));
    }

    public static T AddAndGetComponent<T>(this GameObject parent) where T : Component {
        parent.AddComponent<T>();
        return parent.GetComponent<T>();
    }

    public static void assignSpriteFromPath(this GameObject gameObj, string path) {
        gameObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(path);
    }

    public static float getLengthOfQuadraticCurve(Car.Curve curve, int numPoints)
    {
        float originPosition = curve.currentPlace;
        int localNumPoints = Mathf.Max(numPoints, 3);
        float length = 0;
        float rateIncrease = 1f / (float) localNumPoints;
        Vector2 lastPoint = curve.GetCurrentPosition();
        for (int i = 1; i < localNumPoints - 1; i++)
        {
            curve.currentPlace = i * rateIncrease;
            Vector3 newPoint = curve.GetCurrentPosition();
            length += Vector2.Distance(lastPoint, newPoint);
            lastPoint = newPoint;
        }
        curve.currentPlace = originPosition;
        return length;
    }
}