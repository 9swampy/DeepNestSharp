namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Text.Json.Serialization;
  using ClipperLib;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using Light.GuardClauses;

  public class NfpHelper : INfpHelper, ITestNfpHelper
  {
    private static volatile object lockobj = new object();

    private readonly Dictionary<string, INfp[]> cacheProcess = new Dictionary<string, INfp[]>();
    private bool useCacheProcess = true;
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

    private bool UseCacheProcess { get => this.useCacheProcess; }

    bool ITestNfpHelper.UseCacheProcess { get => this.useCacheProcess; set => this.useCacheProcess = value; }

    // inner nfps can be an array of nfps, outer nfps are always singular
    public static List<List<IntPoint>> InnerNfpToClipperCoordinates(IList<INfp> nfp, double clipperScale)
    {
      var clipperNfp = new List<List<IntPoint>>();
      for (var i = 0; i < nfp.Count(); i++)
      {
        var clip = NfpToClipperCoordinates(nfp[i], clipperScale);
        clipperNfp.AddRange(clip);

        // clipperNfp = clipperNfp.Concat(new[] { clip }).ToList();
      }

      return clipperNfp;
    }

    /// <summary>
    /// Generates a clipper nfp. Remember that clipper nfp are a list of polygons, not a tree.
    /// </summary>
    /// <param name="nfp"></param>
    /// <param name="clipperScale"></param>
    /// <returns></returns>
    public static List<List<IntPoint>> NfpToClipperCoordinates(INfp nfp, double clipperScale)
    {
      var clipperNfp = new List<List<IntPoint>>();

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
          var childNfp = DeepNestClipper.ScaleUpPath(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.PolygonArea(nfp) > 0)
      {
        nfp.Reverse();
      }

      // var outerNfp = SvgNest.toClipperCoordinates(nfp);

      // clipper js defines holes based on orientation
      var outerNfp = DeepNestClipper.ScaleUpPath(nfp.Points, clipperScale);

      // var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);
      clipperNfp.Add(outerNfp);

      // var area = Math.abs(ClipperLib.Clipper.Area(cleaned));
      return clipperNfp;
    }

    internal INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache minkowskiCache, double clipperScale, bool useDllImport)
    {
      return GetInnerNfp(sheet, part, minkowskiCache, clipperScale, useDllImport, o => { });
    }

    public INfp[] GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache minkowskiCache, double clipperScale, bool useDllImport, Action<string> verboseLog)
    {
      var result = GetInnerNfp((INfp)sheet, part, minkowskiCache, clipperScale, useDllImport, verboseLog);
      return result;
    }

    public INfp[] GetInnerNfp(INfp sheet, INfp part, MinkowskiCache minkowskiCache, double clipperScale, bool useDllImport, Action<string> verboseLog)
    {
      verboseLog.MustNotBeNull();
      sheet.MustNotBeNull();
      part.MustNotBeNull();

      var key = new DbCacheKey(sheet.Source, part.Source, 0, part.Rotation);

      // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
      var res = Window.Find(key, true);
      if (res != null)
      {
        verboseLog($"GetInnerNfp found {key}");
        return res;
      }

      var nfp = GetInnerNfp(sheet, part, minkowskiCache, useDllImport, verboseLog);

      if (nfp == null || nfp.Children == null || nfp.Children.Count == 0)
      {
        return null;
      }

      List<INfp> holes = new List<INfp>();
      if (sheet.Children != null && sheet.Children.Count > 0)
      {
        for (var i = 0; i < sheet.Children.Count; i++)
        {
          var hnfp = GetOuterNfp(sheet.Children[i], part, MinkowskiCache.NoCache, useDllImport);
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

      var clipperNfp = InnerNfpToClipperCoordinates(nfp.Children, clipperScale);
      var clipperHoles = InnerNfpToClipperCoordinates(holes, clipperScale);

      List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
      var clipper = new Clipper();

      clipper.AddPaths(clipperHoles, PolyType.ptClip, true);
      clipper.AddPaths(clipperNfp, PolyType.ptSubject, true);

      if (!clipper.Execute(ClipType.ctDifference, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
      {
        return nfp.Children.ToArray();
      }

      if (finalNfp.Count == 0)
      {
        return null;
      }

      List<NoFitPolygon> f = new List<NoFitPolygon>();
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

    internal INfp[] ExecuteDllImportMinkowski(INfp path, INfp pattern, MinkowskiCache minkowskiCache, bool useDllImport)
    {
      var key = new StringBuilder(12).Append(path.Source).Append(";").Append(pattern.Source).Append(";").Append(path.Rotation).Append(";").Append(pattern.Rotation).ToString();
      bool cacheAllow = minkowskiCache == MinkowskiCache.Cache;

      if (UseCacheProcess && cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      INfp[] res = ((ITestNfpHelper)this).ExecuteInterchangeableMinkowski(useDllImport, path, pattern);
      if (UseCacheProcess && cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    INfp[] ITestNfpHelper.ExecuteInterchangeableMinkowski(bool useDllImport, INfp path, INfp pattern)
    {
      return ExecuteInterchangeableMinkowski(useDllImport, path, pattern);
    }

    /// <summary>
    /// Attempt to fit the part passed in inside the given sheet.
    /// </summary>
    /// <param name="sheet">The outer sheet on which to try fit the part.</param>
    /// <param name="part">The part to try fit within the sheet.</param>
    /// <param name="minkowskiCache">A value indicating whether to cache the result.</param>
    /// <returns>The generated InnerFitPolygon if found, otherwise null.</returns>
    internal INfp GetInnerNfp(ISheet sheet, INfp part, MinkowskiCache minkowskiCache, bool useDllImport)
    {
      return GetInnerNfp((INfp)sheet, part, minkowskiCache, useDllImport, o => { });
    }

    /// <summary>
    /// Given two polygons A and B and their respective reference vertices rA and rB with rA placed at the origin.
    /// The NFPAB represents all possible positions for which B touches but does not overlap A.
    /// It is important to note that no rotations of the input polygons are allowed.
    /// </summary>
    /// <param name="a">The stationary polygon.</param>
    /// <param name="b">The polygon that circumnavigates a.</param>
    /// <param name="minkowskiCache">Whether or not to cache the result.</param>
    /// <param name="useDllImport">Whether or not to use the C++ Dll import.</param>
    /// <returns>NoFitPolygon AB.</returns>
    internal INfp GetOuterNfp(INfp a, INfp b, MinkowskiCache minkowskiCache, bool useDllImport)
    {
      return GetNoFitPolygon(a, b, minkowskiCache, NoFitPolygonType.Outer, useDllImport);
    }

    /// <summary>
    /// Expands the part passed in by 10% to generate a frame and adds the original <see cref="INfp"/> passed in as a child of the frame.
    /// </summary>
    /// <param name="a">Part to expand.</param>
    /// <returns>Expanded frame with passsed in a as a child.</returns>
    internal static INfp GetExpandedFrame(INfp a)
    {
      var bounds = GeometryUtil.GetPolygonBounds(a);

      // expand bounds by 10%
      bounds.Width *= 1.1;
      bounds.Height *= 1.1;
      bounds.X -= 0.5 * (bounds.Width - (bounds.Width / 1.1));
      bounds.Y -= 0.5 * (bounds.Height - (bounds.Height / 1.1));

      var frame = new NoFitPolygon(new List<INfp>() { a });
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y + bounds.Height));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y + bounds.Height));

      frame.Source = a.Source;
      frame.Rotation = 0;

      if (!frame.IsClosed)
      {
#if NCRUNCH
        //throw new NotImplementedException("Clipper substitute for DllImport expects paths closed.");
#endif
        frame.EnsureIsClosed();
      }

      return frame;
    }

    /// <summary>
    /// Attempt to fit the part passed in inside the given envelope. This method can only only
    /// be used for fitting on empty sheet or in a hole if it's not been used yet because it can 
    /// only return a single NFP.
    /// </summary>
    /// <param name="envelope">The outer envelope inside which to try fit the part.</param>
    /// <param name="part">The part to try fit inside the envelope.</param>
    /// <param name="minkowskiCache">A value indicating whether to cache the result.</param>
    /// <returns>The generated InnerFitPolygon.</returns>
    private INfp GetInnerNfp(INfp envelope, INfp part, MinkowskiCache minkowskiCache, bool useDllImport, Action<string> verboseLog)
    {
      INfp frame;
      if (useDllImport)
      {
        frame = GetExpandedFrame(envelope);
      }
      else
      {
        // With the NewClipperMinkowskiSum we were getting an inner result when fitting
        // a larger part on a smaller sheet which should have been impossible so guard
        // against it.
        if (part.WidthCalculated >= envelope.WidthCalculated ||
            part.HeightCalculated >= envelope.HeightCalculated)
        {
          verboseLog("GetInnerNfp guarded NewClipperMinkowskiSum against fitting larger part on smaller sheet.");
          return new NoFitPolygon();
        }

        frame = new NoFitPolygon(envelope, WithChildren.Excluded);
        frame.EnsureIsClosed();
      }

      return GetNoFitPolygon(frame, part, minkowskiCache, NoFitPolygonType.Inner, useDllImport);
    }

    private INfp GetNoFitPolygon(INfp a, INfp b, MinkowskiCache minkowskiCache, NoFitPolygonType nfpType, bool useDllImport)
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
            nfpList = ExecuteDllImportMinkowski(a, b, minkowskiCache, useDllImport);
          }
          else
          {
            NoFitPolygon clipperNfp = minkowskiSumService.ClipperExecuteOuterNfp(a.Points, b.Points, MinkowskiSumPick.Smallest);
            nfpList = new NoFitPolygon[] { new NoFitPolygon(clipperNfp.Points) };
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

    private INfp[] ExecuteInterchangeableMinkowski(bool useDllImport, INfp path, INfp pattern)
    {
      INfp[] res;

      if (!path.IsClosed)
      {
#if NCRUNCH
          // throw new NotImplementedException("The implementation Fel88 added executes path is closed so I'd expect the path to be closed. . .");
          path.EnsureIsClosed();
#else
        // System.Diagnostics.Debugger.Break();
        path.EnsureIsClosed();
#endif
      }

      if (useDllImport)
      {
        //nfp = Process2(A, B, type);
        //res = minkowskiSumService.DllImportExecute(path, pattern, MinkowskiSumCleaning.Cleaned);
        // For a long time through the refactor had been using MinkowskiSumCleaning.None and only started to pull in 
        // clean to get NewMinkowski to be same as incumbent but changing from none is wrong thing to do surely; should
        // be keeping as it was not getting the origina lto match with the added.
        // The only place calling the method is this; so shift all UnitTests that have gone the cleaned way to none.
        res = minkowskiSumService.DllImportExecute(path, pattern, MinkowskiSumCleaning.None);
      }
      else
      {
        res = minkowskiSumService.NewMinkowskiSum(pattern.Points, path, WithChildren.Included, takeOnlyBiggestArea: false);
        foreach (var nfp in res)
        {
          nfp.EnsureIsClosed();
        }
      }

      return res;
    }
  }
}