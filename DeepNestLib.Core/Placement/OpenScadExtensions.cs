namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif
  using ClipperLib;
  using DeepNestLib.Geometry;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using Light.GuardClauses;

  public static class OpenScadExtensions
  {
    public static string ToOpenScadPolygon(this List<List<IntPoint>> clipperSheetNfp)
    {
      return ToOpenScadPolygon(clipperSheetNfp.Select(z => z.ToArray()).ToArray());
    }

    public static string ToOpenScadPolygon(this IntPoint[][] clipperSheetNfp)
    {
      var resultBuilder = new StringBuilder();
      for (var polygonIdx = 0; polygonIdx < clipperSheetNfp.Length; polygonIdx++)
      {
        resultBuilder.AppendLine("polygon ([");
        foreach (var p in clipperSheetNfp[polygonIdx])
        {
          resultBuilder.AppendLine($"[{p.X:0.######},{p.Y:0.######}],");
        }

        resultBuilder.AppendLine("]);");
      }

      return resultBuilder.ToString();
    }

    public static string ToOpenScadPolygon(this IEnumerable<INfp> nfps)
    {
      var openScadBuilder = new StringBuilder();
      foreach (var item in nfps)
      {
        openScadBuilder.AppendLine(item.ToOpenScadPolygon());
      }

      return openScadBuilder.ToString();
    }
  }
}