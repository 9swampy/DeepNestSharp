namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.Linq;
  using DeepNestLib;

  public static class MaxMinXYExtensions
  {
    public static double Extremum(this ObservableCollection<object> drawingContext, MinMax minMax, XY xy)
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

        return MinimumXY(drawingContext, accessor);
      }
      else if (xy == XY.X)
      {
        accessor = x => x.MaxX;
      }
      else
      {
        accessor = y => y.MaxY;
      }

      return MaximumXY(drawingContext, accessor);
    }

    private static double MaximumXY(this ObservableCollection<object> drawingContext, Func<IMinMaxXY, double> accessor)
    {
      return drawingContext.Max(o =>
      {
        if (o is IMinMaxXY item)
        {
          return accessor(item);
        }

        throw new NotSupportedException($"{o.GetType().Name} could not be handled.");
      });
    }

    private static double MinimumXY(this ObservableCollection<object> drawingContext, Func<IMinMaxXY, double> accessor)
    {
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
