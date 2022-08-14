namespace DeepNestLib.IO
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfExporter : ExporterBase, IExport
  {
    public override string SaveFileDialogFilter => "Dxf files (*.dxf)|*.dxf";

    public async Task Export(Stream stream, ISheetPlacement sheetPlacement, bool doMergeLines, bool differentiateChildren)
    {
      await Export(stream, sheetPlacement.PolygonsForExport, sheetPlacement.Sheet, doMergeLines, differentiateChildren);
    }

    public async Task Export(Stream stream, IEnumerable<INfp> polygons, ISheet sheet, bool doMergeLines, bool differentiateChildren)
    {
      DxfFile sheetdxf = GenerateDxfFile(polygons, sheet, sheet.Id, doMergeLines, differentiateChildren);
      var dxf = sheetdxf;
      var id = sheet.Id;

      if (dxf.Entities.Count != 1)
      {
        await Task.Run(() =>
        {
          dxf.Save(stream, true);
          //dxf.Save(@"C:\Temp\DeepnestSharp.dxf");
        }).ConfigureAwait(false);
      }
    }

    protected async override Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets, bool doMergeLines, bool differentiateChildren)
    {
      try
      {
        Dictionary<DxfFile, int> dxfexports = new Dictionary<DxfFile, int>();
        var sheetList = sheets.ToList();
        for (var i = 0; i < sheets.Count(); i++)
        {
          var sheet = sheetList[i];
          DxfFile sheetdxf = GenerateDxfFile(polygons, sheet, i, doMergeLines, differentiateChildren);
          dxfexports.Add(sheetdxf, sheet.Id);
        }

        for (var i = 0; i < dxfexports.Count(); i++)
        {
          var dxf = dxfexports.ElementAt(i).Key;
          var id = dxfexports.ElementAt(i).Value;

          if (dxf.Entities.Count != 1)
          {
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
      catch (Exception ex)
      {
        throw;
      }
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

    private static IEnumerable<DxfEntity> GetOffsetDxfEntities(IEnumerable<INfp> polygons, ISheet sheet, int i, bool differentiateChildren)
    {
      foreach (var polygon in polygons)
      {
        RawDetail<DxfEntity> fl;
        if (polygon.Fitted == false || !polygon.Name.ToLower().Contains(".dxf") || polygon.Sheet.Id != sheet.Id)
        {
          continue;
        }
        else
        {
          try
          {
#if NCRUNCH
            if (polygon.IsExact && !(new FileInfo(polygon.Name).Exists))
            {
              fl = DxfParser.ConvertDxfToRawDetail("generated.dxf", polygon.ToDxfFile().Entities.ToArray());
            }
            else
#endif
            {
              fl = DxfParser.ConvertDxfToRawDetail(polygon.Name, DxfFile.Load(polygon.Name).Entities.ToArray());
            }
          }
          catch
          {
            throw new FileNotFoundException($"The file {polygon.Name} could not be loaded. When exporting the original files that generated the nest are used for precision, instead of the copies rotated and manipulated potentially many times during the nest; degrading potentially their accuracy. It would be possible to load and store the original files but that'd take some effort...");
          }
        }

        var sheetXoffset = -sheet.WidthCalculated * i;
        //double sheetYoffset = -sheet.Height * i;
        DxfPoint offsetdistance = new DxfPoint(polygon.X + sheetXoffset, polygon.Y, 0D);
        List<DxfEntity> newlist = OffsetToNest(fl.Outers, offsetdistance, polygon.Rotation, differentiateChildren, polygon.IsDifferentiated);
        foreach (DxfEntity ent in newlist)
        {
          yield return ent;
        }
      }
    }

    private static List<DxfEntity> OffsetToNest(IEnumerable<ILocalContour> contours, DxfPoint offsetdistance, double rotation, bool differentiateChildren, bool isDifferentiated)
    {
      var allEntities = new List<DxfEntity>();
      foreach (var contour in contours)
      {
        if (contour is LocalContour<DxfEntity> castContour)
        {
          if (differentiateChildren && castContour.IsChild)
          {
            foreach (var child in castContour.Entities)
            {
              child.Color = DxfColorHelpers.GetClosestDefaultIndexColor(255, 0, 0);
            }
          }
          else if (isDifferentiated)
          {
            foreach (var ent in castContour.Entities)
            {
              if (!differentiateChildren || !castContour.IsChild)
              {
                ent.Color = DxfColorHelpers.GetClosestDefaultIndexColor(0, 0, 255);
              }
            }
          }

          allEntities.AddRange(castContour.Entities);
        }
      }

      return OffsetToNest(allEntities, offsetdistance, rotation);
    }

    private static List<DxfEntity> OffsetToNest(IList<DxfEntity> dxfEntities, DxfPoint offset, double rotationAngle)
    {
      var result = new List<DxfEntity>();
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
            result.Add(dxfArc);
            break;

          case DxfEntityType.ArcAlignedText:
            DxfArcAlignedText dxfArcAligned = (DxfArcAlignedText)entity;
            dxfArcAligned.CenterPoint = RotateLocation(rotationAngle, dxfArcAligned.CenterPoint);
            dxfArcAligned.CenterPoint += offset;
            dxfArcAligned.StartAngle += rotationAngle;
            dxfArcAligned.EndAngle += rotationAngle;
            result.Add(dxfArcAligned);
            break;

          case DxfEntityType.Attribute:
            DxfAttribute dxfAttribute = (DxfAttribute)entity;
            dxfAttribute.Location = RotateLocation(rotationAngle, dxfAttribute.Location);
            dxfAttribute.Location += offset;
            result.Add(dxfAttribute);
            break;

          case DxfEntityType.AttributeDefinition:
            DxfAttributeDefinition dxfAttributecommon = (DxfAttributeDefinition)entity;
            dxfAttributecommon.Location = RotateLocation(rotationAngle, dxfAttributecommon.Location);
            dxfAttributecommon.Location += offset;
            result.Add(dxfAttributecommon);
            break;

          case DxfEntityType.Circle:
            DxfCircle dxfCircle = (DxfCircle)entity;
            dxfCircle.Center = RotateLocation(rotationAngle, dxfCircle.Center);
            dxfCircle.Center += offset;
            result.Add(dxfCircle);
            break;

          case DxfEntityType.Ellipse:
            DxfEllipse dxfEllipse = (DxfEllipse)entity;
            dxfEllipse.Center = RotateLocation(rotationAngle, dxfEllipse.Center);
            dxfEllipse.Center += offset;
            result.Add(dxfEllipse);
            break;

          case DxfEntityType.Image:
            DxfImage dxfImage = (DxfImage)entity;
            dxfImage.Location = RotateLocation(rotationAngle, dxfImage.Location);
            dxfImage.Location += offset;

            result.Add(dxfImage);
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
            result.Add(dxfLeader);
            break;

          case DxfEntityType.Line:
            DxfLine dxfLine = (DxfLine)entity;
            dxfLine.P1 = RotateLocation(rotationAngle, dxfLine.P1);
            dxfLine.P2 = RotateLocation(rotationAngle, dxfLine.P2);
            dxfLine.P1 += offset;
            dxfLine.P2 += offset;
            result.Add(dxfLine);
            break;

          case DxfEntityType.LwPolyline:
            DxfPolyline dxfPoly = (DxfPolyline)entity;
            foreach (DxfVertex vrt in dxfPoly.Vertices)
            {
              vrt.Location = RotateLocation(rotationAngle, vrt.Location);
              vrt.Location += offset;
            }

            result.Add(dxfPoly);
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
            result.Add(mLine);
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
            result.Add(polyout);

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

      return result;
    }

    private static DxfPoint RotateLocation(double rotationAngle, DxfPoint pt)
    {
      var angle = GeometryUtil.ToRadians(rotationAngle);
      var x = pt.X;
      var y = pt.Y;
      var x1 = (double)(x * Math.Cos(angle) - y * Math.Sin(angle));
      var y1 = (double)(x * Math.Sin(angle) + y * Math.Cos(angle));
      return new DxfPoint(x1, y1, pt.Z);
    }

    private DxfFile GenerateDxfFile(IEnumerable<INfp> polygons, ISheet sheet, int i, bool doMergeLines, bool differentiateChildren)
    {
      try
      {
        DxfFile sheetdxf = GenerateDxfFileWithSheetOutline(sheet);
        var entities = GetOffsetDxfEntities(polygons.Where(o => o.Sheet.Id == sheet.Id), sheet, i, differentiateChildren);

        if (doMergeLines)
        {
          entities = new DxfLineMerger().MergeLines(entities);
          //Have seen oddness where splitting out verticals and non verticals prevented an errant output; but then it just stopped happening!
          //var mergeLines = entities.Where(o => o is DxfLine).Cast<DxfLine>().Select(o => new MergeLine(o));
          //var over = mergeLines.Where(o => o.IsVertical && Math.Max(o.Left.Y, o.Right.Y) > sheet.MaxY);
          //var under = mergeLines.Where(o => o.IsVertical && Math.Min(o.Left.Y, o.Right.Y) < sheet.MinY);
          //if (over.Any() || under.Any())
          //{
          //  System.Diagnostics.Debugger.Break();
          //}

          //entities = mergeLines.Where(o => !o.IsVertical).Select(o => o.Line as DxfEntity).ToList();
          //entities.AddRange(mergeLines.Where(o => o.IsVertical).Select(o => o.Line));
        }

        foreach (var entity in entities)
        {
          sheetdxf.Entities.Add(entity);
        }

        return sheetdxf;
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}