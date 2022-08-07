namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
#if NCRUNCH
  using System.Text;
#endif
  using ClipperLib;
  using DeepNestLib.Placement;

  public static class DebugFileExtensions
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

    public static string ToFauxSheetPlacement(this List<List<IntPoint>> clipperSheetNfp, PlacementTypeEnum placementType, ISheet sheet, double clipperScale)
    {
      var nfpPoints = clipperSheetNfp.Select(z => z.ToArray()).ToArray();
      var partPlacements = new List<PartPlacement>();
      for (var polygonIdx = 0; polygonIdx < nfpPoints.Length; polygonIdx++)
      {
        var svgPoints = new SvgPoint[clipperSheetNfp[polygonIdx].Count];
        var nfpCoordinates = clipperSheetNfp[polygonIdx].ToArray().ToNestCoordinates(clipperScale);
        for (var p = 0; p < nfpCoordinates.Length; p++)
        {
          svgPoints[p] = new SvgPoint(nfpCoordinates[p].X, nfpCoordinates[p].Y);
        }

        var nfp = new NoFitPolygon(svgPoints);
        partPlacements.Add(new PartPlacement(nfp));
      }

      return new SheetPlacement(placementType, sheet, partPlacements, 0).ToJson(true);
    }

    public static string ToFauxSheetPlacement(this IList<INfp> listNfps, PlacementTypeEnum placementType, ISheet sheet, double clipperScale)
    {
      var partPlacements = new List<PartPlacement>();
      foreach (var nfp in listNfps)
      {
        partPlacements.Add(new PartPlacement(nfp));
      }

      return new SheetPlacement(placementType, sheet, partPlacements, 0).ToJson(true);
    }
  }
}