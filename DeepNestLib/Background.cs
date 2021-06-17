namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using ClipperLib;
    using Minkowski;

    public class Background
    {
        public static bool EnableCaches = true;

        public static NFP shiftPolygon(NFP p, PlacementItem shift)
        {
            NFP shifted = new NFP();
            for (var i = 0; i < p.Length; i++)
            {
                shifted.AddPoint(new SvgPoint(p[i].x + shift.x, p[i].y + shift.y) { exact = p[i].exact });
            }

            if (p.children != null /*&& p.children.Count*/)
            {
                shifted.children = new List<NFP>();
                for (int i = 0; i < p.children.Count; i++)
                {
                    shifted.children.Add(shiftPolygon(p.children[i], shift));
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

            // if(B.children && B.children.length > 0){
            // var child = mergedLength(B.children, p, minlength, tolerance);
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
                return new[] { clone(nfp.First()) };
            }

            throw new NotImplementedException();

            // inner nfp is actually an array of nfps
            List<NFP> newnfp = new List<NFP>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                // newnfp.push(clone(nfp[i]));
            }

            // return newnfp;
        }

        public static NFP clone(NFP nfp)
        {
            NFP newnfp = new NFP();
            newnfp.Source = nfp.Source;
            for (var i = 0; i < nfp.Length; i++)
            {
                newnfp.AddPoint(new SvgPoint(nfp[i].x, nfp[i].y));
            }

            if (nfp.children != null && nfp.children.Count > 0)
            {
                newnfp.children = new List<NFP>();
                for (int i = 0; i < nfp.children.Count; i++)
                {
                    var child = nfp.children[i];
                    NFP newchild = new NFP();
                    for (var j = 0; j < child.Length; j++)
                    {
                        newchild.AddPoint(new SvgPoint(child[j].x, child[j].y));
                    }

                    newnfp.children.Add(newchild);
                }
            }

            return newnfp;
        }

        public static int callCounter = 0;

        public static Dictionary<string, NFP[]> cacheProcess = new Dictionary<string, NFP[]>();

        public static NFP[] Process2(NFP A, NFP B, int type)
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

            foreach (var item in A.children)
            {
                foreach (var pitem in item.Points)
                {
                    hdat.Add(pitem.x);
                    hdat.Add(pitem.y);
                }
            }

            var aa = dic2["A"];
            var bb = dic2["B"];
            var arr1 = A.children.Select(z => z.Points.Count() * 2).ToArray();

            MinkowskiWrapper.setData(aa.Count, aa.ToArray(), A.children.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
            MinkowskiWrapper.calculateNFP();

            callCounter++;

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
            ret.Points = new SvgPoint[] { };
            foreach (var item in Apts)
            {
                ret.AddPoint(new SvgPoint(item.X, item.Y));
            }

            foreach (var item in holesout)
            {
                ret.children = new List<NFP>();
                ret.children.Add(new NFP());
                ret.children.Last().Points = new SvgPoint[] { };
                foreach (var hitem in item)
                {
                    ret.children.Last().AddPoint(new SvgPoint(hitem.X, hitem.Y));
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

            var frame = new NFP();
            frame.Push(new SvgPoint(bounds.x, bounds.y));
            frame.Push(new SvgPoint(bounds.x + bounds.width, bounds.y));
            frame.Push(new SvgPoint(bounds.x + bounds.width, bounds.y + bounds.height));
            frame.Push(new SvgPoint(bounds.x, bounds.y + bounds.height));

            frame.children = new List<NFP>() { (NFP)A };
            frame.Source = A.Source;
            frame.Rotation = 0;

            return frame;
        }

        public static NFP[] getInnerNfp(NFP A, NFP B, int type, SvgNestConfig config)
        {
            if (A.Source != null && B.Source != null)
            {
                var key = new DbCacheKey()
                {
                    A = A.Source.Value,
                    B = B.Source.Value,
                    ARotation = 0,
                    BRotation = B.Rotation,

                    // Inside =true??
                };

                // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
                var res = window.db.Find(key, true);
                if (res != null)
                {
                    return res;
                }
            }

            var frame = getFrame(A);

            var nfp = GetOuterNfp(frame, B, type, true);

            if (nfp == null || nfp.children == null || nfp.children.Count == 0)
            {
                return null;
            }

            List<NFP> holes = new List<NFP>();
            if (A.children != null && A.children.Count > 0)
            {
                for (var i = 0; i < A.children.Count; i++)
                {
                    var hnfp = GetOuterNfp(A.children[i], B, 1);
                    if (hnfp != null)
                    {
                        holes.Add(hnfp);
                    }
                }
            }

            if (holes.Count == 0)
            {
                return nfp.children.ToArray();
            }

            var clipperNfp = InnerNfpToClipperCoordinates(nfp.children.ToArray(), config);
            var clipperHoles = InnerNfpToClipperCoordinates(holes.ToArray(), config);

            List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
            var clipper = new ClipperLib.Clipper();

            clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptClip, true);
            clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

            if (!clipper.Execute(ClipperLib.ClipType.ctDifference, finalNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
            {
                return nfp.children.ToArray();
            }

            if (finalNfp.Count == 0)
            {
                return null;
            }

            List<NFP> f = new List<NFP>();
            for (var i = 0; i < finalNfp.Count; i++)
            {
                f.Add(ToNestCoordinates(finalNfp[i].ToArray(), config.ClipperScale));
            }

            if (A.Source != null && B.Source != null)
            {
                // insert into db
                // console.log('inserting inner: ', A.source, B.source, B.rotation, f);
                var doc = new DbCacheKey()
                {
                    A = A.Source.Value,
                    B = B.Source.Value,
                    ARotation = 0,
                    BRotation = B.Rotation,
                    nfp = f.ToArray(),
                };
                window.db.Insert(doc, true);
            }

            return f.ToArray();
        }

        public static NFP rotatePolygon(NFP polygon, float degrees)
        {
            NFP rotated = new NFP();

            var angle = degrees * Math.PI / 180;
            List<SvgPoint> pp = new List<SvgPoint>();
            for (var i = 0; i < polygon.Length; i++)
            {
                var x = polygon[i].x;
                var y = polygon[i].y;
                var x1 = (x * Math.Cos(angle)) - (y * Math.Sin(angle));
                var y1 = (x * Math.Sin(angle)) + (y * Math.Cos(angle));

                pp.Add(new SvgPoint(x1, y1));
            }

            rotated.Points = pp.ToArray();

            if (polygon.children != null && polygon.children.Count > 0)
            {
                rotated.children = new List<NFP>();
                for (var j = 0; j < polygon.children.Count; j++)
                {
                    rotated.children.Add(rotatePolygon(polygon.children[j], degrees));
                }
            }

            return rotated;
        }

        public static SheetPlacement PlaceParts(NFP[] sheets, NFP[] parts, SvgNestConfig config, int nestindex)
        {
            if (sheets == null || sheets.Count() == 0)
            {
                return null;
            }

            int i, j, k, m, n;
            double totalsheetarea = 0;

            NFP part = null;

            // total length of merged lines
            double totalMerged = 0;

            // rotate paths by given rotation
            var rotated = new List<NFP>();
            for (i = 0; i < parts.Length; i++)
            {
                var r = rotatePolygon(parts[i], parts[i].Rotation);
                r.Rotation = parts[i].Rotation;
                r.Source = parts[i].Source;
                r.Id = parts[i].Id;
                rotated.Add(r);
            }

            parts = rotated.ToArray();

            List<SheetPlacementItem> allplacements = new List<SheetPlacementItem>();

            double fitness = 0;

            string key;
            NFP nfp;
            double sheetarea = -1;
            int totalPlaced = 0;
            int totalParts = parts.Count();

            while (parts.Length > 0)
            {
                List<NFP> placed = new List<NFP>();

                List<PlacementItem> placements = new List<PlacementItem>();

                // open a new sheet
                var sheet = sheets.First();
                sheets = sheets.Skip(1).ToArray();
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
                PlacementItem position = null;
                double? minarea = null;
                for (i = 0; i < parts.Length; i++)
                {
                    float prog = 0.66f + (0.34f * (totalPlaced / (float)totalParts));
                    DisplayProgress(prog);

                    part = parts[i];

                    // inner NFP
                    NFP[] sheetNfp = null;

                    // try all possible rotations until it fits
                    // (only do this for the first part of each sheet, to ensure that all parts that can be placed are, even if we have to to open a lot of sheets)
                    for (j = 0; j < (360f / config.Rotations); j++)
                    {
                        sheetNfp = getInnerNfp(sheet, part, 0, config);

                        if (sheetNfp != null && sheetNfp.Count() > 0)
                        {
                            if (sheetNfp[0].Length == 0)
                            {
                                throw new ArgumentException();
                            }
                            else
                            {
                                break;
                            }
                        }

                        var r = rotatePolygon(part, 360f / config.Rotations);
                        r.Rotation = part.Rotation + (360f / config.Rotations);
                        r.Source = part.Source;
                        r.id = part.id;

                        // rotation is not in-place
                        part = r;
                        parts[i] = r;

                        if (part.Rotation > 360f)
                        {
                            part.Rotation = part.Rotation % 360f;
                        }
                    }

                    // part unplaceable, skip
                    if (sheetNfp == null || sheetNfp.Count() == 0)
                    {
                        continue;
                    }

                    position = null;

                    if (placed.Count == 0)
                    {
                        // first placement, put it on the top left corner
                        for (j = 0; j < sheetNfp.Count(); j++)
                        {
                            for (k = 0; k < sheetNfp[j].Length; k++)
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
                                        id = part.id,
                                        rotation = part.Rotation,
                                        source = part.Source.Value,
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

                    clipperSheetNfp = InnerNfpToClipperCoordinates(sheetNfp, config);

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

                    for (j = startindex; j < placed.Count; j++)
                    {
                        nfp = GetOuterNfp(placed[j], part, 0);

                        // minkowski difference failed. very rare but could happen
                        if (nfp == null)
                        {
                            error = true;
                            break;
                        }

                        // shift to placed location
                        for (m = 0; m < nfp.Length; m++)
                        {
                            nfp[m].x += placements[j].x;
                            nfp[m].y += placements[j].y;
                        }

                        if (nfp.children != null && nfp.children.Count > 0)
                        {
                            for (n = 0; n < nfp.children.Count; n++)
                            {
                                for (var o = 0; o < nfp.children[n].Length; o++)
                                {
                                    nfp.children[n][o].x += placements[j].x;
                                    nfp.children[n][o].y += placements[j].y;
                                }
                            }
                        }

                        var clipperNfp = NfpToClipperCoordinates(nfp, config);

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
                    List<List<IntPoint>> _finalNfp = new List<List<IntPoint>>();
                    clipper = new ClipperLib.Clipper();

                    clipper.AddPaths(combinedNfp, ClipperLib.PolyType.ptClip, true);

                    clipper.AddPaths(clipperSheetNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

                    if (!clipper.Execute(ClipperLib.ClipType.ctDifference, _finalNfp, ClipperLib.PolyFillType.pftEvenOdd, ClipperLib.PolyFillType.pftNonZero))
                    {
                        continue;
                    }

                    if (_finalNfp == null || _finalNfp.Count == 0)
                    {
                        continue;
                    }

                    List<NFP> f = new List<NFP>();
                    for (j = 0; j < _finalNfp.Count; j++)
                    {
                        // back to normal scale
                        f.Add(Background.ToNestCoordinates(_finalNfp[j].ToArray(), config.ClipperScale));
                    }

                    var finalNfp = f;

                    // finalNfp = f;

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
                    for (m = 0; m < placed.Count; m++)
                    {
                        for (n = 0; n < placed[m].Length; n++)
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
                        for (m = 0; m < part.Length; m++)
                        {
                            partpoints.AddPoint(new SvgPoint(part[m].x, part[m].y));
                        }

                        partbounds = GeometryUtil.getPolygonBounds(partpoints);
                    }
                    else
                    {
                        allpoints = GetHull(allpoints);
                    }

                    for (j = 0; j < finalNfp.Count; j++)
                    {
                        nf = finalNfp[j];

                        // console.log('evalnf',nf.length);
                        for (k = 0; k < nf.Length; k++)
                        {
                            shiftvector = new PlacementItem()
                            {
                                id = part.id,
                                x = nf[k].x - part[0].x,
                                y = nf[k].y - part[0].y,
                                source = part.Source.Value,
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
                                /*
                             [
                                // allbounds points
                            { x: allbounds.x, y: allbounds.y},
                            { x: allbounds.x + allbounds.width, y: allbounds.y},
                            { x: allbounds.x + allbounds.width, y: allbounds.y + allbounds.height},
                            { x: allbounds.x, y: allbounds.y + allbounds.height},*/

                                poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + shiftvector.y));
                                poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + shiftvector.y));
                                poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));
                                poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));
                                /*
                                 [

                                // part points
                                { x: partbounds.x + shiftvector.x, y: partbounds.y + shiftvector.y},
                                { x: partbounds.x + partbounds.width + shiftvector.x, y: partbounds.y + shiftvector.y},
                                { x: partbounds.x + partbounds.width + shiftvector.x, y: partbounds.y + partbounds.height + shiftvector.y},
                                { x: partbounds.x + shiftvector.x, y: partbounds.y + partbounds.height + shiftvector.y}
                            ]*/
                                rectbounds = GeometryUtil.getPolygonBounds(poly);

                                // weigh width more, to help compress in direction of gravity
                                if (config.PlacementType == PlacementTypeEnum.Gravity)
                                {
                                    area = (rectbounds.width * 2) + rectbounds.height;
                                }
                                else
                                {
                                    area = rectbounds.width * rectbounds.height;
                                }
                            }
                            else
                            {
                                // must be convex hull
                                var localpoints = clone(allpoints);

                                for (m = 0; m < part.Length; m++)
                                {
                                    localpoints.AddPoint(new SvgPoint(part[m].x + shiftvector.x, part[m].y + shiftvector.y));
                                }

                                area = Math.Abs(GeometryUtil.polygonArea(GetHull(localpoints)));
                                shiftvector.hull = GetHull(localpoints);
                                shiftvector.hullsheet = GetHull(sheet);
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

                                for (m = 0; m < placed.Count; m++)
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
                    for (j = 0; j < allplacements.Count; j++)
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
                for (i = 0; i < placed.Count; i++)
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
                        sheetId = sheet.id,
                        sheetSource = sheet.Source.Value,
                        sheetplacements = placements,
                    });

                    // allplacements.Add({ sheet: sheet.source, sheetid: sheet.id, sheetplacements: placements});
                }
                else
                {
                    break; // something went wrong
                }

                if (sheets.Count() == 0)
                {
                    break;
                }
            }

            // there were parts that couldn't be placed
            // scale this value high - we really want to get all the parts in, even at the cost of opening new sheets
            for (i = 0; i < parts.Count(); i++)
            {
                fitness += 100000000 * (Math.Abs(GeometryUtil.polygonArea(parts[i])) / totalsheetarea);
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

        // jsClipper uses X/Y instead of x/y...
        public DataInfo data;
        private NFP[] parts;

        private int index;

        // run the placement synchronously
        public static windowUnk window = new windowUnk();

        public Action<SheetPlacement> ResponseAction;

        public static long LastPlacePartTime = 0;

        public void Sync()
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

            // ipcRenderer.send('background-response', placement);
        }

        public void BackgroundStart(DataInfo data)
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
                parts[i].id = ids[i];
                parts[i].Source = sources[i];
                if (!data.config.Simplify)
                {
                    parts[i].children = children[i];
                }
            }

            for (int i = 0; i < data.sheets.Count; i++)
            {
                data.sheets[i].id = data.sheetids[i];
                data.sheets[i].Source = data.sheetsources[i];
                data.sheets[i].children = data.sheetchildren[i];
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
                                Asource = A.Source.Value,
                                Bsource = B.Source.Value,
                            };
                            var doc = new DbCacheKey()
                            {
                                A = A.Source.Value,
                                B = B.Source.Value,

                                ARotation = A.Rotation,
                                BRotation = B.Rotation,
                            };
                            lock (lobj)
                            {
                                if (!this.InPairs(key, pairs.ToArray()) && !window.db.Has(doc))
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
                            Asource = A.Source.Value,
                            Bsource = B.Source.Value,
                        };
                        var doc = new DbCacheKey()
                        {
                            A = A.Source.Value,
                            B = B.Source.Value,

                            ARotation = A.Rotation,
                            BRotation = B.Rotation,
                        };
                        if (!this.InPairs(key, pairs.ToArray()) && !window.db.Has(doc))
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
                this.Sync();
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

            if (A.children != null)
            {
                for (int j = 0; j < A.children.Count; j++)
                {
                    Achildren.Add(rotatePolygon(A.children[j], processed.ARotation));
                }
            }

            if (Achildren.Count > 0)
            {
                var Brotated = rotatePolygon(B, processed.BRotation);
                var bbounds = GeometryUtil.getPolygonBounds(Brotated);
                List<NFP> cnfp = new List<NFP>();

                for (int j = 0; j < Achildren.Count; j++)
                {
                    var cbounds = GeometryUtil.getPolygonBounds(Achildren[j]);
                    if (cbounds.width > bbounds.width && cbounds.height > bbounds.height)
                    {
                        var n = getInnerNfp(Achildren[j], Brotated, 1, this.data.config);
                        if (n != null && n.Count() > 0)
                        {
                            cnfp.AddRange(n);
                        }
                    }
                }

                processed.nfp.children = cnfp;
            }

            DbCacheKey doc = new DbCacheKey()
            {
                A = processed.Asource,
                B = processed.Bsource,
                ARotation = processed.ARotation,
                BRotation = processed.BRotation,
                nfp = new[] { processed.nfp },
            };

            /*var doc = {
                    A: processed[i].Asource,
                    B: processed[i].Bsource,
                    Arotation: processed[i].Arotation,
                    Brotation: processed[i].Brotation,
                    nfp: processed[i].nfp

                };*/
            window.db.Insert(doc);
        }

        public static Action<float> displayProgress;

        public static void DisplayProgress(float p)
        {
            if (displayProgress != null)
            {
                displayProgress(p);
            }
        }

        public void ThenDeepNest(NfpPair[] processed, List<NFP> parts)
        {
            int cnt = 0;
            if (UseParallel)
            {
                Parallel.For(0, processed.Count(), (i) =>
                {
                    float progress = 0.33f + (0.33f * (cnt / (float)processed.Count()));
                    cnt++;
                    DisplayProgress(progress);
                    this.ThenIterate(processed[i], parts);
                });
            }
            else
            {
                for (var i = 0; i < processed.Count(); i++)
                {
                    float progress = 0.33f + (0.33f * (cnt / (float)processed.Count()));
                    cnt++;
                    DisplayProgress(progress);
                    this.ThenIterate(processed[i], parts);
                }
            }

            // console.timeEnd('Total');
            // console.log('before sync');
            this.Sync();
        }

        public bool InPairs(NfpPair key, NfpPair[] p)
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

        public NfpPair[] PmapDeepNest(List<NfpPair> pairs)
        {
            NfpPair[] ret = new NfpPair[pairs.Count()];
            int cnt = 0;
            if (UseParallel)
            {
                Parallel.For(0, pairs.Count, (i) =>
                {
                    ret[i] = this.Process(pairs[i]);
                    float progress = 0.33f * (cnt / (float)pairs.Count);
                    cnt++;
                    DisplayProgress(progress);
                });
            }
            else
            {
                for (int i = 0; i < pairs.Count; i++)
                {
                    var item = pairs[i];
                    ret[i] = this.Process(item);
                    float progress = 0.33f * (cnt / (float)pairs.Count);
                    cnt++;
                    DisplayProgress(progress);
                }
            }

            return ret.ToArray();
        }

        public NfpPair Process(NfpPair pair)
        {
            var A = rotatePolygon(pair.A, pair.ARotation);
            var B = rotatePolygon(pair.B, pair.BRotation);

            ///////////////////
            var Ac = _Clipper.ScaleUpPaths(A, 10000000);

            var Bc = _Clipper.ScaleUpPaths(B, 10000000);
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

            return new NFP() { Points = clone.ToArray() };
        }

        public static NFP GetHull(NFP polygon)
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
                return polygon;
            }

            NFP hull = new NFP();
            for (int i = 0; i < hullpoints.Count(); i++)
            {
                hull.AddPoint(new SvgPoint(hullpoints[i][0], hullpoints[i][1]));
            }

            return hull;
        }

        // returns clipper nfp. Remember that clipper nfp are a list of polygons, not a tree!
        public static IntPoint[][] NfpToClipperCoordinates(NFP nfp, SvgNestConfig config)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

            // children first
            if (nfp.children != null && nfp.children.Count > 0)
            {
                for (var j = 0; j < nfp.children.Count; j++)
                {
                    if (GeometryUtil.polygonArea(nfp.children[j]) < 0)
                    {
                        nfp.children[j].reverse();
                    }

                    // var childNfp = SvgNest.toClipperCoordinates(nfp.children[j]);
                    var childNfp = _Clipper.ScaleUpPaths(nfp.children[j], config.ClipperScale);
                    clipperNfp.Add(childNfp);
                }
            }

            if (GeometryUtil.polygonArea(nfp) > 0)
            {
                nfp.reverse();
            }

            // var outerNfp = SvgNest.toClipperCoordinates(nfp);

            // clipper js defines holes based on orientation
            var outerNfp = _Clipper.ScaleUpPaths(nfp, config.ClipperScale);

            // var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);
            clipperNfp.Add(outerNfp);

            // var area = Math.abs(ClipperLib.Clipper.Area(cleaned));
            return clipperNfp.ToArray();
        }

        // inner nfps can be an array of nfps, outer nfps are always singular
        public static IntPoint[][] InnerNfpToClipperCoordinates(NFP[] nfp, SvgNestConfig config)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                var clip = NfpToClipperCoordinates(nfp[i], config);
                clipperNfp.AddRange(clip);

                // clipperNfp = clipperNfp.Concat(new[] { clip }).ToList();
            }

            return clipperNfp.ToArray();
        }

        private static object lockobj = new object();

        public static NFP GetOuterNfp(NFP A, NFP B, int type, bool inside = false) // todo:?inside def?
        {
            NFP[] nfp = null;

            var key = new DbCacheKey()
            {
                A = A.Source,
                B = B.Source,
                ARotation = A.Rotation,
                BRotation = B.Rotation,

                // Type = type
            };

            var doc = window.db.Find(key);
            if (doc != null)
            {
                return doc.First();
            }

            /*

            // try the file cache if the calculation will take a long time
            var doc = window.db.find({ A: A.source, B: B.source, Arotation: A.rotation, Brotation: B.rotation });

            if (doc)
            {
                return doc;
            }*/

            // not found in cache
            if (inside || (A.children != null && A.children.Count > 0))
            {
                lock (lockobj)
                {
                    nfp = Process2(A, B, type);
                }
            }
            else
            {
                var ac = _Clipper.ScaleUpPaths(A, 10000000);

                var bc = _Clipper.ScaleUpPaths(B, 10000000);
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

                nfp = new NFP[] { new NFP() { Points = clipperNfp.Points } };
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
                var doc2 = new DbCacheKey()
                {
                    A = A.Source.Value,
                    B = B.Source.Value,
                    ARotation = A.Rotation,
                    BRotation = B.Rotation,
                    nfp = nfp,
                };
                window.db.Insert(doc2);
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
}
