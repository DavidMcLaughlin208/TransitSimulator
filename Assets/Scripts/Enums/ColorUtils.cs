using System;
using UnityEngine;
using System.Collections.Generic;

static class ColorUtils
{
    public static Theme activeTheme = Theme.Light;

    public static Color getColor(Colors colors)
    {
        return colorDict[colors][activeTheme];
    }

    public enum Theme
    {
        Solarized,
        Light,
        Dark
    }

    public enum Colors
    {
        ValidPlacement, InvalidPlacement, SelectedButton, UnselectedButton, Coffee, Tea, Beer, TrainStation,
        Hotel, ParkingLot
    }

    public static Color GetColorForDestType(DestinationType destType)
    {


        if (destType == DestinationType.COFFEE)
        {
            return getColor(Colors.Coffee);
        }
        else if (destType == DestinationType.TEA)
        {
            return getColor(Colors.Tea);
        }
        else if (destType == DestinationType.BEER)
        {
            return getColor(Colors.Tea);
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

    public enum LightTheme
    {
        white, black, grey, lightgrey, blue, lightblue, waterblue, yellow, darkyellow, green, lightgreen,
        darkgreen, orange, darkorange, red

    }

    public static Dictionary<LightTheme, Color> lightTheme = new Dictionary<LightTheme, Color>()
    {
        {LightTheme.white, new Color(255/255f, 255/255f, 255/255f)},
        {LightTheme.black, new Color(0/255f, 0/255f, 0/255f)},
        {LightTheme.grey, new Color(241/255f, 243/255f, 244/255f)},
        {LightTheme.lightgrey, new Color(248/255f, 249/255f, 250/255f)},
        {LightTheme.blue, new Color(51/255f, 118/255f, 224/255f)},
        {LightTheme.lightblue, new Color(115/255f, 159/255f, 240/255f)},
        {LightTheme.waterblue, new Color(162/255f, 192/255f, 244/255f)},
        {LightTheme.yellow, new Color(252/255f, 239/255f, 200/255f)},
        {LightTheme.darkyellow, new Color(249/255f, 226/255f, 157/255f)},
        {LightTheme.darkgreen, new Color(88/255f, 164/255f, 192/255f)},
        {LightTheme.green, new Color(178/255f, 216/255f, 184/255f)},
        {LightTheme.lightgreen, new Color(211/255f, 223/255f, 215/255f)},
        {LightTheme.orange, new Color(230/255f, 156/255f, 55/255f)},
        {LightTheme.darkorange, new Color(241/255f, 182/255f, 87/255f)},
        {LightTheme.red, new Color(204/255f, 104/255f, 101/255f)},
    };

    public static Dictionary<Colors, Dictionary<Theme, Color>> colorDict = new Dictionary<Colors, Dictionary<Theme, Color>>()
    {
        {Colors.ValidPlacement, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.green]},
                {Theme.Light, lightTheme[LightTheme.green]}
            }
        },
        {Colors.InvalidPlacement, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.magenta]},
                {Theme.Light, lightTheme[LightTheme.red]}
            }
        },
        {Colors.SelectedButton, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.blue]},
                {Theme.Light, lightTheme[LightTheme.blue]}
            }
        },
        {Colors.UnselectedButton, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.brblack]},
                {Theme.Light, lightTheme[LightTheme.black]}
            }
        },
        {Colors.Coffee, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.yellow]},
                {Theme.Light, lightTheme[LightTheme.yellow]}
            }
        },
        {Colors.Tea, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.magenta]},
                {Theme.Light, lightTheme[LightTheme.red]}
            }
        },
        {Colors.Beer, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.blue]},
                {Theme.Light, lightTheme[LightTheme.blue]}
            }
        },
        {Colors.TrainStation, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.cyan]},
                {Theme.Light, lightTheme[LightTheme.lightblue]}
            }
        },
        {Colors.Hotel, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.brblack]},
                {Theme.Light, lightTheme[LightTheme.darkorange]}
            }
        },
        {Colors.ParkingLot, new Dictionary<Theme, Color>()
            {
                {Theme.Solarized, solColors[SolarizedColors.brwhite]},
                {Theme.Light, lightTheme[LightTheme.grey]}
            }
        }
    };
}