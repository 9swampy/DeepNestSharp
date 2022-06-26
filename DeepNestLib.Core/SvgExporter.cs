namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SvgExporter : ExporterBase
    {
        public override string SaveFileDialogFilter => "Svg files (*.svg)|*.svg";

        protected override async Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets)
        {
            await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("	<svg version=\"1.1\" id=\"svg2\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"   xml:space=\"preserve\">");

                foreach (var item in polygons.Union(sheets))
                {
                    if (!sheets.Contains(item))
                    {
                        if (!item.Fitted)
                        {
                            continue;
                        }
                    }

                    var m = new Matrix();
                    m.Translate((float)item.X, (float)item.Y);
                    m.Rotate((float)item.Rotation);

                    PointF[] pp = item.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
                    m.TransformPoints(pp);
                    var points = pp.Select(z => new SvgPoint(z.X, z.Y)).ToArray();

                    string fill = "lightblue";
                    if (sheets.Contains(item))
                    {
                        fill = "none";
                    }

                    sb.AppendLine($"<path fill=\"{fill}\"  stroke=\"black\" d=\"");
                    for (int i = 0; i < points.Count(); i++)
                    {
                        var p = points[i];
                        string coord = p.X.ToString().Replace(",", ".") + " " + p.Y.ToString().Replace(",", ".");
                        if (i == 0)
                        {
                            sb.Append("M" + coord + " ");
                            continue;
                        }

                        sb.Append("L" + coord + " ");
                    }

                    sb.Append("z ");
                    if (item.Children != null)
                    {
                        foreach (var citem in item.Children)
                        {
                            pp = citem.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
                            m.TransformPoints(pp);
                            points = pp.Select(z => new SvgPoint(z.X, z.Y)).Reverse().ToArray();

                            for (int i = 0; i < points.Count(); i++)
                            {
                                var p = points[i];
                                string coord = p.X.ToString().Replace(",", ".") + " " + p.Y.ToString().Replace(",", ".");
                                if (i == 0)
                                {
                                    sb.Append("M" + coord + " ");
                                    continue;
                                }

                                sb.Append("L" + coord + " ");
                            }

                            sb.Append("z ");
                        }
                    }

                    sb.Append("\"/>");
                }

                sb.AppendLine("</svg>");
                File.WriteAllText(path, sb.ToString());
            }).ConfigureAwait(false);
        }
    }
}
