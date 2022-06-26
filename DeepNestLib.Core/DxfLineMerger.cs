namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfLineMerger
  {
    private const double Tolerance = 0.0001;

    internal static MergeLine GetCombined(MergeLine a, MergeLine b)
    {
      double newY;
      if (b.IsVertical)
      {
        newY = Math.Max(a.Right.Y, b.Right.Y);
      }
      else
      {
        newY = (b.Slope * Math.Max(b.Right.X, a.Right.X)) + b.Intercept;
      }

      var right = new DxfPoint(Math.Max(b.Right.X, a.Right.X), newY, 0);
      return new MergeLine(new DxfLine(a.Left, right));
    }

    internal static bool Coincident(MergeLine prior, MergeLine current)
    {
      if (prior.IsVertical)
      {
        return current.Left.Y >= prior.Left.Y &&
               current.Left.Y <= prior.Right.Y;
      }
      else
      {
        return current.Left.X >= prior.Left.X &&
               current.Left.X <= prior.Right.X;
      }
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
      var result = new List<DxfEntity>();
      var lines = new List<DxfLine>();
      foreach (var entity in entities)
      {
        if (entity is DxfPolyline polyline)
        {
          lines.AddRange(Split(polyline));
        }
        else if (entity is DxfLine line)
        {
          lines.Add(line);
        }
        else
        {
          result.Add(entity);
        }
      }

      var mergeLines = MergeLines(lines).ToList();
      result.AddRange(mergeLines);
      return result;
    }

    internal IEnumerable<DxfLine> MergeLines(List<DxfLine> list)
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
        .ThenBy(o => o.Left.X)
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
          if (Coaligned(prior, current) &&
              Coincident(prior, current))
          {
            lines.Remove(current);
            i--;
            var combined = GetCombined(prior, current);
            lines[i] = combined;
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