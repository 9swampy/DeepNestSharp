namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfParser
  {
    private const int NumberOfRetries = 5;
    private const int DelayOnRetry = 1000;
    private const double RemoveThreshold = 10e-5;
    private const double ClosingThreshold = 10e-2;

    private static volatile object loadLock = new object();

    public static async Task<IRawDetail> LoadDxfFile(string path)
    {
      FileInfo fi = new FileInfo(path);
      DxfFile dxffile;
      for (int i = 1; i <= NumberOfRetries; ++i)
      {
        try
        {
          lock (loadLock)
          {
            dxffile = DxfFile.Load(fi.FullName);
            IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
            return ConvertDxfToRawDetail(fi.FullName, entities);
          }
        }
        catch (IOException) when (i <= NumberOfRetries)
        {
          await Task.Delay(DelayOnRetry);
        }
        catch (IOException)
        {
          throw;
        }
      }

      return default;
    }

    public static RawDetail<DxfEntity> ConvertDxfToRawDetail(string fullFilename, IEnumerable<DxfEntity> entities)
    {
      RawDetail<DxfEntity> s = new RawDetail<DxfEntity>();
      s.Name = fullFilename;
      Dictionary<DxfEntity, IList<LineElement>> approximations = ApproximateEntities(entities);
      s.AddRangeContour(ConnectElements(approximations));
      if (s.Outers.Any(z => z.Points.Count < 3))
      {
        throw new Exception("Too few points");
      }

      return s;
    }

    private static Dictionary<DxfEntity, IList<LineElement>> ApproximateEntities(IEnumerable<DxfEntity> entities)
    {
      var approximations = new Dictionary<DxfEntity, IList<LineElement>>();

      foreach (DxfEntity ent in entities)
      {
        var elems = new List<LineElement>();
        switch (ent.EntityType)
        {
          case DxfEntityType.LwPolyline:
            {
              DxfLwPolyline poly = (DxfLwPolyline)ent;
              if (poly.Vertices.Count() < 2)
              {
                continue;
              }

              var localContour = new List<PointF>();
              foreach (DxfLwPolylineVertex vert in poly.Vertices)
              {
                localContour.Add(new PointF((float)vert.X, (float)vert.Y));
              }

              elems.AddRange(ConnectTheDots(localContour).ToList());
            }

            break;
          case DxfEntityType.Arc:
            {
              DxfArc arc = (DxfArc)ent;
              List<PointF> pp = new List<PointF>();

              if (arc.StartAngle > arc.EndAngle)
              {
                arc.StartAngle -= 360;
              }

              for (var i = arc.StartAngle; i < arc.EndAngle; i += 15)
              {
                var tt = arc.GetPointFromAngle(i);
                pp.Add(new PointF((float)tt.X, (float)tt.Y));
              }

              var t = arc.GetPointFromAngle(arc.EndAngle);
              pp.Add(new PointF((float)t.X, (float)t.Y));
              for (int j = 1; j < pp.Count; j++)
              {
                var p1 = pp[j - 1];
                var p2 = pp[j];
                elems.Add(new LineElement() { Start = new PointF((float)p1.X, (float)p1.Y), End = new PointF((float)p2.X, (float)p2.Y) });
              }
            }

            break;
          case DxfEntityType.Circle:
            {
              DxfCircle cr = (DxfCircle)ent;
              var cc = new List<PointF>();

              for (int i = 0; i <= 360; i += 15)
              {
                var ang = i * Math.PI / 180f;
                var xx = cr.Center.X + (cr.Radius * Math.Cos(ang));
                var yy = cr.Center.Y + (cr.Radius * Math.Sin(ang));
                cc.Add(new PointF((float)xx, (float)yy));
              }

              elems.AddRange(ConnectTheDots(cc));
            }

            break;
          case DxfEntityType.Line:
            {
              DxfLine poly = (DxfLine)ent;
              elems.Add(new LineElement() { Start = new PointF((float)poly.P1.X, (float)poly.P1.Y), End = new PointF((float)poly.P2.X, (float)poly.P2.Y) });
              break;
            }

          case DxfEntityType.Polyline:
            {
              DxfPolyline poly = (DxfPolyline)ent;
              if (poly.Vertices.Count() < 2)
              {
                continue;
              }

              var localContour = new List<PointF>();
              foreach (DxfVertex vert in poly.Vertices)
              {
                localContour.Add(new PointF((float)vert.Location.X, (float)vert.Location.Y));
              }

              elems.AddRange(ConnectTheDots(localContour));

              break;
            }

          default:
            throw new ArgumentException("unsupported entity type: " + ent);
        }

        elems = elems.Where(z => z.Start.DistTo(z.End) > RemoveThreshold).ToList();
        approximations.Add(ent, elems);
      }

      return approximations;
    }

    internal static RawDetail<DxfEntity> LoadDxfFileStreamAsRawDetail(string path)
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadDxfStream(path, inputStream);
      }
    }

    internal static INfp LoadDxfFileStreamAsNfp(string path)
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadDxfStream(path, inputStream).ToNfp();
      }
    }

    internal static DxfFile LoadDxfFileStream(string path)
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return DxfFile.Load(inputStream);
      }
    }

    internal static RawDetail<DxfEntity> LoadDxfStream(string name, Stream inputStream)
    {
      DxfFile dxffile = DxfFile.Load(inputStream);
      IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
      return ConvertDxfToRawDetail(name, entities);
    }

    /// <summary>
    /// Returns a series of LineElements to connect the points passed in.
    /// </summary>
    /// <param name="points">List of <see cref="PointF"/> to join.</param>
    /// <returns>List of <see cref="LineElement"/> connecting the dots.</returns>
    private static IEnumerable<LineElement> ConnectTheDots(IList<PointF> points)
    {
      for (int i = 0; i < points.Count; i++)
      {
        var p0 = points[i];
        var p1 = points[(i + 1) % points.Count];
        yield return new LineElement() { Start = p0, End = p1 };
      }
    }

    private static LocalContour<DxfEntity>[] ConnectElements(Dictionary<DxfEntity, IList<LineElement>> approximations)
    {
      List<(DxfEntity Entity, LineElement LineElement)> allLineElements = GetAllLineElements(approximations);

      PointF prior = default;
      List<PointF> newContourPoints = new List<PointF>();
      var newContourEntities = new HashSet<DxfEntity>();
      var result = new List<LocalContour<DxfEntity>>();
      while (allLineElements.Any())
      {
        if (newContourPoints.Count == 0)
        {
          var toStart = allLineElements.First().LineElement;
          newContourPoints.Add(toStart.Start);
          prior = toStart.End;
          newContourPoints.Add(prior);
          newContourEntities.Add(allLineElements.First().Entity);
          allLineElements.RemoveAt(0);
        }
        else
        {
          if (!TryGetAnotherPoint(prior, allLineElements, out (DxfEntity Entity, LineElement LineElement) next))
          {
            result.Add(new LocalContour<DxfEntity>(newContourPoints.ToList(), newContourEntities));
            newContourPoints = new List<PointF>();
            newContourEntities = new HashSet<DxfEntity>();
          }
          else
          {
            allLineElements.Remove(next);
            newContourEntities.Add(next.Entity);
            prior = EndIsClosest(prior, next) ? next.LineElement.End : next.LineElement.Start;
            newContourPoints.Add(prior);
          }
        }
      }

      if (newContourPoints.Any())
      {
        result.Add(new LocalContour<DxfEntity>(newContourPoints.ToList(), newContourEntities));
      }

      result.OrderByDescending(o => Math.Abs(Geometry.GeometryUtil.PolygonArea(o.Points))).First().IsChild = false;
      return result.ToArray();
    }

    private static List<(DxfEntity Entity, LineElement LineElement)> GetAllLineElements(Dictionary<DxfEntity, IList<LineElement>> approximations)
    {
      var allLineElements = new List<(DxfEntity Entity, LineElement LineElement)>();
      foreach (KeyValuePair<DxfEntity, IList<LineElement>> kvp in approximations)
      {
        allLineElements.AddRange(kvp.Value.Select(o => (kvp.Key, o)));
      }

      return allLineElements;
    }

    private static bool EndIsClosest(PointF prior, (DxfEntity Entity, LineElement LineElement) next)
    {
      return next.LineElement.Start.DistTo(prior) < next.LineElement.End.DistTo(prior);
    }

    private static bool TryGetAnotherPoint(PointF prior, List<(DxfEntity Entity, LineElement LineElement)> allLineElements, out (DxfEntity Entity, LineElement LineElement) next)
    {
      var match = allLineElements.Select(candidate => (candidate, MinDistance(prior, candidate)))
                       .Where(o => o.Item2 <= ClosingThreshold)
                       .OrderBy(o => o.Item2).FirstOrDefault();
      if (match != default)
      {
        next = match.candidate;
        return true;
      }

      next = default;
      return false;
    }

    private static double MinDistance(PointF prior, (DxfEntity Entity, LineElement LineElement) candidate)
    {
      return Math.Min(candidate.LineElement.Start.DistTo(prior), candidate.LineElement.End.DistTo(prior));
    }
  }
}