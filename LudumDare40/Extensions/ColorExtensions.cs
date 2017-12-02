using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace LudumDare40.Extensions
{
    public static class ColorExtensions
    {
        // From https://github.com/Cyral/Cyral.Extensions/blob/master/Extensions/Xna/ColorExtensions.cs
        public static Color FromHex(this string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Color color = Color.White;
            if (hexString.Length == 8)
            {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else if (hexString.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else
            {
                throw new InvalidOperationException("Invald hex representation of an ARGB or RGB color value.");
            }
            return color;
        }
    }
}
