namespace DeepNestSharp.Domain
{
  using System;
  using System.Collections.ObjectModel;
  using System.Linq;
  using DeepNestLib;
  using DeepNestSharp.Ui.ViewModels;

  public static class MaxMinXYExtensions
  {
    public static double Extremum(this IZoomPreviewDrawingContext drawingContext, MinMax minMax, XY xy)
    {
      Func<IMinMaxXY, double> accessor;
      if (minMax == MinMax.Min)
      {
        if (xy == XY.X)
        {
          accessor = x => x.MinX;
        }
        else
        {
          accessor = y => y.MinY;
        }

        return drawingContext.MinimumXY(accessor);
      }
      else if (xy == XY.X)
      {
        accessor = x => x.MaxX;
      }
      else
      {
        accessor = y => y.MaxY;
      }

      return drawingContext.MaximumXY(accessor);
    }

    private static double MaximumXY(this IZoomPreviewDrawingContext drawingContext, Func<IMinMaxXY, double> accessor)
    {
      if (drawingContext.Count == 0)
      {
        return 1;
      }

      return drawingContext.Max(o =>
      {
        if (o is IMinMaxXY item)
        {
          return accessor(item);
        }

        throw new NotSupportedException($"{o.GetType().Name} could not be handled.");
      });
    }

    private static double MinimumXY(this IZoomPreviewDrawingContext drawingContext, Func<IMinMaxXY, double> accessor)
    {
      if (drawingContext.Count == 0)
      {
        return 1;
      }

      return drawingContext.Min(o =>
      {
        if (o is IMinMaxXY item)
        {
          return accessor(item);
        }

        throw new NotSupportedException($"{o.GetType().Name} could not be handled.");
      });
    }
  }
}
