namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfExporter : ExporterBase, IExport
  {
    public override string SaveFileDialogFilter => "Dxf files (*.dxf)|*.dxf";

    protected async override Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets, bool doMergeLines)
    {
      Dictionary<DxfFile, int> dxfexports = new Dictionary<DxfFile, int>();
      var sheetList = sheets.ToList();
      for (int i = 0; i < sheets.Count(); i++)
      {
        var sheet = sheetList[i];
        DxfFile sheetdxf = GenerateDxfFile(polygons, sheet, i, doMergeLines);
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

    private static IEnumerable<DxfEntity> GetOffsetDxfEntities(IEnumerable<INfp> polygons, ISheet sheet, int i)
    {
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
          yield return ent;
        }
      }
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

    private static DxfPoint RotateLocation(double rotationAngle, DxfPoint pt)
    {
      var angle = (double)(rotationAngle * Math.PI / 180.0f);
      var x = pt.X;
      var y = pt.Y;
      var x1 = (double)((x * Math.Cos(angle)) - (y * Math.Sin(angle)));
      var y1 = (double)((x * Math.Sin(angle)) + (y * Math.Cos(angle)));
      return new DxfPoint(x1, y1, pt.Z);
    }

    private DxfFile GenerateDxfFile(IEnumerable<INfp> polygons, ISheet sheet, int i = 0, bool doMergeLines = false)
    {
      DxfFile sheetdxf = GenerateDxfFileWithSheetOutline(sheet);
      var entities = GetOffsetDxfEntities(polygons.Where(o => o.Sheet.Id == sheet.Id), sheet, i);
      if (doMergeLines)
      {
        entities = new DxfLineMerger().MergeLines(entities);
      }

      foreach (var entity in entities)
      {
        sheetdxf.Entities.Add(entity);
      }

      return sheetdxf;
    }
  }
}