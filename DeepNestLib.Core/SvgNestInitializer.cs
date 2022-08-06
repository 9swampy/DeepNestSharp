namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class SvgNestInitializer
  {
    public static (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) BuildNestItems(ISvgNestConfig config, ICollection<INfp> parts, IList<ISheet> sheets, IProgressDisplayer progressDisplayer)
    {
      List<NestItem<INfp>> partsLocal;
      List<NestItem<ISheet>> sheetsLocal;
      SetPolygonIds(parts);
      SetSheetIds(sheets);
      var clonedPolygons = ClonePolygons(parts);
      var clonedSheets = CloneSheets(sheets);

      if (config.OffsetTreePhase)
      {
        ExecuteOffsetTreePhase(config, clonedPolygons, clonedSheets, progressDisplayer).Wait();
      }

      partsLocal = GroupToNestItemList(clonedPolygons);
      sheetsLocal = GroupToNestItemList(clonedSheets);
      SetIncrementingSource(partsLocal, sheetsLocal);

      return (partsLocal.ToArray(), sheetsLocal);
    }

    /// <summary>
    /// Set both Parts and Sheet Source to a single incrementing zero based index.
    /// </summary>
    /// <param name="partsLocal">NestItem list of parts to index.</param>
    /// <param name="sheetsLocal">NestItem list of sheets to index.</param>
    private static void SetIncrementingSource(IEnumerable<NestItem<INfp>> partsLocal, IEnumerable<NestItem<ISheet>> sheetsLocal)
    {
      int srcc = 0;
      foreach (var item in partsLocal)
      {
        item.Polygon.Source = srcc++;
      }

      foreach (var item in sheetsLocal)
      {
        item.Polygon.Source = srcc++;
      }
    }

    private static List<NestItem<ISheet>> GroupToNestItemList(List<ISheet> clonedSheets)
    {
      var p2 = clonedSheets.GroupBy(z => z.Source).Select(z => new NestItem<ISheet>()
      {
        Polygon = z.First(),
        Quantity = z.Count(),
      });

      var sheetsLocal = new List<NestItem<ISheet>>(p2);
      return sheetsLocal;
    }

    private static List<NestItem<INfp>> GroupToNestItemList(IList<INfp> clonedPolygons)
    {
      var p1 = clonedPolygons.GroupBy(z => z.Source).Select(z => new NestItem<INfp>()
      {
        Polygon = z.First(),
        Quantity = z.Count(),
      });
      var partsLocal = new List<NestItem<INfp>>(p1);
      return partsLocal;
    }

    private static async Task ExecuteOffsetTreePhase(ISvgNestConfig config, IList<INfp> clonedPolygons, List<ISheet> clonedSheets, IProgressDisplayer progressDisplayer)
    {
      var grps = clonedPolygons.GroupBy(z => z.Source).ToArray();
      progressDisplayer.InitialiseLoopProgress(ProgressBar.Primary, "Pre-processing (Offset Tree Phase). . .", grps.Length);
      if (config.UseParallel)
      {
        Parallel.ForEach(grps, async (item) =>
        {
          OffsetTreeReplace(config, item);
          await progressDisplayer.IncrementLoopProgress(ProgressBar.Primary).ConfigureAwait(false);
        });
      }
      else
      {
        var idx = 0;
        foreach (var item in grps)
        {
          OffsetTreeReplace(config, item);
          await progressDisplayer.IncrementLoopProgress(ProgressBar.Primary).ConfigureAwait(false);
          progressDisplayer.DisplayTransientMessage($"Pre-processing (Offset Tree Phase-{idx}). . .");
          idx++;
        }
      }

      foreach (var item in clonedSheets)
      {
        var gap = config.SheetSpacing - (config.Spacing / 2);
        INfp sheet = item;
        SvgNest.OffsetTree(ref sheet, -gap, config, true);
      }
    }

    /// <summary>
    /// Clones the Sheets for use in the nest; the original Sheets won't get used again in the nest.
    /// </summary>
    /// <returns>A cloned set of sheets so the original won't get modified.</returns>
    private static List<ISheet> CloneSheets(IList<ISheet> sheets)
    {
      var lsheets = new List<ISheet>();
      foreach (var item in sheets)
      {
        var clone = new Sheet();
        clone.Id = item.Id;
        clone.Source = item.Source;
        clone.ReplacePoints(item.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
        if (item.Children != null)
        {
          foreach (var citem in item.Children)
          {
            clone.Children.Add(new NoFitPolygon());
            var l = clone.Children.Last();
            l.Id = citem.Id;
            l.Source = citem.Source;
            l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
          }
        }

        lsheets.Add(clone);
      }

      return lsheets;
    }

    /// <summary>
    /// Clones the Polygons for use in the nest; the original Polygons won't get used again in the nest.
    /// </summary>
    /// <returns>A cloned set of polygons to nest so the original won't get modified.</returns>
    private static IList<INfp> ClonePolygons(ICollection<INfp> parts)
    {
      var result = new List<INfp>();
      foreach (var item in parts)
      {
        var clone = item.CloneExact();
        result.Add(clone);
      }

      return result;
    }

    /// <summary>
    /// Set each sheet Id to an incrementing zero based index.
    /// </summary>
    private static void SetSheetIds(IList<ISheet> sheets)
    {
      for (int i = 0; i < sheets.Count; i++)
      {
        sheets[i].Id = i;
      }
    }

    /// <summary>
    /// Set each polygon Id to an incrementing zero based index.
    /// </summary>
    private static void SetPolygonIds(ICollection<INfp> parts)
    {
      int id = 0;
      foreach (var item in parts)
      {
        item.Id = id;
        id++;
      }
    }

    /// <summary>
    /// This is the only point where simplification feeds in to the process so use this in tests to apply config-simplifications to imports.
    /// Don't see the reason to apply to all in the group because later we regroup and just use the first again.
    /// </summary>
    /// <param name="config">Config to use when simplifying.</param>
    /// <param name="item">The item that will be modified.</param>
    private static void OffsetTreeReplace(ISvgNestConfig config, IGrouping<int, INfp> item)
    {
      var target = item.First();
      SvgNest.OffsetTree(ref target, 0.5 * config.Spacing, config);
      foreach (var zitem in item)
      {
        zitem.ReplacePoints(item.First().Points);
      }
    }
  }
}
