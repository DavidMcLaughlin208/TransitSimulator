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

    public static IEnumerable<TSource> Also<TSource>(this IEnumerable<TSource> source, Action<TSource> selector) {
        source.ToList().ForEach(i => selector(i));
        return source;
    }

    public static T AddAndGetComponent<T>(this GameObject parent) where T : Component {
        parent.AddComponent<T>();
        return parent.GetComponent<T>();
    }

    public static void assignSpriteFromPath(this GameObject gameObj, string path) {
        gameObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(path);
    }
}