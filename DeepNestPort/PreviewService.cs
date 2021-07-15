using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepNestLib;
using DeepNestSharp;

namespace DeepNestPort
{
  public class PreviewService
  {
    public readonly static PreviewService Default = new PreviewService();

    public void RedrawPreview(DrawingContext ctx2, object previewObject)
    {
      if (ctx2 == null) return;
      if (previewObject == null) return;

      ctx2.Update();
      ctx2.Clear(Color.White);

      //ctx2.gr.DrawLine(Pens.Blue, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(100, 0));
      //ctx2.gr.DrawLine(Pens.Red, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(0, 100));
      ctx2.Reset();

      if (previewObject != null)
      {
        RectangleF bnd;
        if (previewObject is RawDetail || previewObject is NFP)
        {
          if (previewObject is RawDetail raw)
          {
            ctx2.Draw(raw, Pens.Black, Brushes.LightBlue);
            if (SvgNest.Config.DrawSimplification)
            {
              AddApproximation(ctx2, raw);
            }

            bnd = raw.BoundingBox();
          }
          else
          {
            var g = ctx2.Draw(previewObject as NFP, Pens.Black, Brushes.LightBlue);
            bnd = g.GetBounds();
          }

          var cap = $"{bnd.Width:N2} x {bnd.Height:N2}";
          ctx2.DrawLabel(cap, Brushes.Black, Color.LightGreen, 5, 5);
        }
      }

      ctx2.Setup();
    }

    /// <summary>
    /// Display the bounds that will be used by the nesting algorithym.
    /// </summary>
    /// <param name="ctx">Drawing context upon which to draw.</param>
    /// <param name="raw">The part to approximate.</param>
    private void AddApproximation(DrawingContext ctx, RawDetail raw)
    {
      NFP part = raw.ToNfp();
      var simplification = SvgNest.simplifyFunction(part, false);
      ctx.Draw(simplification, Pens.Red);
      var pointsChange = $"{part.Points.Length} => {simplification.Points.Length} points";
      ctx.DrawLabel(pointsChange, Brushes.Black, Color.Orange, 5, (int)(10 + ctx.GetLabelHeight()));
    }
  }
}
