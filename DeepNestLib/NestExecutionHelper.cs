namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using DeepNestLib.NestProject;
  
  public class NestExecutionHelper
  {
    public void InitialiseNest(NestingContext context, IList<ISheetLoadInfo> sheetLoadInfos, IList<IDetailLoadInfo> detailLoadInfos, IProgressDisplayer progressDisplayer)
    {
      context.Reset();
      int src = 0;
      foreach (var item in sheetLoadInfos)
      {
        src = context.GetNextSheetSource();
        for (int i = 0; i < item.Quantity; i++)
        {
          var ns = Sheet.NewSheet(context.Sheets.Count + 1, item.Width, item.Height);
          context.Sheets.Add(ns);
          ns.Source = src;
        }
      }

      context.ReorderSheets();
      src = 0;
      foreach (var item in detailLoadInfos.Where(o => o.IsIncluded))
      {
        progressDisplayer.DisplayTransientMessage($"Preload {item.Path}. . .");
        var det = LoadRawDetail(new FileInfo(item.Path));

        AddToPolygons(context, src, det, item.Quantity, progressDisplayer, isPriority: item.IsPriority, isMultiplied: item.IsMultiplied, strictAngles: item.StrictAngle);

        src++;
      }

      progressDisplayer.DisplayTransientMessage(string.Empty);
    }

    public void AddToPolygons(NestingContext context, int src, RawDetail det, int quantity, IProgressDisplayer progressDisplayer, bool isIncluded = true, bool isPriority = false, bool isMultiplied = false, AnglesEnum strictAngles = AnglesEnum.Vertical)
    {
      var item = new DetailLoadInfo() { Quantity = quantity, IsIncluded = isIncluded, IsPriority = isPriority, IsMultiplied = isMultiplied, StrictAngle = strictAngles };
      AddToPolygons(context, src, det, item, progressDisplayer);
    }

    public void AddToPolygons(NestingContext context, int src, RawDetail det, DetailLoadInfo item, IProgressDisplayer progressDisplayer)
    {
      INfp loadedNfp;
      if (det.TryConvertToNfp(src, out loadedNfp))
      {
        loadedNfp.IsPriority = item.IsPriority;
        loadedNfp.StrictAngle = item.StrictAngle;
        var quantity = item.Quantity * (item.IsMultiplied ? SvgNest.Config.Multiplier : 1);
        for (int i = 0; i < quantity; i++)
        {
          context.Polygons.Add(loadedNfp.Clone());
        }
      }
      else
      {
        progressDisplayer.DisplayMessageBox($"Failed to import {det.Name}.", "Load Error", MessageBoxIcon.Stop);
      }
    }

    public RawDetail LoadRawDetail(FileInfo f)
    {
      RawDetail det = null;
      if (f.Extension == ".svg")
      {
        det = SvgParser.LoadSvg(f.FullName);
      }

      if (f.Extension == ".dxf")
      {
        det = DxfParser.LoadDxfFile(f.FullName).Result;
      }

      return det;
    }
  }
}
