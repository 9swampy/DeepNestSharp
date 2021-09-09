﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using DeepNestLib.Placement;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfParser : ParserBase, IExport
  {
    private const int NumberOfRetries = 5;
    private const int DelayOnRetry = 1000;

    private static volatile object loadLock = new object();

    public static RawDetail LoadDxfStream(string path)
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadDxfStream(path, inputStream);
      }
    }

    public static RawDetail LoadDxfStream(string name, Stream inputStream)
    {
      DxfFile dxffile = DxfFile.Load(inputStream);
      IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
      return ConvertDxfToRawDetail(name, entities);
    }

    public async static Task<RawDetail> LoadDxfFile(string path)
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

      return default(RawDetail);
    }

    public static RawDetail ConvertDxfToRawDetail(string fullFilename, IEnumerable<DxfEntity> entities)
    {
      RawDetail s = new RawDetail();
      s.Name = fullFilename;

      var points = new LocalContour();
      var elems = new List<LineElement>();

      foreach (DxfEntity ent in entities)
      {
        switch (ent.EntityType)
        {
          case DxfEntityType.LwPolyline:
            {
              DxfLwPolyline poly = (DxfLwPolyline)ent;
              if (poly.Vertices.Count() < 2)
              {
                continue;
              }

              foreach (DxfLwPolylineVertex vert in poly.Vertices)
              {
                points.Points.Add(new PointF((float)vert.X, (float)vert.Y));
              }

              elems.AddRange(ConnectTheDots(points.Points));
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
              LocalContour cc = new LocalContour();

              for (int i = 0; i <= 360; i += 15)
              {
                var ang = i * Math.PI / 180f;
                var xx = cr.Center.X + (cr.Radius * Math.Cos(ang));
                var yy = cr.Center.Y + (cr.Radius * Math.Sin(ang));
                cc.Points.Add(new PointF((float)xx, (float)yy));
              }

              elems.AddRange(ConnectTheDots(cc.Points));
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

              foreach (DxfVertex vert in poly.Vertices)
              {
                points.Points.Add(new PointF((float)vert.Location.X, (float)vert.Location.Y));
              }

              elems.AddRange(ConnectTheDots(points.Points));

              break;
            }

          default:
            throw new ArgumentException("unsupported entity type: " + ent);
        }
      }

      elems = elems.Where(z => z.Start.DistTo(z.End) > RemoveThreshold).ToList();
      var cntrs2 = ConnectElements(elems.ToArray());
      s.AddRangeContour(cntrs2);
      if (s.Outers.Any(z => z.Points.Count < 3))
      {
        throw new Exception("few points");
      }

      return s;
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

    public static double RemoveThreshold = 10e-5;
    public static double ClosingThreshold = 10e-2;

    public override string SaveFileDialogFilter => "Dxf files (*.dxf)|*.dxf";

    public static LocalContour[] ConnectElements(LineElement[] elems)
    {
      List<LocalContour> ret = new List<LocalContour>();

      List<PointF> pp = new List<PointF>();
      List<LineElement> last = new List<LineElement>();
      last.AddRange(elems);

      while (last.Any())
      {
        if (pp.Count == 0)
        {
          pp.Add(last.First().Start);
          pp.Add(last.First().End);
          last.RemoveAt(0);
        }
        else
        {
          var ll = pp.Last();
          var f1 = last.OrderBy(z => Math.Min(z.Start.DistTo(ll), z.End.DistTo(ll))).First();

          var dist = Math.Min(f1.Start.DistTo(ll), f1.End.DistTo(ll));
          if (dist > ClosingThreshold)
          {
            ret.Add(new LocalContour() { Points = pp.ToList() });
            pp.Clear();
            continue;
          }

          last.Remove(f1);
          if (f1.Start.DistTo(ll) < f1.End.DistTo(ll))
          {
            pp.Add(f1.End);
          }
          else
          {
            pp.Add(f1.Start);
          }
        }
      }

      if (pp.Any())
      {
        ret.Add(new LocalContour() { Points = pp.ToList() });
      }

      return ret.ToArray();
    }

    public async override Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets)
    {
      Dictionary<DxfFile, int> dxfexports = new Dictionary<DxfFile, int>();
      var sheetList = sheets.ToList();
      for (int i = 0; i < sheets.Count(); i++)
      {
        var sheet = sheetList[i];
        DxfFile sheetdxf = GenerateDxfFile(polygons, sheet, i);
        dxfexports.Add(sheetdxf, sheet.Id);
      }

      int sheetcount = 0;
      for (int i = 0; i < dxfexports.Count(); i++)
      {
        var dxf = dxfexports.ElementAt(i).Key;
        var id = dxfexports.ElementAt(i).Value;

        if (dxf.Entities.Count != 1)
        {
          sheetcount += 1;
          FileInfo fi = new FileInfo(path);
          await Task.Run(() =>
          {
            if (dxfexports.Count() == 1)
            {
              dxf.Save(fi.FullName, true);
            }
            else
            {
              dxf.Save($"{fi.FullName.Substring(0, fi.FullName.Length - 4)}{id}.dxf", true);
            }
          }).ConfigureAwait(false);
        }
      }
    }

    public DxfFile GenerateDxfFile(ISheetPlacement sheetPlacement)
    {
      return GenerateDxfFile(sheetPlacement.PartPlacements.Select(o => o.Part), sheetPlacement.Sheet, 0);
    }

    private DxfFile GenerateDxfFile(IEnumerable<INfp> polygons, ISheet sheet, int i = 0)
    {
      DxfFile sheetdxf = GenerateDxfFileWithSheetOutline(sheet);
      foreach (var polygon in polygons)
      {
        DxfFile fl;
        if (polygon.Fitted == false || !polygon.Name.ToLower().Contains(".dxf") || polygon.Sheet.Id != sheet.Id)
        {
          continue;
        }
        else
        {
          fl = DxfFile.Load(polygon.Name);
        }

        double sheetXoffset = -sheet.WidthCalculated * i;
        //double sheetyoffset = -sheetheight * i;
        DxfPoint offsetdistance = new DxfPoint(polygon.X + sheetXoffset, polygon.Y, 0D);
        List<DxfEntity> newlist = OffsetToNest(fl.Entities, offsetdistance, polygon.Rotation);

        foreach (DxfEntity ent in newlist)
        {
          sheetdxf.Entities.Add(ent);
        }
      }

      return sheetdxf;
    }

    private static DxfFile GenerateDxfFileWithSheetOutline(ISheet sheet)
    {
      // Generate Sheet Outline in Dxf
      var sheetdxf = new DxfFile();
      sheetdxf.Views.Clear();

      List<DxfVertex> sheetverts = new List<DxfVertex>();

      // Bottom Left Point
      sheetverts.Add(new DxfVertex(new DxfPoint(0, 0, 0)));

      // Bottom Right Point
      sheetverts.Add(new DxfVertex(new DxfPoint(sheet.WidthCalculated, 0, 0)));

      // Top Right Point
      sheetverts.Add(new DxfVertex(new DxfPoint(sheet.WidthCalculated, sheet.HeightCalculated, 0)));

      // Top Left Point
      sheetverts.Add(new DxfVertex(new DxfPoint(0, sheet.HeightCalculated, 0)));

      DxfPolyline sheetentity = new DxfPolyline(sheetverts)
      {
        IsClosed = true,
        Layer = $"Plate H{sheet.HeightCalculated} W{sheet.WidthCalculated}",
      };

      sheetdxf.Entities.Add(sheetentity);

      return sheetdxf;
    }

    private static List<DxfEntity> OffsetToNest(IList<DxfEntity> dxfEntities, DxfPoint offset, double rotationAngle)
    {
      List<DxfEntity> dxfreturn = new List<DxfEntity>();
      List<DxfPoint> tmpPts;
      foreach (DxfEntity entity in dxfEntities)
      {
        switch (entity.EntityType)
        {
          case DxfEntityType.Arc:
            DxfArc dxfArc = (DxfArc)entity;
            dxfArc.Center = RotateLocation(rotationAngle, dxfArc.Center);
            dxfArc.Center += offset;
            dxfArc.StartAngle += rotationAngle;
            dxfArc.EndAngle += rotationAngle;
            dxfreturn.Add(dxfArc);
            break;

          case DxfEntityType.ArcAlignedText:
            DxfArcAlignedText dxfArcAligned = (DxfArcAlignedText)entity;
            dxfArcAligned.CenterPoint = RotateLocation(rotationAngle, dxfArcAligned.CenterPoint);
            dxfArcAligned.CenterPoint += offset;
            dxfArcAligned.StartAngle += rotationAngle;
            dxfArcAligned.EndAngle += rotationAngle;
            dxfreturn.Add(dxfArcAligned);
            break;

          case DxfEntityType.Attribute:
            DxfAttribute dxfAttribute = (DxfAttribute)entity;
            dxfAttribute.Location = RotateLocation(rotationAngle, dxfAttribute.Location);
            dxfAttribute.Location += offset;
            dxfreturn.Add(dxfAttribute);
            break;

          case DxfEntityType.AttributeDefinition:
            DxfAttributeDefinition dxfAttributecommon = (DxfAttributeDefinition)entity;
            dxfAttributecommon.Location = RotateLocation(rotationAngle, dxfAttributecommon.Location);
            dxfAttributecommon.Location += offset;
            dxfreturn.Add(dxfAttributecommon);
            break;

          case DxfEntityType.Circle:
            DxfCircle dxfCircle = (DxfCircle)entity;
            dxfCircle.Center = RotateLocation(rotationAngle, dxfCircle.Center);
            dxfCircle.Center += offset;
            dxfreturn.Add(dxfCircle);
            break;

          case DxfEntityType.Ellipse:
            DxfEllipse dxfEllipse = (DxfEllipse)entity;
            dxfEllipse.Center = RotateLocation(rotationAngle, dxfEllipse.Center);
            dxfEllipse.Center += offset;
            dxfreturn.Add(dxfEllipse);
            break;

          case DxfEntityType.Image:
            DxfImage dxfImage = (DxfImage)entity;
            dxfImage.Location = RotateLocation(rotationAngle, dxfImage.Location);
            dxfImage.Location += offset;

            dxfreturn.Add(dxfImage);
            break;

          case DxfEntityType.Leader:
            DxfLeader dxfLeader = (DxfLeader)entity;
            tmpPts = new List<DxfPoint>();

            foreach (DxfPoint vrt in dxfLeader.Vertices)
            {
              var tmppnt = RotateLocation(rotationAngle, vrt);
              tmppnt += offset;
              tmpPts.Add(tmppnt);
            }

            dxfLeader.Vertices.Clear();
            dxfLeader.Vertices.Concat(tmpPts);
            dxfreturn.Add(dxfLeader);
            break;

          case DxfEntityType.Line:
            DxfLine dxfLine = (DxfLine)entity;
            dxfLine.P1 = RotateLocation(rotationAngle, dxfLine.P1);
            dxfLine.P2 = RotateLocation(rotationAngle, dxfLine.P2);
            dxfLine.P1 += offset;
            dxfLine.P2 += offset;
            dxfreturn.Add(dxfLine);
            break;

          case DxfEntityType.LwPolyline:
            DxfPolyline dxfPoly = (DxfPolyline)entity;
            foreach (DxfVertex vrt in dxfPoly.Vertices)
            {
              vrt.Location = RotateLocation(rotationAngle, vrt.Location);
              vrt.Location += offset;
            }

            dxfreturn.Add(dxfPoly);
            break;

          case DxfEntityType.MLine:
            DxfMLine mLine = (DxfMLine)entity;
            tmpPts = new List<DxfPoint>();
            mLine.StartPoint += offset;

            mLine.StartPoint = RotateLocation(rotationAngle, mLine.StartPoint);

            foreach (DxfPoint vrt in mLine.Vertices)
            {
              var tmppnt = RotateLocation(rotationAngle, vrt);
              tmppnt += offset;
              tmpPts.Add(tmppnt);
            }

            mLine.Vertices.Clear();
            mLine.Vertices.Concat(tmpPts);
            dxfreturn.Add(mLine);
            break;

          case DxfEntityType.Polyline:
            DxfPolyline polyline = (DxfPolyline)entity;

            List<DxfVertex> verts = new List<DxfVertex>();
            foreach (DxfVertex vrt in polyline.Vertices)
            {
              var tmppnt = vrt;
              tmppnt.Location = RotateLocation(rotationAngle, tmppnt.Location);
              tmppnt.Location += offset;
              verts.Add(tmppnt);
            }

            DxfPolyline polyout = new DxfPolyline(verts);
            polyout.Location = polyline.Location + offset;
            polyout.IsClosed = polyline.IsClosed;
            polyout.Layer = polyline.Layer;
            dxfreturn.Add(polyout);

            break;

          case DxfEntityType.Body:
          case DxfEntityType.DgnUnderlay:
          case DxfEntityType.Dimension:
          case DxfEntityType.DwfUnderlay:
          case DxfEntityType.Face:
          case DxfEntityType.Helix:
          case DxfEntityType.Insert:
          case DxfEntityType.Light:
          case DxfEntityType.ModelerGeometry:
          case DxfEntityType.MText:
          case DxfEntityType.OleFrame:
          case DxfEntityType.Ole2Frame:
          case DxfEntityType.PdfUnderlay:
          case DxfEntityType.Point:
          case DxfEntityType.ProxyEntity:
          case DxfEntityType.Ray:
          case DxfEntityType.Region:
          case DxfEntityType.RText:
          case DxfEntityType.Section:
          case DxfEntityType.Seqend:
          case DxfEntityType.Shape:
          case DxfEntityType.Solid:
          case DxfEntityType.Spline:
          case DxfEntityType.Text:
          case DxfEntityType.Tolerance:
          case DxfEntityType.Trace:
          case DxfEntityType.Underlay:
          case DxfEntityType.Vertex:
          case DxfEntityType.WipeOut:
          case DxfEntityType.XLine:
            throw new ArgumentException("unsupported entity type: " + entity.EntityType);
        }
      }

      return dxfreturn;
    }

    public static DxfPoint RotateLocation(double rotationAngle, DxfPoint pt)
    {
      var angle = (double)(rotationAngle * Math.PI / 180.0f);
      var x = pt.X;
      var y = pt.Y;
      var x1 = (double)((x * Math.Cos(angle)) - (y * Math.Sin(angle)));
      var y1 = (double)((x * Math.Sin(angle)) + (y * Math.Cos(angle)));
      return new DxfPoint(x1, y1, pt.Z);
    }
  }
}