//  _____                                             
// |_   _|                                            
//   | | ___   ___ _ __ ___  _ __ ___  ___  ___ _ __  
//   | |/ _ \ / _ \ '_ ` _ \| '_ ` _ \/ __|/ _ \ '_ \ 
//   | | (_) |  __/ | | | | | | | | | \__ \  __/ | | |
//   \_/\___/ \___|_| |_| |_|_| |_| |_|___/\___|_| |_|
//                                            
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace ZumoLib;

public class ColorSensor : ComDevice
{

    public ColorSensor(ICom com) : base(com, 0x31) { }

    public static string RGBToReadable(string rgbHex)
    {
        if (string.IsNullOrWhiteSpace(rgbHex) || !rgbHex.StartsWith("#") || rgbHex.Length != 7)
            return "Invalid";

        try
        {
            int r = Convert.ToInt32(rgbHex.Substring(1, 2), 16);
            int g = Convert.ToInt32(rgbHex.Substring(3, 2), 16);
            int b = Convert.ToInt32(rgbHex.Substring(5, 2), 16);
            return $"R: {r}, G: {g}, B: {b}";
        }
        catch
        {
            return "Invalid";
        }
    }

    public string ReadColorHUE()
    {
        string response = GetRequest(5, 0x31, "0");
        
        if (response.Length == 8) // Expected format: "5*3100F3"
        {
            string hueHex = "0x" + response.Substring(4, 4);
            return hueHex;
        }

        return "Invalid";
    }
    public string ReadColorRGB()
    {
        string response = GetRequest(5, 0x31, "0");
        if (response.Length == 8)
        {
            string hueHex = "0x" + response.Substring(4, 4);
            return HUEAngleToRGB(hueHex);
        }
        return "Invalid";
    }

    public static string HUEAngleToRGB(string angle)
    {
        if (string.IsNullOrWhiteSpace(angle))
            return "Invalid";

        // Try to parse the angle as integer (hex or decimal)
        int hue;
        if (angle.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(angle.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out hue))
                return "Invalid";
        }
        else if (!int.TryParse(angle, out hue))
        {
            return "Invalid";
        }

        // Check for special invalid value (e.g., 0xFFFF or 65535)
        if (hue == 0xFFFF || hue == 65535)
            return "Invalid";

        // Clamp hue to [0, 359]
        hue = ((hue % 360) + 360) % 360;

        // Convert HUE to RGB (HSV with S=1, V=1)
        double h = hue / 60.0;
        int i = (int)Math.Floor(h);
        double f = h - i;
        double v = 1.0, s = 1.0;
        double p = v * (1 - s);
        double q = v * (1 - s * f);
        double t = v * (1 - s * (1 - f));
        double r = 0, g = 0, b = 0;
        switch (i)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
        }
        int R = (int)(r * 255);
        int G = (int)(g * 255);
        int B = (int)(b * 255);
        return $"#{R:X2}{G:X2}{B:X2}";
    }
    public string Calibrate(CalibrationColor color)
    {
        return SetRequest(5, 0x31, $"60{((int)color)}");
    }

    public enum CalibrationColor
    {
        Black = 0,
        White = 1,
    }

}
