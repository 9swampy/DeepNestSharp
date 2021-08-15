namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using ClipperLib;
  using Light.GuardClauses;

  public class NfpHelper
  {
    private static volatile object lockobj = new object();

    private readonly Dictionary<string, INfp[]> cacheProcess = new Dictionary<string, INfp[]>();
    private readonly IMinkowskiSumService minkowskiSumService;

    // run the placement synchronously
    private readonly IWindowUnk window;

    public NfpHelper(IMinkowskiSumService minkowskiSumService, IWindowUnk window)
    {
      this.minkowskiSumService = minkowskiSumService;
      this.window = window;
    }

    // inner nfps can be an array of nfps, outer nfps are always singular
    public static IntPoint[][] InnerNfpToClipperCoordinates(INfp[] nfp, double clipperScale)
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

    // returns clipper nfp. Remember that clipper nfp are a list of polygons, not a tree!
    public static IntPoint[][] NfpToClipperCoordinates(INfp nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

      // children first
      if (nfp.Children != null && nfp.Children.Count > 0)
      {
        for (var j = 0; j < nfp.Children.Count; j++)
        {
          if (GeometryUtil.polygonArea(nfp.Children[j]) < 0)
          {
            nfp.Children[j].Reverse();
          }

          // var childNfp = SvgNest.toClipperCoordinates(nfp.Children[j]);
          var childNfp = DeepNestClipper.ScaleUpPaths(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.polygonArea(nfp) > 0)
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

    internal INfp[] ExecuteDllImportMinkowski(INfp a, INfp b, MinkowskiCache minkowskiCache)
    {
      var key = new StringBuilder(12).Append(a.Source).Append(";").Append(b.Source).Append(";").Append(a.Rotation).Append(";").Append(b.Rotation).ToString();
      bool cacheAllow = minkowskiCache == MinkowskiCache.Cache;

      if (cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      var ret = minkowskiSumService.DllImportExecute(a, b, MinkowskiSumCleaning.None);
      var res = new INfp[] { ret };
      if (cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    internal INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache type, double clipperScale)
    {
      a.MustNotBeNull();
      b.MustNotBeNull();

      var key = new DbCacheKey(a.Source, b.Source, 0, b.Rotation);

      // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
      var res = window.Find(key, true);
      if (res != null)
      {
        return res;
      }

      var frame = GetFrame(a);

      var nfp = GetOuterNfp(frame, b, type, true);

      if (nfp == null || nfp.Children == null || nfp.Children.Count == 0)
      {
        return null;
      }

      List<INfp> holes = new List<INfp>();
      if (a.Children != null && a.Children.Count > 0)
      {
        for (var i = 0; i < a.Children.Count; i++)
        {
          var hnfp = GetOuterNfp(a.Children[i], b, MinkowskiCache.NoCache);
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

      if (a.Source != null && b.Source != null)
      {
        // insert into db
        // console.log('inserting inner: ', A.source, B.source, B.rotation, f);
        var doc = new DbCacheKey(a.Source, b.Source, 0, b.Rotation, f.ToArray());
        window.Insert(doc, true);
      }

      return f.ToArray();
    }

    internal INfp GetOuterNfp(INfp a, INfp b, MinkowskiCache type, bool inside = false) // todo:?inside def?
    {
      INfp[] nfp;

      var key = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation);

      lock (lockobj)
      {
        var doc = window.Find(key);
        if (doc != null)
        {
          return doc.First();
        }

        // not found in cache
        if (inside || (a.Children != null && a.Children.Count > 0))
        {
          nfp = ExecuteDllImportMinkowski(a, b, type);
        }
        else
        {
          NFP clipperNfp = minkowskiSumService.ClipperExecute(a, b, MinkowskiSumPick.Smallest);
          nfp = new NFP[] { new NFP(clipperNfp.Points) };
        }

        if (nfp == null || nfp.Length == 0)
        {
          // console.log('holy shit', nfp, A, B, JSON.stringify(A), JSON.stringify(B));
          return null;
        }

        INfp nfps = nfp.FirstOrDefault();
        if (nfps == null || nfps.Length == 0)
        {
          return null;
        }

        if (!inside)
        {
          var doc2 = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation, nfp);
          window.Insert(doc2);
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
        return nfps;
      }
    }

    private static INfp GetFrame(INfp a)
    {
      var bounds = GeometryUtil.getPolygonBounds(a);

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