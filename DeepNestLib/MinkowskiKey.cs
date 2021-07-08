namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using System.Threading;
  using ClipperLib;
  using Minkowski;

  public class MinkowskiKey : Tuple<int, double[], int, int[], double[], int, double[]>
  {
    public MinkowskiKey(int aLength, double[] aPoints, int aChildrenLength, int[] arr1, double[] hdat, int bLength, double[] bPoints)
      : base(aLength, aPoints, aChildrenLength, arr1, hdat, bLength, bPoints)
    {
    }

    public int ALength => Item1;

    public double[] APoints => Item2;

    public int AChildrenLength => Item3;

    public int[] Arr1 => Item4;

    public double[] Hdat => Item5;

    public int BLength => Item6;

    public double[] BPoints => Item7;
  }
}
