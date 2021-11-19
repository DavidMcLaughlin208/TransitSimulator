using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class Utils {
    
    private static System.Random rng = new System.Random();  

    public static T getRandomElement<T>(this IEnumerable<T> list) {
        return list.OrderBy(i => rng.Next()).First();
    }

    public static List<T> getManyRandomElements<T>(this IEnumerable<T> list, int number) {
        return list.OrderBy(i => rng.Next()).Take(number).ToList();
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
    
    public static List<T> Shuffled<T>(this List<T> list) {
        return list.OrderBy(x => Guid.NewGuid()).ToList();
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

    /// <summary>
    /// This will calculate center points for elements to be spaced out horizontally within a container.
    /// Once the container width is reached, the margin between elements will shrink in order to keep all elements
    /// evenly spaced.
    /// Container width can be tricky to get right, so toy around with the settings until it looks right. Calculations
    /// seem to be working fine, so I have a feeling this is more likely due to some UI / World spacing discrepancy I'm not aware of.
    /// </summary>
    public static List<Vector2> getCenterPointsInHorizontalSpread(
        Vector2 containerCenter, 
        float containerWidth,
        int numElements,
        float elementWidth,
        float marginRatio=0.1f // the margin that will attempt to buffer two elements compared to elementWidth
    ) {
        var centers = new List<Vector2>();
        var widthWithMargins = (elementWidth + (elementWidth * marginRatio) * 2) * numElements;
        if (widthWithMargins <= containerWidth || containerWidth == float.MaxValue) { // try to center elements with their margins around the center point if it fits in the container
            var leftMostElementCenter = 
                containerCenter - // from the center...
                new Vector2(widthWithMargins / 2, 0) + // go to the left of the total elements' span 
                new Vector2((elementWidth / 2) + (elementWidth * marginRatio), 0); // and then go right a single margin and half of an element width
            for (var i = 0; i < numElements; i++) {
                centers.Add(
                    leftMostElementCenter + 
                    new Vector2( i * (elementWidth + (elementWidth * marginRatio) * 2),0)
                );
            }
        }
        else {
            var sectionWidth = containerWidth / numElements;
            var leftBorder = containerCenter - new Vector2(containerWidth / 2, 0);
            for (var i = 0; i < numElements; i++) {
                centers.Add(
                    leftBorder +
                    new Vector2(sectionWidth * i + (sectionWidth / 2),0)
                    );
            }
        }
        return centers;
    }
}