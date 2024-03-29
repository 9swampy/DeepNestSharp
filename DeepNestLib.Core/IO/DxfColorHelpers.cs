﻿namespace DeepNestLib.IO
{
  using System.Collections.ObjectModel;
  using IxMilia.Dxf;

  public static class DxfColorHelpers
  {
    public static DxfColor GetClosestDefaultIndexColor(byte r, byte g, byte b)
    {
      var minDist = int.MaxValue;
      var minIndex = -1;
      ReadOnlyCollection<uint> colors = DxfColor.DefaultColors;
      // index 0 is left out intentionally!
      for (var i = 1; i < colors.Count; i++)
      {
        var sqd = SquaredColorDistance(r, g, b, colors[i]);
        if (sqd == 0) // exact match
        {
          return DxfColor.FromIndex((byte)i);
        }
        if (sqd < minDist)
        {
          minDist = sqd;
          minIndex = i;
        }
      }
      return DxfColor.FromIndex((byte)minIndex);
    }

    private static int SquaredColorDistance(byte r, byte g, byte b, uint otherColor)
    {
      (var r2, var g2, var b2) = ToRgb(otherColor);
      return (r - r2) * (r - r2)
           + (g - g2) * (g - g2)
           + (b - b2) * (b - b2);
    }

    public static (byte, byte, byte) ToRgb(uint color)
    {
      //byte a = (byte)(color >> 24);
      var r = (byte)(color >> 16);
      var g = (byte)(color >> 8);
      var b = (byte)(color >> 0);
      return (r, g, b);
    }

    public static uint FromRgb(byte r, byte g, byte b)
    {
      const byte a = 0xFF; // alpha not used
      return (uint)(a << 24 | r << 16 | g << 8 | b << 0);
    }

    public static string ToHexString(this DxfColor color)
    {
      if (color.IsIndex)
      {
        var argb = DxfColor.DefaultColors[color.Index];
        return ToHexString(argb);
      }
      return color.ToString();
    }

    public static string ToHexString(uint color)
    {
      return "0x" + color.ToString("X8");
    }
  }
}