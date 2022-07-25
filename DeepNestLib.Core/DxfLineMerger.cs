namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfLineMerger
  {
    private const decimal Tolerance = 0.0001M;

    internal static MergeLine GetCombined(MergeLine a, MergeLine b)
    {
      if (!Coincident(a, b) || !Coaligned(a, b))
      {
        throw new InvalidOperationException("Mergelines are not coincident.");
      }

      if (b.IsVertical)
      {
        var minY = Math.Min(Math.Min(a.Left.Y, a.Right.Y), Math.Min(b.Left.Y, b.Right.Y));
        var maxY = Math.Max(Math.Max(a.Left.Y, a.Right.Y), Math.Max(b.Left.Y, b.Right.Y));
        var min = new DxfPoint(a.Left.X, minY, 0);
        var max = new DxfPoint(a.Left.X, maxY, 0);

        return new MergeLine(new DxfLine(min, max));
      }
      else
      {
        double newY = ((double)b.Slope * Math.Max(b.Right.X, a.Right.X)) + (double)b.Intercept;
        var right = new DxfPoint(Math.Max(b.Right.X, a.Right.X), newY, 0);
        return new MergeLine(new DxfLine(a.Left, right));
      }
    }

    internal static bool Coincident(MergeLine prior, MergeLine current)
    {
      if (Coaligned(prior, current))
      {
        if (prior.IsVertical)
        {
          return PairCoincident(prior, current) ||
                 PairCoincident(current, prior);
        }
        else
        {
          return current.Left.X >= prior.Left.X &&
                 current.Left.X <= prior.Right.X;
        }
      }

      return false;
    }

    private static bool PairCoincident(MergeLine prior, MergeLine current)
    {
      var result = (current.Left.Y >= prior.Left.Y &&
                    current.Left.Y <= prior.Right.Y) ||
                   (current.Right.Y >= prior.Left.Y &&
                    current.Right.Y <= prior.Right.Y);
      return result;
    }

    internal static bool Coaligned(MergeLine prior, MergeLine current)
    {
      return ((current.IsVertical && prior.IsVertical) || (current.Slope - prior.Slope < Tolerance)) &&
             current.Intercept - prior.Intercept < Tolerance;
    }

    internal DxfFile MergeLines(DxfFile dxfFile)
    {
      var result = new DxfFile();
      foreach (var entity in MergeLines(dxfFile.Entities))
      {
        result.Entities.Add(entity);
      }

      return result;
    }

    internal IEnumerable<DxfEntity> MergeLines(IEnumerable<DxfEntity> entities)
    {
      int entityCount;
      do
      {
        entityCount = entities.Count();
        entities = DoMergeLines(entities);
      }
      while (entityCount != entities.Count());
      return entities;
    }

    private IEnumerable<DxfEntity> DoMergeLines(IEnumerable<DxfEntity> entities)
    {
      var result = new List<DxfEntity>();
      var splitEntities = SplitLines(entities);
      result.AddRange(splitEntities.Where(o => !(o is DxfLine)));
      var mergeLines = MergeLines(splitEntities.Where(o => o is DxfLine).Cast<DxfLine>()).ToList();
      result.AddRange(mergeLines);
      return result;
    }

    internal IEnumerable<DxfEntity> SplitLines(IEnumerable<DxfEntity> entities)
    {
      foreach (var entity in entities)
      {
        if (entity is DxfPolyline polyline)
        {
          foreach (var line in Split(polyline))
          {
            yield return line;
          }
        }
        else if (entity is DxfLine line)
        {
          yield return line;
        }
        else
        {
          yield return entity;
        }
      }
    }

    internal IEnumerable<DxfLine> MergeLines(IEnumerable<DxfLine> list)
    {
      if (list.Count() <= 1)
      {
        return list;
      }

      var result = new List<MergeLine>();
      foreach (var line in list)
      {
        result.Add(new MergeLine(line));
      }

      var lines = result
        .OrderBy(o => o.Slope)
        .ThenBy(o => o.Intercept)
        .ThenBy(o => o.IsVertical ? o.Left.Y : o.Left.X)
        .ThenBy(o => o.Left.Y).ToList();
      MergeLine prior = null;
      MergeLine current = null;
      for (int i = 0; i < lines.Count; i++)
      {
        if (prior == null)
        {
          prior = lines[i];
        }
        else
        {
          current = lines[i];
          if (Coincident(prior, current))
          {
            lines.Remove(current);
            i--;
            var combined = GetCombined(prior, current);
            lines[i] = combined;
            prior = combined;
          }
          else
          {
            prior = current;
          }
        }
      }

      return lines.Select(o => o.Line);
    }

    private static IEnumerable<DxfLine> Split(DxfPolyline polyline)
    {
      bool isFirst = true;
      DxfVertex origin = null;
      DxfVertex prior = null;
      foreach (var point in polyline.Vertices)
      {
        if (isFirst)
        {
          isFirst = false;
          origin = point;
        }
        else
        {
          yield return new DxfLine(prior.Location, point.Location);
        }

        prior = point;
      }

      if (polyline.IsClosed)
      {
        yield return new DxfLine(prior.Location, origin.Location);
      }
    }
  }
}