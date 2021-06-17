namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClipperLib;

    public class SvgNest
    {
        public SvgNest()
        {
        }

        public class InrangeItem
        {
            public SvgPoint point;
            public double distance;
        }

        public static SvgPoint getTarget(SvgPoint o, NFP simple, double tol)
        {
            List<InrangeItem> inrange = new List<InrangeItem>();

            // find closest points within 2 offset deltas
            for (var j = 0; j < simple.Length; j++)
            {
                var s = simple[j];
                var d2 = ((o.x - s.x) * (o.x - s.x)) + ((o.y - s.y) * (o.y - s.y));
                if (d2 < tol * tol)
                {
                    inrange.Add(new InrangeItem() { point = s, distance = d2 });
                }
            }

            SvgPoint target = null;
            if (inrange.Count > 0)
            {
                var filtered = inrange.Where((p) =>
                {
                    return p.point.exact;
                }).ToList();

                // use exact points when available, normal points when not
                inrange = filtered.Count > 0 ? filtered : inrange;

                inrange = inrange.OrderBy((b) =>
  {
      return b.distance;
  }).ToList();

                target = inrange[0].point;
            }
            else
            {
                double? mind = null;
                for (int j = 0; j < simple.Length; j++)
                {
                    var s = simple[j];
                    var d2 = ((o.x - s.x) * (o.x - s.x)) + ((o.y - s.y) * (o.y - s.y));
                    if (mind == null || d2 < mind)
                    {
                        target = s;
                        mind = d2;
                    }
                }
            }

            return target;
        }

        public static SvgNestConfig Config = new SvgNestConfig();

        public static NFP clone(NFP p)
        {
            var newp = new NFP();
            for (var i = 0; i < p.Length; i++)
            {
                newp.AddPoint(new SvgPoint(

                     p[i].x,
                     p[i].y));
            }

            return newp;
        }

        public static bool pointInPolygon(SvgPoint point, NFP polygon)
        {
            // scaling is deliberately coarse to filter out points that lie *on* the polygon
            var p = svgToClipper2(polygon, 1000);
            var pt = new ClipperLib.IntPoint(1000 * point.x, 1000 * point.y);

            return ClipperLib.Clipper.PointInPolygon(pt, p.ToList()) > 0;
        }

        // returns true if any complex vertices fall outside the simple polygon
        public static bool exterior(NFP simple, NFP complex, bool inside)
        {
            // find all protruding vertices
            for (var i = 0; i < complex.Length; i++)
            {
                var v = complex[i];
                if (!inside && !pointInPolygon(v, simple) && find(v, simple) == null)
                {
                    return true;
                }

                if (inside && pointInPolygon(v, simple) && find(v, simple) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static NFP simplifyFunction(NFP polygon, bool inside)
        {
            var tolerance = 4 * Config.CurveTolerance;

            // give special treatment to line segments above this length (squared)
            var fixedTolerance = 40 * Config.CurveTolerance * 40 * Config.CurveTolerance;
            int i, j, k;

            if (Config.Simplify)
            {
                /*
                // use convex hull
                var hull = new ConvexHullGrahamScan();
                for(var i=0; i<polygon.length; i++){
                    hull.addPoint(polygon[i].x, polygon[i].y);
                }

                return hull.getHull();*/
                var hull = Background.GetHull(polygon);
                if (hull != null)
                {
                    return hull;
                }
                else
                {
                    return polygon;
                }
            }

            var cleaned = cleanPolygon2(polygon);
            if (cleaned != null && cleaned.Length > 1)
            {
                polygon = cleaned;
            }
            else
            {
                return polygon;
            }

            // polygon to polyline
            var copy = polygon.slice(0);
            copy.Push(copy[0]);

            // mark all segments greater than ~0.25 in to be kept
            // the PD simplification algo doesn't care about the accuracy of long lines, only the absolute distance of each point
            // we care a great deal
            for (i = 0; i < copy.Length - 1; i++)
            {
                var p1 = copy[i];
                var p2 = copy[i + 1];
                var sqd = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));
                if (sqd > fixedTolerance)
                {
                    p1.marked = true;
                    p2.marked = true;
                }
            }

            var simple = Simplify.simplify(copy, tolerance, true);

            // now a polygon again
            // simple.pop();
            simple.Points = simple.Points.Take(simple.Points.Count() - 1).ToArray();

            // could be dirty again (self intersections and/or coincident points)
            simple = cleanPolygon2(simple);

            // simplification process reduced poly to a line or point
            if (simple == null)
            {
                simple = polygon;
            }

            var offsets = polygonOffsetDeepNest(simple, inside ? -tolerance : tolerance);

            NFP offset = null;
            double offsetArea = 0;
            List<NFP> holes = new List<NFP>();
            for (i = 0; i < offsets.Length; i++)
            {
                var area = GeometryUtil.polygonArea(offsets[i]);
                if (offset == null || area < offsetArea)
                {
                    offset = offsets[i];
                    offsetArea = area;
                }

                if (area > 0)
                {
                    holes.Add(offsets[i]);
                }
            }

            // mark any points that are exact
            for (i = 0; i < simple.Length; i++)
            {
                var seg = new NFP();
                seg.AddPoint(simple[i]);
                seg.AddPoint(simple[i + 1 == simple.Length ? 0 : i + 1]);

                var index1 = find(seg[0], polygon);
                var index2 = find(seg[1], polygon);

                if (index1 + 1 == index2 || index2 + 1 == index1 || (index1 == 0 && index2 == polygon.Length - 1) || (index2 == 0 && index1 == polygon.Length - 1))
                {
                    seg[0].exact = true;
                    seg[1].exact = true;
                }
            }

            var numshells = 4;
            NFP[] shells = new NFP[numshells];

            for (j = 1; j < numshells; j++)
            {
                var delta = j * (tolerance / numshells);
                delta = inside ? -delta : delta;
                var shell = polygonOffsetDeepNest(simple, delta);
                if (shell.Count() > 0)
                {
                    shells[j] = shell.First();
                }
                else
                {
                    // shells[j] = shell;
                }
            }

            if (offset == null)
            {
                return polygon;
            }

            // selective reversal of offset
            for (i = 0; i < offset.Length; i++)
            {
                var o = offset[i];
                var target = getTarget(o, simple, 2 * tolerance);

                // reverse point offset and try to find exterior points
                var test = clone(offset);
                test.Points[i] = new SvgPoint(target.x, target.y);

                if (!exterior(test, polygon, inside))
                {
                    o.x = target.x;
                    o.y = target.y;
                }
                else
                {
                    // a shell is an intermediate offset between simple and offset
                    for (j = 1; j < numshells; j++)
                    {
                        if (shells[j] != null)
                        {
                            var shell = shells[j];
                            var delta = j * (tolerance / numshells);
                            target = getTarget(o, shell, 2 * delta);
                            test = clone(offset);
                            test.Points[i] = new SvgPoint(target.x, target.y);
                            if (!exterior(test, polygon, inside))
                            {
                                o.x = target.x;
                                o.y = target.y;
                                break;
                            }
                        }
                    }
                }
            }

            // straighten long lines
            // a rounded rectangle would still have issues at this point, as the long sides won't line up straight
            var straightened = false;

            for (i = 0; i < offset.Length; i++)
            {
                var p1 = offset[i];
                var p2 = offset[i + 1 == offset.Length ? 0 : i + 1];

                var sqd = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));

                if (sqd < fixedTolerance)
                {
                    continue;
                }

                for (j = 0; j < simple.Length; j++)
                {
                    var s1 = simple[j];
                    var s2 = simple[j + 1 == simple.Length ? 0 : j + 1];

                    var sqds = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));

                    if (sqds < fixedTolerance)
                    {
                        continue;
                    }

                    if ((GeometryUtil._almostEqual(s1.x, s2.x) || GeometryUtil._almostEqual(s1.y, s2.y)) && // we only really care about vertical and horizontal lines
                    GeometryUtil._withinDistance(p1, s1, 2 * tolerance) &&
                    GeometryUtil._withinDistance(p2, s2, 2 * tolerance) &&
                    (!GeometryUtil._withinDistance(p1, s1, Config.CurveTolerance / 1000) ||
                    !GeometryUtil._withinDistance(p2, s2, Config.CurveTolerance / 1000)))
                    {
                        p1.x = s1.x;
                        p1.y = s1.y;
                        p2.x = s2.x;
                        p2.y = s2.y;
                        straightened = true;
                    }
                }
            }

            // if(straightened){
            var Ac = _Clipper.ScaleUpPaths(offset, 10000000);
            var Bc = _Clipper.ScaleUpPaths(polygon, 10000000);

            var combined = new List<List<IntPoint>>();
            var clipper = new ClipperLib.Clipper();

            clipper.AddPath(Ac.ToList(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPath(Bc.ToList(), ClipperLib.PolyType.ptSubject, true);

            // the line straightening may have made the offset smaller than the simplified
            if (clipper.Execute(ClipperLib.ClipType.ctUnion, combined, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
            {
                double? largestArea = null;
                for (i = 0; i < combined.Count; i++)
                {
                    var n = Background.ToNestCoordinates(combined[i].ToArray(), 10000000);
                    var sarea = -GeometryUtil.polygonArea(n);
                    if (largestArea == null || largestArea < sarea)
                    {
                        offset = n;
                        largestArea = sarea;
                    }
                }
            }

            // }
            cleaned = cleanPolygon2(offset);
            if (cleaned != null && cleaned.Length > 1)
            {
                offset = cleaned;
            }

            // mark any points that are exact (for line merge detection)
            for (i = 0; i < offset.Length; i++)
            {
                var seg = new SvgPoint[] { offset[i], offset[i + 1 == offset.Length ? 0 : i + 1] };
                var index1 = find(seg[0], polygon);
                var index2 = find(seg[1], polygon);
                if (index1 == null)
                {
                    index1 = 0;
                }

                if (index2 == null)
                {
                    index2 = 0;
                }

                if (index1 + 1 == index2 || index2 + 1 == index1
                    || (index1 == 0 && index2 == polygon.Length - 1) ||
                    (index2 == 0 && index1 == polygon.Length - 1))
                {
                    seg[0].exact = true;
                    seg[1].exact = true;
                }
            }

            if (!inside && holes != null && holes.Count > 0)
            {
                offset.children = holes;
            }

            return offset;
        }

        public static int? find(SvgPoint v, NFP p)
        {
            for (var i = 0; i < p.Length; i++)
            {
                if (GeometryUtil._withinDistance(v, p[i], Config.CurveTolerance / 1000))
                {
                    return i;
                }
            }

            return null;
        }

        // offset tree recursively
        public static void offsetTree(NFP t, double offset, SvgNestConfig config, bool? inside = null)
        {
            var simple = simplifyFunction(t, (inside == null) ? false : inside.Value);
            var offsetpaths = new NFP[] { simple };
            if (Math.Abs(offset) > 0)
            {
                offsetpaths = polygonOffsetDeepNest(simple, offset);
            }

            if (offsetpaths.Count() > 0)
            {
                List<SvgPoint> rett = new List<SvgPoint>();
                rett.AddRange(offsetpaths[0].Points);
                rett.AddRange(t.Points.Skip(t.Length));
                t.Points = rett.ToArray();

                // replace array items in place

                // Array.prototype.splice.apply(t, [0, t.length].concat(offsetpaths[0]));
            }

            if (simple.children != null && simple.children.Count > 0)
            {
                if (t.children == null)
                {
                    t.children = new List<NFP>();
                }

                for (var i = 0; i < simple.children.Count; i++)
                {
                    t.children.Add(simple.children[i]);
                }
            }

            if (t.children != null && t.children.Count > 0)
            {
                for (var i = 0; i < t.children.Count; i++)
                {
                    offsetTree(t.children[i], -offset, config, (inside == null) ? true : (!inside));
                }
            }
        }

        // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
        // note that this returns an array of polygons
        public static NFP[] polygonOffsetDeepNest(NFP polygon, double offset)
        {
            if (offset == 0 || GeometryUtil._almostEqual(offset, 0))
            {
                return new[] { polygon };
            }

            var p = svgToClipper(polygon).ToList();

            var miterLimit = 4;
            var co = new ClipperLib.ClipperOffset(miterLimit, Config.CurveTolerance * Config.ClipperScale);
            co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

            var newpaths = new List<List<ClipperLib.IntPoint>>();
            co.Execute(ref newpaths, offset * Config.ClipperScale);

            var result = new List<NFP>();
            for (var i = 0; i < newpaths.Count; i++)
            {
                result.Add(clipperToSvg(newpaths[i]));
            }

            return result.ToArray();
        }

        // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
        public static IntPoint[] svgToClipper2(NFP polygon, double? scale = null)
        {
            var d = _Clipper.ScaleUpPaths(polygon, scale == null ? Config.ClipperScale : scale.Value);
            return d.ToArray();
        }

        // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
        public static ClipperLib.IntPoint[] svgToClipper(NFP polygon)
        {
            var d = _Clipper.ScaleUpPaths(polygon, Config.ClipperScale);
            return d.ToArray();

            return polygon.Points.Select(z => new IntPoint((long)z.x, (long)z.y)).ToArray();
        }

        // returns a less complex polygon that satisfies the curve tolerance
        public static NFP cleanPolygon(NFP polygon)
        {
            var p = svgToClipper2(polygon);

            // remove self-intersections and find the biggest polygon that's left
            var simple = ClipperLib.Clipper.SimplifyPolygon(p.ToList(), ClipperLib.PolyFillType.pftNonZero);

            if (simple == null || simple.Count == 0)
            {
                return null;
            }

            var biggest = simple[0];
            var biggestarea = Math.Abs(ClipperLib.Clipper.Area(biggest));
            for (var i = 1; i < simple.Count; i++)
            {
                var area = Math.Abs(ClipperLib.Clipper.Area(simple[i]));
                if (area > biggestarea)
                {
                    biggest = simple[i];
                    biggestarea = area;
                }
            }

            // clean up singularities, coincident points and edges
            var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

            if (clean == null || clean.Count == 0)
            {
                return null;
            }

            return clipperToSvg(clean);
        }

        public static NFP cleanPolygon2(NFP polygon)
        {
            var p = svgToClipper(polygon);

            // remove self-intersections and find the biggest polygon that's left
            var simple = ClipperLib.Clipper.SimplifyPolygon(p.ToList(), ClipperLib.PolyFillType.pftNonZero);

            if (simple == null || simple.Count == 0)
            {
                return null;
            }

            var biggest = simple[0];
            var biggestarea = Math.Abs(ClipperLib.Clipper.Area(biggest));
            for (var i = 1; i < simple.Count; i++)
            {
                var area = Math.Abs(ClipperLib.Clipper.Area(simple[i]));
                if (area > biggestarea)
                {
                    biggest = simple[i];
                    biggestarea = area;
                }
            }

            // clean up singularities, coincident points and edges
            var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

            if (clean == null || clean.Count == 0)
            {
                return null;
            }

            var cleaned = clipperToSvg(clean);

            // remove duplicate endpoints
            var start = cleaned[0];
            var end = cleaned[cleaned.Length - 1];
            if (start == end || (GeometryUtil._almostEqual(start.x, end.x)
                && GeometryUtil._almostEqual(start.y, end.y)))
            {
                cleaned.Points = cleaned.Points.Take(cleaned.Points.Count() - 1).ToArray();
            }

            return cleaned;
        }

        public static NFP clipperToSvg(IList<IntPoint> polygon)
        {
            List<SvgPoint> ret = new List<SvgPoint>();

            for (var i = 0; i < polygon.Count; i++)
            {
                ret.Add(new SvgPoint(polygon[i].X / Config.ClipperScale, polygon[i].Y / Config.ClipperScale));
            }

            return new NFP() { Points = ret.ToArray() };
        }

        public int toTree(PolygonTreeItem[] list, int idstart = 0)
        {
            List<PolygonTreeItem> parents = new List<PolygonTreeItem>();
            int i, j;

            // assign a unique id to each leaf
            // var id = idstart || 0;
            var id = idstart;

            for (i = 0; i < list.Length; i++)
            {
                var p = list[i];

                var ischild = false;
                for (j = 0; j < list.Length; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    if (GeometryUtil.pointInPolygon(p.Polygon.Points[0], list[j].Polygon).Value)
                    {
                        if (list[j].Childs == null)
                        {
                            list[j].Childs = new List<PolygonTreeItem>();
                        }

                        list[j].Childs.Add(p);
                        p.Parent = list[j];
                        ischild = true;
                        break;
                    }
                }

                if (!ischild)
                {
                    parents.Add(p);
                }
            }

            for (i = 0; i < list.Length; i++)
            {
                if (parents.IndexOf(list[i]) < 0)
                {
                    list = list.Skip(i).Take(1).ToArray();
                    i--;
                }
            }

            for (i = 0; i < parents.Count; i++)
            {
                parents[i].Polygon.Id = id;
                id++;
            }

            for (i = 0; i < parents.Count; i++)
            {
                if (parents[i].Childs != null)
                {
                    id = this.toTree(parents[i].Childs.ToArray(), id);
                }
            }

            return id;
        }

        public static NFP cloneTree(NFP tree)
        {
            NFP newtree = new NFP();
            foreach (var t in tree.Points)
            {
                newtree.AddPoint(new SvgPoint(t.x, t.y) { exact = t.exact });
            }

            if (tree.children != null && tree.children.Count > 0)
            {
                newtree.children = new List<NFP>();
                foreach (var c in tree.children)
                {
                    newtree.children.Add(cloneTree(c));
                }
            }

            return newtree;
        }

        public Background background = new Background();

        private PopulationItem individual = null;
        private NFP[] placelist;
        private GeneticAlgorithm ga;

        public List<SheetPlacement> nests = new List<SheetPlacement>();

        public void ResponseProcessor(SheetPlacement payload)
        {
            // console.log('ipc response', payload);
            if (this.ga == null)
            {
                // user might have quit while we're away
                return;
            }

            this.ga.Population[payload.index].processing = null;
            this.ga.Population[payload.index].fitness = payload.fitness;

            // render placement
            if (this.nests.Count == 0 || this.nests[0].fitness > payload.fitness)
            {
                this.nests.Insert(0, payload);

                if (this.nests.Count > Config.PopulationSize)
                {
                    this.nests.RemoveAt(this.nests.Count - 1);
                }

                // if (displayCallback)
                {
                    // displayCallback();
                }
            }
        }

        public void launchWorkers(NestItem[] parts)
        {
            this.background.ResponseAction = this.ResponseProcessor;
            if (this.ga == null)
            {
                List<NFP> adam = new List<NFP>();
                var id = 0;
                for (int i = 0; i < parts.Count(); i++)
                {
                    if (!parts[i].IsSheet)
                    {
                        for (int j = 0; j < parts[i].Quanity; j++)
                        {
                            var poly = cloneTree(parts[i].Polygon); // deep copy
                            poly.id = id; // id is the unique id of all parts that will be nested, including cloned duplicates
                            poly.Source = i; // source is the id of each unique part from the main part list

                            adam.Add(poly);
                            id++;
                        }
                    }
                }

                adam = adam.OrderByDescending(z => Math.Abs(GeometryUtil.polygonArea(z))).ToList();
                /*List<NFP> shuffle = new List<NFP>();
                Random r = new Random(DateTime.Now.Millisecond);
                while (adam.Any())
                {
                    var rr = r.Next(adam.Count);
                    shuffle.Add(adam[rr]);
                    adam.RemoveAt(rr);
                }
                adam = shuffle;*/

                /*#region special case
                var temp = adam[1];
                adam.RemoveAt(1);
                adam.Insert(9, temp);

                #endregion*/
                this.ga = new GeneticAlgorithm(adam.ToArray(), Config);
            }

            this.individual = null;

            // check if current generation is finished
            var finished = true;
            for (int i = 0; i < this.ga.Population.Count; i++)
            {
                if (this.ga.Population[i].fitness == null)
                {
                    finished = false;
                    break;
                }
            }

            if (finished)
            {
                // console.log('new generation!');
                // all individuals have been evaluated, start next generation
                this.ga.Generation();
            }

            var running = this.ga.Population.Where((p) =>
            {
                return p.processing != null;
            }).Count();

            List<NFP> sheets = new List<NFP>();
            List<int> sheetids = new List<int>();
            List<int> sheetsources = new List<int>();
            List<List<NFP>> sheetchildren = new List<List<NFP>>();
            var sid = 0;
            for (int i = 0; i < parts.Count(); i++)
            {
                if (parts[i].IsSheet)
                {
                    var poly = parts[i].Polygon;
                    for (int j = 0; j < parts[i].Quanity; j++)
                    {
                        var cln = cloneTree(poly);
                        cln.id = sid; // id is the unique id of all parts that will be nested, including cloned duplicates
                        cln.Source = poly.Source; // source is the id of each unique part from the main part list

                        sheets.Add(cln);
                        sheetids.Add(sid);
                        sheetsources.Add(i);
                        sheetchildren.Add(poly.children);
                        sid++;
                    }
                }
            }

            for (int i = 0; i < this.ga.Population.Count; i++)
            {
                // if(running < config.threads && !GA.population[i].processing && !GA.population[i].fitness){
                // only one background window now...
                if (running < 1 && this.ga.Population[i].processing == null && this.ga.Population[i].fitness == null)
                {
                    this.ga.Population[i].processing = true;

                    // hash values on arrays don't make it across ipc, store them in an array and reassemble on the other side....
                    List<int> ids = new List<int>();
                    List<int> sources = new List<int>();
                    List<List<NFP>> children = new List<List<NFP>>();

                    for (int j = 0; j < this.ga.Population[i].placements.Count; j++)
                    {
                        var id = this.ga.Population[i].placements[j].id;
                        var source = this.ga.Population[i].placements[j].Source;
                        var child = this.ga.Population[i].placements[j].children;

                        // ids[j] = id;
                        ids.Add(id);

                        // sources[j] = source;
                        sources.Add(source.Value);

                        // children[j] = child;
                        children.Add(child);
                    }

                    DataInfo data = new DataInfo()
                    {
                        index = i,
                        sheets = sheets,
                        sheetids = sheetids.ToArray(),
                        sheetsources = sheetsources.ToArray(),
                        sheetchildren = sheetchildren,
                        individual = this.ga.Population[i],
                        config = Config,
                        ids = ids.ToArray(),
                        sources = sources.ToArray(),
                        children = children,
                    };

                    this.background.BackgroundStart(data);

                    // ipcRenderer.send('background-start', { index: i, sheets: sheets, sheetids: sheetids, sheetsources: sheetsources, sheetchildren: sheetchildren, individual: GA.population[i], config: config, ids: ids, sources: sources, children: children});
                    running++;
                }
            }
        }

        public PolygonTreeItem[] tree;

        public static IntPoint[] toClipperCoordinates(NFP polygon)
        {
            var clone = new List<IntPoint>();
            for (var i = 0; i < polygon.Length; i++)
            {
                clone.Add(
                    new IntPoint(
                     polygon[i].x,
                     polygon[i].y));
            }

            return clone.ToArray();
        }

        public bool useHoles;
        public bool searchEdges;
    }
}
