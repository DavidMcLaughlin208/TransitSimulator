using System.Collections.Generic;
using System.Linq;
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

    public static void assignSpriteFromPath(this GameObject gameObj, string path) {
        gameObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(path);
    }
}