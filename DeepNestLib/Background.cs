﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Drawing;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;
  using ClipperLib;
  using Minkowski;

  public class Background
  {
    public static bool EnableCaches = true;

    public Background()
    {
      this.cacheProcess = new Dictionary<string, NFP[]>();
      this.window = new windowUnk();
      this.callCounter = 0;
    }

    public static NFP shiftPolygon(NFP p, PlacementItem shift)
    {
      NFP shifted = new NFP();
      for (var i = 0; i < p.Length; i++)
      {
        shifted.AddPoint(new SvgPoint(p[i].x + shift.x, p[i].y + shift.y) { exact = p[i].exact });
      }

      if (p.Children != null /*&& p.Children.Count*/)
      {
        for (int i = 0; i < p.Children.Count(); i++)
        {
          shifted.Children.Add(shiftPolygon(p.Children[i], shift));
        }
      }

      return shifted;
    }

    // returns the square of the length of any merged lines
    // filter out any lines less than minlength long
    public static MergedResult mergedLength(NFP[] parts, NFP p, double minlength, double tolerance)
    {
      // var min2 = minlength * minlength;
      //            var totalLength = 0;
      //            var segments = [];

      // for (var i = 0; i < p.length; i++)
      //            {
      //                var A1 = p[i];

      // if (i + 1 == p.length)
      //                {
      //                    A2 = p[0];
      //                }
      //                else
      //                {
      //                    var A2 = p[i + 1];
      //                }

      // if (!A1.exact || !A2.exact)
      //                {
      //                    continue;
      //                }

      // var Ax2 = (A2.x - A1.x) * (A2.x - A1.x);
      //                var Ay2 = (A2.y - A1.y) * (A2.y - A1.y);

      // if (Ax2 + Ay2 < min2)
      //                {
      //                    continue;
      //                }

      // var angle = Math.atan2((A2.y - A1.y), (A2.x - A1.x));

      // var c = Math.cos(-angle);
      //                var s = Math.sin(-angle);

      // var c2 = Math.cos(angle);
      //                var s2 = Math.sin(angle);

      // var relA2 = { x: A2.x - A1.x, y: A2.y - A1.y};
      //            var rotA2x = relA2.x * c - relA2.y * s;

      // for (var j = 0; j < parts.length; j++)
      //            {
      //                var B = parts[j];
      //                if (B.length > 1)
      //                {
      //                    for (var k = 0; k < B.length; k++)
      //                    {
      //                        var B1 = B[k];

      // if (k + 1 == B.length)
      //                        {
      //                            var B2 = B[0];
      //                        }
      //                        else
      //                        {
      //                            var B2 = B[k + 1];
      //                        }

      // if (!B1.exact || !B2.exact)
      //                        {
      //                            continue;
      //                        }
      //                        var Bx2 = (B2.x - B1.x) * (B2.x - B1.x);
      //                        var By2 = (B2.y - B1.y) * (B2.y - B1.y);

      // if (Bx2 + By2 < min2)
      //                        {
      //                            continue;
      //                        }

      // // B relative to A1 (our point of rotation)
      //                        var relB1 = { x: B1.x - A1.x, y: B1.y - A1.y};
      //                    var relB2 = { x: B2.x - A1.x, y: B2.y - A1.y};

      // // rotate such that A1 and A2 are horizontal
      //                var rotB1 = { x: relB1.x* c -relB1.y * s, y: relB1.x* s +relB1.y * c};
      //            var rotB2 = { x: relB2.x* c -relB2.y * s, y: relB2.x* s +relB2.y * c};

      // if(!GeometryUtil.almostEqual(rotB1.y, 0, tolerance) || !GeometryUtil.almostEqual(rotB2.y, 0, tolerance)){
      // continue;
      // }

      // var min1 = Math.min(0, rotA2x);
      //        var max1 = Math.max(0, rotA2x);

      // var min2 = Math.min(rotB1.x, rotB2.x);
      //        var max2 = Math.max(rotB1.x, rotB2.x);

      // // not overlapping
      // if(min2 >= max1 || max2 <= min1){
      // continue;
      // }

      // var len = 0;
      //        var relC1x = 0;
      //        var relC2x = 0;

      // // A is B
      // if(GeometryUtil.almostEqual(min1, min2) && GeometryUtil.almostEqual(max1, max2)){
      // len = max1-min1;
      // relC1x = min1;
      // relC2x = max1;
      // }
      // // A inside B
      // else if(min1 > min2 && max1<max2){
      // len = max1-min1;
      // relC1x = min1;
      // relC2x = max1;
      // }
      // // B inside A
      // else if(min2 > min1 && max2<max1){
      // len = max2-min2;
      // relC1x = min2;
      // relC2x = max2;
      // }
      // else{
      // len = Math.max(0, Math.min(max1, max2) - Math.max(min1, min2));
      // relC1x = Math.min(max1, max2);
      // relC2x = Math.max(min1, min2);
      // }

      // if(len* len > min2){
      // totalLength += len;

      // var relC1 = { x: relC1x * c2, y: relC1x * s2 };
      // var relC2 = { x: relC2x * c2, y: relC2x * s2 };

      // var C1 = { x: relC1.x + A1.x, y: relC1.y + A1.y };
      // var C2 = { x: relC2.x + A1.x, y: relC2.y + A1.y };

      // segments.push([C1, C2]);
      // }
      // }
      // }

      // if(B.Children && B.Children.length > 0){
      // var child = mergedLength(B.Children, p, minlength, tolerance);
      // totalLength += child.totalLength;
      // segments = segments.concat(child.segments);
      // }
      // }
      // }

      // return {totalLength: totalLength, segments: segments};
      throw new NotImplementedException();
    }

    public class MergedResult
    {
      public double totalLength;
      public object segments;
    }

    public static NFP[] cloneNfp(NFP[] nfp, bool inner = false)
    {
      if (!inner)
      {
        return new[] { nfp.First().Clone() };
      }

      System.Diagnostics.Debug.Print("Original source had marked this 'Background.cloneNfp' as not implemented; not sure why. . .");

      // inner nfp is actually an array of nfps
      List<NFP> result = new List<NFP>();
      for (var i = 0; i < nfp.Count(); i++)
      {
        result.Add(nfp[i].Clone());
      }

      return result.ToArray();
    }

    public int callCounter = 0;

    public Dictionary<string, NFP[]> cacheProcess = new Dictionary<string, NFP[]>();

    internal NFP[] Process2(INfp A, INfp B, int type)
    {
      var key = A.Source + ";" + B.Source + ";" + A.Rotation + ";" + B.Rotation;
      bool cacheAllow = type != 1;
      if (cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      Stopwatch swg = Stopwatch.StartNew();
      Dictionary<string, List<PointF>> dic1 = new Dictionary<string, List<PointF>>();
      Dictionary<string, List<double>> dic2 = new Dictionary<string, List<double>>();
      dic2.Add("A", new List<double>());
      foreach (var item in A.Points)
      {
        var target = dic2["A"];
        target.Add(item.x);
        target.Add(item.y);
      }

      dic2.Add("B", new List<double>());
      foreach (var item in B.Points)
      {
        var target = dic2["B"];
        target.Add(item.x);
        target.Add(item.y);
      }

      List<double> hdat = new List<double>();

      foreach (var item in A.Children)
      {
        foreach (var pitem in item.Points)
        {
          hdat.Add(pitem.x);
          hdat.Add(pitem.y);
        }
      }

      var aa = dic2["A"];
      var bb = dic2["B"];
      var arr1 = A.Children.Select(z => z.Points.Count() * 2).ToArray();

#if x64
      System.Diagnostics.Debug.Print("Minkowski_x64");
      long[] longs = arr1.Select(o => (long)o).ToArray();
      MinkowskiWrapper.setData(aa.Count, aa.ToArray(), A.Children.Count, longs, hdat.ToArray(), bb.Count, bb.ToArray());
#elif x86
      System.Diagnostics.Debug.Print("Minkowski_x86");
      MinkowskiWrapper.setData(aa.Count, aa.ToArray(), A.Children.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
#else
      System.Diagnostics.Debug.Print("Minkowski_AnyCpu");
      MinkowskiWrapper.setData(aa.Count, aa.ToArray(), A.Children.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
#endif
      MinkowskiWrapper.calculateNFP();

      this.callCounter++;

      int[] sizes = new int[2];
      MinkowskiWrapper.getSizes1(sizes);
      int[] sizes1 = new int[sizes[0]];
      int[] sizes2 = new int[sizes[1]];
      MinkowskiWrapper.getSizes2(sizes1, sizes2);
      double[] dat1 = new double[sizes1.Sum()];
      double[] hdat1 = new double[sizes2.Sum()];

      MinkowskiWrapper.getResults(dat1, hdat1);

      if (sizes1.Count() > 1)
      {
        throw new ArgumentException("sizes1 cnt >1");
      }

      // convert back to answer here
      bool isa = true;
      List<PointF> Apts = new List<PointF>();

      List<List<double>> holesval = new List<List<double>>();
      bool holes = false;

      for (int i = 0; i < dat1.Length; i += 2)
      {
        var x1 = (float)dat1[i];
        var y1 = (float)dat1[i + 1];
        Apts.Add(new PointF(x1, y1));
      }

      int index = 0;
      for (int i = 0; i < sizes2.Length; i++)
      {
        holesval.Add(new List<double>());
        for (int j = 0; j < sizes2[i]; j++)
        {
          holesval.Last().Add(hdat1[index]);
          index++;
        }
      }

      List<List<PointF>> holesout = new List<List<PointF>>();
      foreach (var item in holesval)
      {
        holesout.Add(new List<PointF>());
        for (int i = 0; i < item.Count; i += 2)
        {
          var x = (float)item[i];
          var y = (float)item[i + 1];
          holesout.Last().Add(new PointF(x, y));
        }
      }

      NFP ret = new NFP();
      foreach (var item in Apts)
      {
        ret.AddPoint(new SvgPoint(item.X, item.Y));
      }

      foreach (var item in holesout)
      {
        ret.Children.Add(new NFP());
        foreach (var hitem in item)
        {
          ret.Children.Last().AddPoint(new SvgPoint(hitem.X, hitem.Y));
        }
      }

      swg.Stop();
      var msg = swg.ElapsedMilliseconds;
      var res = new NFP[] { ret };

      if (cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    public static NFP getFrame(NFP A)
    {
      var bounds = GeometryUtil.getPolygonBounds(A);

      // expand bounds by 10%
      bounds.width *= 1.1;
      bounds.height *= 1.1;
      bounds.x -= 0.5 * (bounds.width - (bounds.width / 1.1));
      bounds.y -= 0.5 * (bounds.height - (bounds.height / 1.1));

      var frame = new NFP(new List<NFP>() { A });
      frame.Push(new SvgPoint(bounds.x, bounds.y));
      frame.Push(new SvgPoint(bounds.x + bounds.width, bounds.y));
      frame.Push(new SvgPoint(bounds.x + bounds.width, bounds.y + bounds.height));
      frame.Push(new SvgPoint(bounds.x, bounds.y + bounds.height));

      frame.Source = A.Source;
      frame.Rotation = 0;

      return frame;
    }

    private NFP[] getInnerNfp(NFP A, NFP B, int type, double clipperScale)
    {
      if (A.Source != null && B.Source != null)
      {
        var key = new DbCacheKey(A.Source, B.Source, 0, B.Rotation);

        // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
        var res = window.Find(key, true);
        if (res != null)
        {
          return res;
        }
      }

      var frame = getFrame(A);

      var nfp = GetOuterNfp(frame, B, type, true);

      if (nfp == null || nfp.Children == null || nfp.Children.Count == 0)
      {
        return null;
      }

      List<NFP> holes = new List<NFP>();
      if (A.Children != null && A.Children.Count > 0)
      {
        for (var i = 0; i < A.Children.Count; i++)
        {
          var hnfp = GetOuterNfp(A.Children[i], B, 1);
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
      var clipper = new ClipperLib.Clipper();

      clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptClip, true);
      clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

      if (!clipper.Execute(ClipperLib.ClipType.ctDifference, finalNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
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
        f.Add(ToNestCoordinates(finalNfp[i].ToArray(), clipperScale));
      }

      if (A.Source != null && B.Source != null)
      {
        // insert into db
        // console.log('inserting inner: ', A.source, B.source, B.rotation, f);
        var doc = new DbCacheKey(A.Source, B.Source, 0, B.Rotation, f.ToArray());
        window.Insert(doc, true);
      }

      return f.ToArray();
    }

    internal SheetPlacement PlaceParts(IEnumerable<NFP> sheets, NFP[] parts, ISvgNestConfig config, int nestindex)
    {
      if (sheets == null || sheets.Count() == 0)
      {
        return null;
      }

      Queue<NFP> unusedSheets = new Queue<NFP>(sheets);

      double totalsheetarea = 0;

      NFP part = null;

      // total length of merged lines
      double totalMerged = 0;

      // rotate paths by given rotation
      var rotated = new List<NFP>();
      for (int i = 0; i < parts.Length; i++)
      {
        var r = parts[i].Rotate(parts[i].Rotation);
        r.Rotation = parts[i].Rotation;
        r.Source = parts[i].Source;
        r.Id = parts[i].Id;
        rotated.Add(r);
      }

      parts = rotated.ToArray();

      List<SheetPlacementItem> allplacements = new List<SheetPlacementItem>();

      double fitness = 0;

      NFP nfp;
      double sheetarea = -1;
      int totalPlaced = 0;
      int totalParts = parts.Count();

      while (parts.Length > 0 && unusedSheets.Count > 0)
      {
        List<NFP> placed = new List<NFP>();
        List<PlacementItem> placements = new List<PlacementItem>();

        // open a new sheet
        var sheet = unusedSheets.Dequeue();
        sheetarea = Math.Abs(GeometryUtil.polygonArea(sheet));
        totalsheetarea += sheetarea;

        fitness += sheetarea; // add 1 for each new sheet opened (lower fitness is better)

        string clipkey = string.Empty;
        Dictionary<string, ClipCacheItem> clipCache = new Dictionary<string, ClipCacheItem>();
        var clipper = new ClipperLib.Clipper();
        var combinedNfp = new List<List<ClipperLib.IntPoint>>();
        var error = false;
        IntPoint[][] clipperSheetNfp = null;
        double? minwidth = null;

        double? minarea = null;
        for (int i = 0; i < parts.Length; i++)
        {
          part = parts[i];

          // inner NFP
          NFP[] sheetNfp = null;
          var canBePlaced = false;

          // try all possible rotations until it fits
          // (only do this for the first part of each sheet, to ensure that all parts that can be placed are, even if we have to to open a lot of sheets)
          for (int j = 0; j < (360f / config.Rotations); j++)
          {
            if (CanBePlaced(sheet, part, config.ClipperScale, out sheetNfp))
            {
              canBePlaced = true;
              break;
            }

            var r = part.Rotate(360f / config.Rotations);
            r.Rotation = part.Rotation + (360f / config.Rotations);
            r.Source = part.Source;
            r.Id = part.Id;

            // rotation is not in-place
            part = r;
            parts[i] = r;

            if (part.Rotation > 360f)
            {
              part.Rotation = part.Rotation % 360f;
            }
          }

          // part unplaceable, skip
          if (!canBePlaced)
          {
            continue;
          }

          PlacementItem position = null;
          if (placed.Count == 0)
          {
            // first placement, put it on the bottom left corner
            for (int j = 0; j < sheetNfp.Count(); j++)
            {
              for (int k = 0; k < sheetNfp[j].Length; k++)
              {
                if (position == null ||
                    ((sheetNfp[j][k].x - part[0].x) < position.x) ||
                    (
                    GeometryUtil._almostEqual(sheetNfp[j][k].x - part[0].x, position.x)
                    && ((sheetNfp[j][k].y - part[0].y) < position.y)))
                {
                  position = new PlacementItem()
                  {
                    x = sheetNfp[j][k].x - part[0].x,
                    y = sheetNfp[j][k].y - part[0].y,
                    id = part.Id,
                    rotation = part.Rotation,
                    source = part.Source,
                  };
                }
              }
            }

            if (position == null)
            {
              throw new Exception("position null");

              // console.log(sheetNfp);
            }

            placements.Add(position);
            placed.Add(part);
            totalPlaced++;

            continue;
          }

          clipperSheetNfp = InnerNfpToClipperCoordinates(sheetNfp, config.ClipperScale);

          clipper = new ClipperLib.Clipper();
          combinedNfp = new List<List<ClipperLib.IntPoint>>();

          error = false;

          // check if stored in clip cache
          // var startindex = 0;
          clipkey = "s:" + part.Source + "r:" + part.Rotation;
          var startindex = 0;
          if (EnableCaches && clipCache.ContainsKey(clipkey))
          {
            var prevNfp = clipCache[clipkey].nfpp;
            clipper.AddPaths(prevNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
            startindex = clipCache[clipkey].index;
          }

          for (int j = startindex; j < placed.Count; j++)
          {
            nfp = GetOuterNfp(placed[j], part, 0);

            // minkowski difference failed. very rare but could happen
            if (nfp == null)
            {
              error = true;
              break;
            }

            // shift to placed location
            for (int m = 0; m < nfp.Length; m++)
            {
              nfp[m].x += placements[j].x;
              nfp[m].y += placements[j].y;
            }

            if (nfp.Children != null && nfp.Children.Count > 0)
            {
              for (int n = 0; n < nfp.Children.Count; n++)
              {
                for (var o = 0; o < nfp.Children[n].Length; o++)
                {
                  nfp.Children[n][o].x += placements[j].x;
                  nfp.Children[n][o].y += placements[j].y;
                }
              }
            }

            var clipperNfp = NfpToClipperCoordinates(nfp, config.ClipperScale);

            clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
          }

          // TODO: a lot here to insert
          if (error || !clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
          {
            // console.log('clipper error', error);
            continue;
          }

          if (EnableCaches)
          {
            clipCache[clipkey] = new ClipCacheItem()
            {
              index = placed.Count - 1,
              nfpp = combinedNfp.Select(z => z.ToArray()).ToArray(),
            };
          }

          // console.log('save cache', placed.length - 1);

          // difference with sheet polygon
          List<List<IntPoint>> differenceWithSheetPolygonNfpPoints = new List<List<IntPoint>>();
          clipper = new ClipperLib.Clipper();

          clipper.AddPaths(combinedNfp, ClipperLib.PolyType.ptClip, true);

          clipper.AddPaths(clipperSheetNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

          if (!clipper.Execute(ClipperLib.ClipType.ctDifference, differenceWithSheetPolygonNfpPoints, ClipperLib.PolyFillType.pftEvenOdd, ClipperLib.PolyFillType.pftNonZero))
          {
            continue;
          }

          if (differenceWithSheetPolygonNfpPoints == null || differenceWithSheetPolygonNfpPoints.Count == 0)
          {
            continue;
          }

          List<NFP> differenceWithSheetPolygonNfp = new List<NFP>();
          for (int j = 0; j < differenceWithSheetPolygonNfpPoints.Count; j++)
          {
            // back to normal scale
            differenceWithSheetPolygonNfp.Add(Background.ToNestCoordinates(differenceWithSheetPolygonNfpPoints[j].ToArray(), config.ClipperScale));
          }

          var finalNfp = differenceWithSheetPolygonNfp;

          // choose placement that results in the smallest bounding box/hull etc
          // todo: generalize gravity direction
          /*var minwidth = null;
          var minarea = null;
          var minx = null;
          var miny = null;
          var nf, area, shiftvector;*/
          minwidth = null;
          minarea = null;
          double? minx = null;
          double? miny = null;
          NFP nf;
          double area;
          PlacementItem shiftvector = null;

          NFP allpoints = new NFP();
          for (int m = 0; m < placed.Count; m++)
          {
            for (int n = 0; n < placed[m].Length; n++)
            {
              allpoints.AddPoint(
                  new SvgPoint(
                   placed[m][n].x + placements[m].x, placed[m][n].y + placements[m].y));
            }
          }

          PolygonBounds allbounds = null;
          PolygonBounds partbounds = null;
          if (config.PlacementType == PlacementTypeEnum.Gravity || config.PlacementType == PlacementTypeEnum.BoundingBox)
          {
            allbounds = GeometryUtil.getPolygonBounds(allpoints);

            NFP partpoints = new NFP();
            for (int m = 0; m < part.Length; m++)
            {
              partpoints.AddPoint(new SvgPoint(part[m].x, part[m].y));
            }

            partbounds = GeometryUtil.getPolygonBounds(partpoints);
          }
          else
          {
            allpoints = GetHull(allpoints.Points);
          }

          for (int j = 0; j < finalNfp.Count; j++)
          {
            nf = finalNfp[j];

            // console.log('evalnf',nf.length);
            for (int k = 0; k < nf.Length; k++)
            {
              shiftvector = new PlacementItem()
              {
                id = part.Id,
                x = nf[k].x - part[0].x,
                y = nf[k].y - part[0].y,
                source = part.Source,
                rotation = part.Rotation,
              };

              PolygonBounds rectbounds = null;
              if (config.PlacementType == PlacementTypeEnum.Gravity || config.PlacementType == PlacementTypeEnum.BoundingBox)
              {
                NFP poly = new NFP();
                poly.AddPoint(new SvgPoint(allbounds.x, allbounds.y));
                poly.AddPoint(new SvgPoint(allbounds.x + allbounds.width, allbounds.y));
                poly.AddPoint(new SvgPoint(allbounds.x + allbounds.width, allbounds.y + allbounds.height));
                poly.AddPoint(new SvgPoint(allbounds.x, allbounds.y + allbounds.height));

                poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + shiftvector.y));
                poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + shiftvector.y));
                poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));
                poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));

                rectbounds = GeometryUtil.getPolygonBounds(poly);

                // weigh width more, to help compress in direction of gravity
                if (config.PlacementType == PlacementTypeEnum.Gravity)
                {
                  area = (rectbounds.width * 3) + rectbounds.height;
                }
                else
                {
                  area = rectbounds.width * rectbounds.height;
                }
              }
              else
              {
                // must be convex hull
                var localpoints = allpoints.Clone();

                for (int m = 0; m < part.Length; m++)
                {
                  localpoints.AddPoint(new SvgPoint(part[m].x + shiftvector.x, part[m].y + shiftvector.y));
                }

                area = Math.Abs(GeometryUtil.polygonArea(GetHull(localpoints.Points)));
                shiftvector.hull = GetHull(localpoints.Points);
                shiftvector.hullsheet = GetHull(sheet.Points);
              }

              // console.timeEnd('evalbounds');
              // console.time('evalmerge');
              MergedResult merged = null;
              if (config.MergeLines)
              {
                throw new NotImplementedException();

                // if lines can be merged, subtract savings from area calculation
                var shiftedpart = shiftPolygon(part, shiftvector);
                List<NFP> shiftedplaced = new List<NFP>();

                for (int m = 0; m < placed.Count; m++)
                {
                  shiftedplaced.Add(shiftPolygon(placed[m], placements[m]));
                }

                // don't check small lines, cut off at about 1/2 in
                double minlength = 0.5 * config.Scale;
                merged = mergedLength(shiftedplaced.ToArray(), shiftedpart, minlength, 0.1 * config.CurveTolerance);
                area -= merged.totalLength * config.TimeRatio;
              }

              // console.timeEnd('evalmerge');
              if (
      minarea == null ||
      area < minarea ||
      (GeometryUtil._almostEqual(minarea, area) && (minx == null || shiftvector.x < minx)) ||
      (GeometryUtil._almostEqual(minarea, area) && (minx != null && GeometryUtil._almostEqual(shiftvector.x, minx) && shiftvector.y < miny)))
              {
                minarea = area;

                minwidth = rectbounds != null ? rectbounds.width : 0;
                position = shiftvector;
                if (minx == null || shiftvector.x < minx)
                {
                  minx = shiftvector.x;
                }

                if (miny == null || shiftvector.y < miny)
                {
                  miny = shiftvector.y;
                }

                if (config.MergeLines)
                {
                  position.mergedLength = merged.totalLength;
                  position.mergedSegments = merged.segments;
                }
              }
            }
          }

          if (position != null)
          {
            placed.Add(part);
            totalPlaced++;
            placements.Add(position);
            if (position.mergedLength != null)
            {
              totalMerged += position.mergedLength.Value;
            }
          }

          // send placement progress signal
          var placednum = placed.Count;
          for (int j = 0; j < allplacements.Count; j++)
          {
            placednum += allplacements[j].sheetplacements.Count;
          }

          // console.log(placednum, totalnum);
          // ipcRenderer.send('background-progress', { index: nestindex, progress: 0.5 + 0.5 * (placednum / totalnum)});

          // console.timeEnd('placement');
        }

        // if(minwidth){
        if (!minwidth.HasValue)
        {
          fitness = double.NaN;
        }
        else
        {
          fitness += (minwidth.Value / sheetarea) + minarea.Value;
        }

        // }
        for (int i = 0; i < placed.Count; i++)
        {
          var index = Array.IndexOf(parts, placed[i]);
          if (index >= 0)
          {
            parts = parts.Splice(index, 1);
          }
        }

        if (placements != null && placements.Count > 0)
        {
          allplacements.Add(new SheetPlacementItem()
          {
            sheetId = sheet.Id,
            sheetSource = sheet.Source,
            sheetplacements = placements,
          });

          // fitness += Add the unused space on each sheet to fitness; we want to use as much as possible within rectangle bounds, not just rectanglebounds
          // fitness += placements.Sum(o=> o.hullsheet.Area

          // allplacements.Add({ sheet: sheet.source, sheetid: sheet.id, sheetplacements: placements});
        }
        else
        {
          break; // something went wrong
        }
      }

      // there were parts that couldn't be placed
      // scale this value high - we really want to get all the parts in, even at the cost of opening new sheets
      for (int noPlaceIdx = 0; noPlaceIdx < parts.Count(); noPlaceIdx++)
      {
        fitness += 100000000 * (Math.Abs(GeometryUtil.polygonArea(parts[noPlaceIdx])) / totalsheetarea);
      }

      // send finish progerss signal
      // ipcRenderer.send('background-progress', { index: nestindex, progress: -1});
      return new SheetPlacement()
      {
        placements = new[] { allplacements.ToList() },
        fitness = fitness,

        // paths = paths,
        area = sheetarea,
        mergedLength = totalMerged,
      };

      // return { placements: allplacements, fitness: fitness, area: sheetarea, mergedLength: totalMerged };
    }

    private bool CanBePlaced(NFP sheet, NFP part, double clipperScale, out NFP[] sheetNfp)
    {
      sheetNfp = getInnerNfp(sheet, part, 0, clipperScale);
      if (sheetNfp != null && sheetNfp.Count() > 0)
      {
        if (sheetNfp[0].Length == 0)
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

    // jsClipper uses X/Y instead of x/y...
    public DataInfo data;
    private NFP[] parts;

    private int index;

    // run the placement synchronously
    public IWindowUnk window = new windowUnk();

    public Action<SheetPlacement> ResponseAction;

    public long LastPlacePartTime = 0;

    private void SyncPlaceParts()
    {
      // console.log('starting synchronous calculations', Object.keys(window.nfpCache).length);
      // console.log('in sync');
      var c = 0;
      foreach (var key in window.nfpCache)
      {
        c++;
      }

      // console.log('nfp cached:', c);
      Stopwatch sw = Stopwatch.StartNew();
      var placement = PlaceParts(this.data.sheets.ToArray(), this.parts, this.data.config, this.index);
      sw.Stop();
      LastPlacePartTime = sw.ElapsedMilliseconds;

      placement.index = this.data.index;
      this.ResponseAction(placement);
    }

    internal void BackgroundStart(DataInfo data)
    {
      try
      {
        this.data = data;
        var index = data.index;
        var individual = data.individual;

        var parts = individual.placements;
        var rotations = individual.Rotation;
        var ids = data.ids;
        var sources = data.sources;
        var children = data.children;

        for (var i = 0; i < parts.Count; i++)
        {
          parts[i].Rotation = rotations[i];
          parts[i].Id = ids[i];
          parts[i].Source = sources[i];
          if (!data.config.Simplify)
          {
            parts[i].Children = children[i];
          }
        }

        for (int i = 0; i < data.sheets.Count; i++)
        {
          data.sheets[i].Id = data.sheetids[i];
          data.sheets[i].Source = data.sheetsources[i];
          data.sheets[i].Children = data.sheetchildren[i];
        }

        // preprocess
        List<NfpPair> pairs = new List<NfpPair>();

        if (Background.UseParallel)
        {
          object lobj = new object();
          Parallel.For(0, parts.Count, i =>
          {
            {
              var B = parts[i];
              for (var j = 0; j < i; j++)
              {
                var A = parts[j];
                var key = new NfpPair()
                {
                  A = A,
                  B = B,
                  ARotation = A.Rotation,
                  BRotation = B.Rotation,
                  Asource = A.Source,
                  Bsource = B.Source,
                };
                var doc = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation);
                lock (lobj)
                {
                  if (!this.InPairs(key, pairs.ToArray()) && !window.Has(doc))
                  {
                    pairs.Add(key);
                  }
                }
              }
            }
          });
        }
        else
        {
          for (var i = 0; i < parts.Count; i++)
          {
            var B = parts[i];
            for (var j = 0; j < i; j++)
            {
              var A = parts[j];
              var key = new NfpPair()
              {
                A = A,
                B = B,
                ARotation = A.Rotation,
                BRotation = B.Rotation,
                Asource = A.Source,
                Bsource = B.Source,
              };
              var doc = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation);
              if (!this.InPairs(key, pairs.ToArray()) && !window.Has(doc))
              {
                pairs.Add(key);
              }
            }
          }
        }

        // console.log('pairs: ', pairs.length);
        // console.time('Total');
        this.parts = parts.ToArray();
        if (pairs.Count > 0)
        {
          var ret1 = this.PmapDeepNest(pairs);
          this.ThenDeepNest(ret1, parts);
        }
        else
        {
          this.SyncPlaceParts();
        }
      }
      catch (DllNotFoundException)
      {
        throw;
      }
      catch (BadImageFormatException)
      {
        throw;
      }
      catch (SEHException)
      {
        throw;
      }
    }

    public NFP GetPart(int source, List<NFP> parts)
    {
      for (var k = 0; k < parts.Count; k++)
      {
        if (parts[k].Source == source)
        {
          return parts[k];
        }
      }

      return null;
    }

    public void ThenIterate(NfpPair processed, List<NFP> parts)
    {
      // returned data only contains outer nfp, we have to account for any holes separately in the synchronous portion
      // this is because the c++ addon which can process interior nfps cannot run in the worker thread
      var A = this.GetPart(processed.Asource, parts);
      var B = this.GetPart(processed.Bsource, parts);

      List<NFP> Achildren = new List<NFP>();

      if (A.Children != null)
      {
        for (int j = 0; j < A.Children.Count; j++)
        {
          Achildren.Add(A.Children[j].Rotate(processed.ARotation));
        }
      }

      if (Achildren.Count > 0)
      {
        var Brotated = B.Rotate(processed.BRotation);
        var bbounds = GeometryUtil.getPolygonBounds(Brotated);
        List<NFP> cnfp = new List<NFP>();

        for (int j = 0; j < Achildren.Count; j++)
        {
          var cbounds = GeometryUtil.getPolygonBounds(Achildren[j]);
          if (cbounds.width > bbounds.width && cbounds.height > bbounds.height)
          {
            var n = getInnerNfp(Achildren[j], Brotated, 1, this.data.config.ClipperScale);
            if (n != null && n.Count() > 0)
            {
              cnfp.AddRange(n);
            }
          }
        }

        processed.nfp.Children = cnfp;
      }

      DbCacheKey doc = new DbCacheKey(processed.Asource, processed.Bsource, processed.ARotation, processed.BRotation, new[] { processed.nfp });

      /*var doc = {
              A: processed[i].Asource,
              B: processed[i].Bsource,
              Arotation: processed[i].Arotation,
              Brotation: processed[i].Brotation,
              nfp: processed[i].nfp

          };*/
      window.Insert(doc);
    }

    private void ThenDeepNest(NfpPair[] processed, List<NFP> parts)
    {
      int cnt = 0;
      if (UseParallel)
      {
        Parallel.For(0, processed.Count(), (i) =>
        {
          this.ThenIterate(processed[i], parts);
        });
      }
      else
      {
        for (var i = 0; i < processed.Count(); i++)
        {
          this.ThenIterate(processed[i], parts);
        }
      }

      // console.timeEnd('Total');
      // console.log('before sync');
      this.SyncPlaceParts();
    }

    private bool InPairs(NfpPair key, NfpPair[] p)
    {
      for (var i = 0; i < p.Length; i++)
      {
        if (p[i].Asource == key.Asource && p[i].Bsource == key.Bsource && p[i].ARotation == key.ARotation && p[i].BRotation == key.BRotation)
        {
          return true;
        }
      }

      return false;
    }

    public static bool UseParallel = false;

    private NfpPair[] PmapDeepNest(List<NfpPair> pairs)
    {
      NfpPair[] ret = new NfpPair[pairs.Count()];
      if (UseParallel)
      {
        Parallel.For(0, pairs.Count, (i) =>
        {
          ret[i] = this.Process(pairs[i]);
        });
      }
      else
      {
        for (int i = 0; i < pairs.Count; i++)
        {
          var item = pairs[i];
          ret[i] = this.Process(item);
        }
      }

      return ret.ToArray();
    }

    private NfpPair Process(NfpPair pair)
    {
      var A = pair.A.Rotate(pair.ARotation);
      var B = pair.B.Rotate(pair.BRotation);

      ///////////////////
      var Ac = _Clipper.ScaleUpPaths(A.Points, 10000000);

      var Bc = _Clipper.ScaleUpPaths(B.Points, 10000000);
      for (var i = 0; i < Bc.Length; i++)
      {
        Bc[i].X *= -1;
        Bc[i].Y *= -1;
      }

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(Ac), new List<IntPoint>(Bc), true);
      NFP clipperNfp = null;

      double? largestArea = null;
      for (int i = 0; i < solution.Count(); i++)
      {
        var n = ToNestCoordinates(solution[i].ToArray(), 10000000);
        var sarea = -GeometryUtil.polygonArea(n);
        if (largestArea == null || largestArea < sarea)
        {
          clipperNfp = n;
          largestArea = sarea;
        }
      }

      for (var i = 0; i < clipperNfp.Length; i++)
      {
        clipperNfp[i].x += B[0].x;
        clipperNfp[i].y += B[0].y;
      }

      // return new SvgNestPort.NFP[] { new SvgNestPort.NFP() { Points = clipperNfp.Points } };

      //////////////

      pair.A = null;
      pair.B = null;
      pair.nfp = clipperNfp;
      return pair;
    }

    public static NFP ToNestCoordinates(IntPoint[] polygon, double scale)
    {
      var clone = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count(); i++)
      {
        clone.Add(new SvgPoint(
             polygon[i].X / scale,
             polygon[i].Y / scale));
      }

      return new NFP(clone);
    }

    public static NFP GetHull(SvgPoint[] polygon)
    {
      // convert to hulljs format
      /*var hull = new ConvexHullGrahamScan();
      for(var i=0; i<polygon.length; i++){
          hull.addPoint(polygon[i].x, polygon[i].y);
      }

      return hull.getHull();*/
      double[][] points = new double[polygon.Length][];
      for (var i = 0; i < polygon.Length; i++)
      {
        points[i] = new double[] { polygon[i].x, polygon[i].y };
      }

      var hullpoints = D3.polygonHull(points);

      if (hullpoints == null)
      {
        return new NFP(polygon);
      }

      NFP hull = new NFP();
      for (int i = 0; i < hullpoints.Count(); i++)
      {
        hull.AddPoint(new SvgPoint(hullpoints[i][0], hullpoints[i][1]));
      }

      return hull;
    }

    // returns clipper nfp. Remember that clipper nfp are a list of polygons, not a tree!
    public static IntPoint[][] NfpToClipperCoordinates(NFP nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

      // children first
      if (nfp.Children != null && nfp.Children.Count > 0)
      {
        for (var j = 0; j < nfp.Children.Count; j++)
        {
          if (GeometryUtil.polygonArea(nfp.Children[j]) < 0)
          {
            nfp.Children[j].reverse();
          }

          // var childNfp = SvgNest.toClipperCoordinates(nfp.Children[j]);
          var childNfp = _Clipper.ScaleUpPaths(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.polygonArea(nfp) > 0)
      {
        nfp.reverse();
      }

      // var outerNfp = SvgNest.toClipperCoordinates(nfp);

      // clipper js defines holes based on orientation
      var outerNfp = _Clipper.ScaleUpPaths(nfp.Points, clipperScale);

      // var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);
      clipperNfp.Add(outerNfp);

      // var area = Math.abs(ClipperLib.Clipper.Area(cleaned));
      return clipperNfp.ToArray();
    }

    // inner nfps can be an array of nfps, outer nfps are always singular
    public static IntPoint[][] InnerNfpToClipperCoordinates(NFP[] nfp, double clipperScale)
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

    private static object lockobj = new object();

    private NFP GetOuterNfp(NFP A, NFP B, int type, bool inside = false) // todo:?inside def?
    {
      NFP[] nfp = null;

      var key = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation);

      var doc = window.Find(key);
      if (doc != null)
      {
        return doc.First();
      }

      // not found in cache
      if (inside || (A.Children != null && A.Children.Count > 0))
      {
        lock (lockobj)
        {
          nfp = Process2(A, B, type);
        }
      }
      else
      {
        var ac = _Clipper.ScaleUpPaths(A.Points, 10000000);

        var bc = _Clipper.ScaleUpPaths(B.Points, 10000000);
        for (var i = 0; i < bc.Length; i++)
        {
          bc[i].X *= -1;
          bc[i].Y *= -1;
        }

        var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<IntPoint>(bc), true);
        NFP clipperNfp = null;

        double? largestArea = null;
        for (int i = 0; i < solution.Count(); i++)
        {
          var n = Background.ToNestCoordinates(solution[i].ToArray(), 10000000);
          var sarea = GeometryUtil.polygonArea(n);
          if (largestArea == null || largestArea > sarea)
          {
            clipperNfp = n;
            largestArea = sarea;
          }
        }

        for (var i = 0; i < clipperNfp.Length; i++)
        {
          clipperNfp[i].x += B[0].x;
          clipperNfp[i].y += B[0].y;
        }

        nfp = new NFP[] { new NFP(clipperNfp.Points) };
      }

      if (nfp == null || nfp.Length == 0)
      {
        // console.log('holy shit', nfp, A, B, JSON.stringify(A), JSON.stringify(B));
        return null;
      }

      NFP nfps = nfp.First();
      /*
      nfp = nfp.pop();
      */
      if (nfps == null || nfps.Length == 0)
      {
        return null;
      }

      /*
      if (!nfp || nfp.length == 0)
      {
          return null;
      }
      */
      if (!inside && A.Source != null && B.Source != null)
      {
        var doc2 = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation, nfp);
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

  public class ClipCacheItem
  {
    public int index;
    public IntPoint[][] nfpp;
  }

  public class dbCache : IDbCache
  {
    public dbCache(IWindowUnk w)
    {
      window = w;
    }

    public bool Has(DbCacheKey dbCacheKey)
    {
      lock (lockobj)
      {
        if (window.nfpCache.ContainsKey(dbCacheKey.Key))
        {
          return true;
        }

        return false;
      }
    }

    public IWindowUnk window;

    public static volatile object lockobj = new object();

    public void Insert(DbCacheKey obj, bool inner = false)
    {
      //if (window.performance.memory.totalJSHeapSize < 0.8 * window.performance.memory.jsHeapSizeLimit)
      {
        lock (lockobj)
        {
          if (!window.nfpCache.ContainsKey(obj.Key))
          {
            window.nfpCache.Add(obj.Key, Background.cloneNfp(obj.nfp, inner).ToList());
          }
          else
          {
            throw new Exception("trouble .cache already has such key");
            //   window.nfpCache[key] = Background.cloneNfp(new[] { obj.nfp }, inner).ToList();
          }
        }
        //console.log('cached: ',window.cache[key].poly);
        //console.log('using', window.performance.memory.totalJSHeapSize/window.performance.memory.jsHeapSizeLimit);
      }
    }

    public NFP[] Find(DbCacheKey obj, bool inner = false)
    {
      lock (lockobj)
      {
        //var key = "A" + obj.A + "B" + obj.B + "Arot" + (int)Math.Round(obj.ARotation) + "Brot" + (int)Math.Round((obj.BRotation));

        //console.log('key: ', key);
        if (window.nfpCache.ContainsKey(obj.Key))
        {
          return Background.cloneNfp(window.nfpCache[obj.Key].ToArray(), inner);
        }

        return null;
      }
    }
  }

  public class windowUnk : IWindowUnk
  {
    public windowUnk()
    {
      db = new dbCache(this);
    }

    public Dictionary<string, List<NFP>> nfpCache { get; } = new Dictionary<string, List<NFP>>();

    private IDbCache db { get; }

    public NFP[] Find(DbCacheKey obj, bool inner = false)
    {
      return this.db.Find(obj, inner);
    }

    public bool Has(DbCacheKey dbCacheKey)
    {
      return this.db.Has(dbCacheKey);
    }

    public void Insert(DbCacheKey obj, bool inner = false)
    {
      this.db.Insert(obj, inner);
    }
  }
}
