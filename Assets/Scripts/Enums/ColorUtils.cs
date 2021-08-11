using System;
using UnityEngine;
using System.Collections.Generic;

static class ColorUtils
{
    public static Color GetColorForShopType(ShopType shopType)
    {
        if (shopType == ShopType.COFFEE)
        {
            return solColors[SolarizedColors.brblack];
        }
        else if (shopType == ShopType.TEA)
        {
            return solColors[SolarizedColors.brmagenta];
        }
        else if (shopType == ShopType.BEER)
        {
            return solColors[SolarizedColors.brcyan];
        }
        return Color.white;
    }

    public enum SolarizedColors {
        brblack, brwhite, bryellow, brred, brmagenta, brblue, brcyan, brgreen,
        black, white, yellow, red, magenta, blue, cyan, green,
    }

    public static Dictionary<SolarizedColors, Color> solColors = new Dictionary<SolarizedColors, Color>() {
        {SolarizedColors.brblack, new Color(0f/255f, 43f/255f, 54f/255f)},
        {SolarizedColors.brwhite, new Color(253f/255f, 246f/255f, 227f/255f)},
        {SolarizedColors.bryellow, new Color(101f/255f, 123f/255f, 131f/255f)},
        {SolarizedColors.brred, new Color(203f/255f, 75f/255f, 22f/255f)},
        {SolarizedColors.brmagenta, new Color(108f/255f, 113f/255f, 196f/255f)},
        {SolarizedColors.brblue, new Color(131f/255f, 148f/255f, 150f/255f)},
        {SolarizedColors.brcyan, new Color(147f/255f, 161f/255f, 161f/255f)},
        {SolarizedColors.brgreen, new Color(88f/255f, 110f/255f, 117f/255f)},
        {SolarizedColors.black, new Color(7f/255f, 54f/255f, 66f/255f)},
        {SolarizedColors.white, new Color(238f/255f, 232f/255f, 213f/255f)},
        {SolarizedColors.yellow, new Color(181f/255f, 137f/255f, 0f/255f)},
        {SolarizedColors.red, new Color(211f/255f, 1f/255f, 2f/255f)},
        {SolarizedColors.magenta, new Color(211f/255f, 54f/255f, 130f/255f)},
        {SolarizedColors.blue, new Color(38f/255f, 139f/255f, 210f/255f)},
        {SolarizedColors.cyan, new Color(42f/255f, 161f/255f, 152f/255f)},
        {SolarizedColors.green, new Color(133f/255f, 153f/255f, 0f/255f)},
    };
}