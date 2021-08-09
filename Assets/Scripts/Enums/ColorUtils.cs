using System;
using UnityEngine;

static class ColorUtils
{
    public static Color GetColorForShopType(ShopType shopType)
    {
        if (shopType == ShopType.COFEE)
        {
            return Color.red;
        }
        else if (shopType == ShopType.TEA)
        {
            return Color.blue;
        }
        else if (shopType == ShopType.BEER)
        {
            return Color.green;
        }
        return Color.white;
    }
}