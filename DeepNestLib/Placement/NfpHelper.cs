namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Text.Json.Serialization;
  using ClipperLib;
  using DeepNestLib.Geometry;
  using Light.GuardClauses;

  public class NfpHelper : INfpHelper, ITestNfpHelper
  {
    private static volatile object lockobj = new object();

    private readonly Dictionary<string, INfp[]> cacheProcess = new Dictionary<string, INfp[]>();
    private IMinkowskiSumService minkowskiSumService;

    [JsonConstructor]
    public NfpHelper()
    {
    }

    public NfpHelper(IMinkowskiSumService minkowskiSumService, IWindowUnk window)
    {
      this.minkowskiSumService = minkowskiSumService;
      this.Window = window;
    }

    [JsonInclude]
    public IWindowUnk Window { get; private set; } = new WindowUnk();

    [JsonInclude]
    public IMinkowskiSumService MinkowskiSumService { get => this.minkowskiSumService; set => this.minkowskiSumService = value; }

    IMinkowskiSumService ITestNfpHelper.MinkowskiSumService { get => this.minkowskiSumService; set => this.minkowskiSumService = value; }

    // inner nfps can be an array of nfps, outer nfps are always singular
    public static IntPoint[][] InnerNfpToClipperCoordinates(IList<INfp> nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
      for (var i = 0; i < nfp.Count(); i++)
      {
        var clip = NfpToClipperCoordinates(nfp[i], clipperScale);
        clipperNfp.AddRange(clip);

        // clipperNfp = clipperNfp.Concat(new[] { clip }).ToList();
      }

      return clipperNfp.ToArray();
    }

    /// <summary>
    /// Generates a clipper nfp. Remember that clipper nfp are a list of polygons, not a tree.
    /// </summary>
    /// <param name="nfp"></param>
    /// <param name="clipperScale"></param>
    /// <returns></returns>
    public static IntPoint[][] NfpToClipperCoordinates(INfp nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

      // children first
      if (nfp.Children != null && nfp.Children.Count > 0)
      {
        for (var j = 0; j < nfp.Children.Count; j++)
        {
          if (GeometryUtil.PolygonArea(nfp.Children[j]) < 0)
          {
            nfp.Children[j].Reverse();
          }

          // var childNfp = SvgNest.toClipperCoordinates(nfp.Children[j]);
          var childNfp = DeepNestClipper.ScaleUpPaths(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.PolygonArea(nfp) > 0)
      {
        nfp.Reverse();
      }

      // var outerNfp = SvgNest.toClipperCoordinates(nfp);

      // clipper js defines holes based on orientation
      var outerNfp = DeepNestClipper.ScaleUpPaths(nfp.Points, clipperScale);

      // var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);
      clipperNfp.Add(outerNfp);

      // var area = Math.abs(ClipperLib.Clipper.Area(cleaned));
      return clipperNfp.ToArray();
    }

    public INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache minkowskiCache, double clipperScale)
    {
      return GetInnerNfp((INfp)sheet, part, minkowskiCache, clipperScale);
    }

    public INfp[] GetInnerNfp(INfp sheet, INfp part, MinkowskiCache minkowskiCache, double clipperScale)
    {
      sheet.MustNotBeNull();
      part.MustNotBeNull();

      var key = new DbCacheKey(sheet.Source, part.Source, 0, part.Rotation);

      // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
      var res = Window.Find(key, true);
      if (res != null)
      {
        return res;
      }

      var frame = GetExpandedFrame(sheet);

      var nfp = GetOuterNfp(frame, part, minkowskiCache, NoFitPolygonType.Inner);

      if (nfp == null || nfp.Children == null || nfp.Children.Count == 0)
      {
        return null;
      }

      List<INfp> holes = new List<INfp>();
      if (sheet.Children != null && sheet.Children.Count > 0)
      {
        for (var i = 0; i < sheet.Children.Count; i++)
        {
          var hnfp = GetOuterNfp(sheet.Children[i], part, MinkowskiCache.NoCache);
          if (hnfp != null)
          {
            holes.Add(hnfp);
          }
        }
      }

      if (holes.Count == 0)
      {
        return nfp.Children.ToArray();
      }

      var clipperNfp = InnerNfpToClipperCoordinates(nfp.Children.ToArray(), clipperScale);
      var clipperHoles = InnerNfpToClipperCoordinates(holes.ToArray(), clipperScale);

      List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
      var clipper = new Clipper();

      clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
      clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      if (!clipper.Execute(ClipType.ctDifference, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
      {
        return nfp.Children.ToArray();
      }

      if (finalNfp.Count == 0)
      {
        return null;
      }

      List<NFP> f = new List<NFP>();
      for (var i = 0; i < finalNfp.Count; i++)
      {
        f.Add(finalNfp[i].ToArray().ToNestCoordinates(clipperScale));
      }

      if (sheet?.Source != null && part?.Source != null)
      {
        // insert into db
        // console.log('inserting inner: ', A.source, B.source, B.rotation, f);
        var keyItem = new DbCacheKey(sheet.Source, part.Source, 0, part.Rotation, f.ToArray());
        Window.Insert(keyItem, true);
      }

      return f.ToArray();
    }

    internal INfp[] ExecuteDllImportMinkowski(INfp path, INfp pattern, MinkowskiCache minkowskiCache)
    {
      var key = new StringBuilder(12).Append(path.Source).Append(";").Append(pattern.Source).Append(";").Append(path.Rotation).Append(";").Append(pattern.Rotation).ToString();
      bool cacheAllow = minkowskiCache == MinkowskiCache.Cache;

      if (cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      var res = minkowskiSumService.DllImportExecute(path, pattern, MinkowskiSumCleaning.None);
      if (cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    internal INfp GetOuterNfp(INfp a, INfp b, MinkowskiCache minkowskiCache, NoFitPolygonType nfpType = NoFitPolygonType.Outer)
    {
      try
      {
        INfp[] nfpList;

        var key = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation);

        lock (lockobj)
        {
          var doc = Window.Find(key);
          if (doc != null)
          {
            return doc.First();
          }

          // not found in cache
          if (nfpType == NoFitPolygonType.Inner || (a.Children != null && a.Children.Count > 0))
          {
            nfpList = ExecuteDllImportMinkowski(a, b, minkowskiCache);
          }
          else
          {
            NFP clipperNfp = minkowskiSumService.ClipperExecute(a, b, MinkowskiSumPick.Smallest);
            nfpList = new NFP[] { new NFP(clipperNfp.Points) };
          }

          if (nfpList == null || nfpList.Length == 0)
          {
            // console.log('holy shit', nfp, A, B, JSON.stringify(A), JSON.stringify(B));
            return null;
          }

          INfp nfp = nfpList.FirstOrDefault();
          if (nfp == null || nfp.Length == 0)
          {
            return null;
          }

          if (nfpType == NoFitPolygonType.Outer)
          {
            var doc2 = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation, nfpList);
            Window.Insert(doc2);
          }

          /*
          if (!inside && typeof A.source !== 'undefined' && typeof B.source !== 'undefined')
          {
              // insert into db
              doc = {
                  A: A.source,
          B: B.source,
          Arotation: A.rotation,
          Brotation: B.rotation,
          nfp: nfp

        };
              window.db.insert(doc);
          }
          */
          return nfp;
        }
      }
      catch (BadImageFormatException)
      {
        throw;
      }
      catch (System.Exception ex)
      {
        throw;
      }
    }

    /// <summary>
    /// Expands the part passed in by 10% to generate a frame and adds the original <see cref="INfp"/> passed in as a child of the frame.
    /// </summary>
    /// <param name="a">Part to expand.</param>
    /// <returns>Expanded frame with passsed in a as a child.</returns>
    private static INfp GetExpandedFrame(INfp a)
    {
      var bounds = GeometryUtil.GetPolygonBounds(a);

      // expand bounds by 10%
      bounds.Width *= 1.1;
      bounds.Height *= 1.1;
      bounds.X -= 0.5 * (bounds.Width - (bounds.Width / 1.1));
      bounds.Y -= 0.5 * (bounds.Height - (bounds.Height / 1.1));

      var frame = new NFP(new List<INfp>() { a });
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y + bounds.Height));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y + bounds.Height));

      frame.Source = a.Source;
      frame.Rotation = 0;

      return frame;
    }
  }
}