namespace DeepNestLib
{
  using DeepNestLib.Geometry;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif

  public class SheetNfp : NfpCandidateList
  {
    public new const string FileDialogFilter = "DeepNest SheetNfp (*.dnsnfp)|*.dnsnfp|All files (*.*)|*.*";

    public SheetNfp(NfpCandidateList nfpCandidateList)
      : base(nfpCandidateList.Items, nfpCandidateList.Sheet, nfpCandidateList.Part)
    {
    }

    [JsonConstructor]
    public SheetNfp(INfp[] items, ISheet sheet, INfp part)
      : base(items, sheet, part)
    {
    }

    // Inner NFP aka SheetNfp
    public SheetNfp(INfpHelper nfpHelper, ISheet sheet, INfp part, double clipperScale, bool useDllImport)
      : this(RemoveOuterNfps(nfpHelper.GetInnerNfp(sheet, part, MinkowskiCache.Cache, clipperScale, useDllImport) ?? new INfp[0], sheet), sheet, part)
    {
    }

    private static INfp[] RemoveOuterNfps(INfp[] nfps, ISheet sheet)
    {
      IEnumerable<INfp> result = new List<INfp>(nfps);
      result = result.Where(o => o.WidthCalculated < sheet.WidthCalculated &&
                                 o.HeightCalculated < sheet.HeightCalculated)
                     .Select(o =>
                     {
                       var cleaned = SvgNest.CleanPolygon2(o);
                       var points = cleaned.Points.ToList();
                       points.Add(points[0]);
                       o.ReplacePoints(points);
                       return o;
                     });
      return result.ToArray();
    }

    internal bool CanAcceptPart
    {
      get
      {
        if (this.NumberOfNfps > 0)
        {
          if (this[0].Length == 0)
          {
            throw new ArgumentException();
          }
          else
          {
            return true;
          }
        }

        return false;
      }
    }

    public static new SheetNfp FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      var nfpCandidateList = JsonSerializer.Deserialize<NfpCandidateList>(json, options);
      return new SheetNfp(nfpCandidateList);
    }

    public static new SheetNfp LoadFromFile(string fileName)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd());
      }
    }

    internal object Shift(INfp processedPart)
    {
      throw new NotImplementedException();
    }

    public IPointXY GetCandidatePointClosestToOrigin()
    {
      SvgPoint result = null;
      for (int nfpCandidateIndex = 0; nfpCandidateIndex < this.NumberOfNfps; nfpCandidateIndex++)
      {
        var nfpCandidate = this[nfpCandidateIndex];
        for (int pointIndex = 0; pointIndex < nfpCandidate.Points.Length; pointIndex++)
        {
          var nfpPoint = nfpCandidate.Points[pointIndex];
          if (result == null ||
              nfpPoint.X < result.X ||
              (GeometryUtil.AlmostEqual(nfpPoint.X, result.X) && nfpPoint.Y < result.Y))
          {
            result = nfpPoint;
          }
        }
      }

      if (result == null)
      {
        throw new Exception("result null; shouldn't be possible; shouldn't get to method if the SheetNfp couldn't accept part.");
      }

      return result;
    }
  }
}