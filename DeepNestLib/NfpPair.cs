namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;

  public class NfpPair
  {
    public INfp A { get; internal set; }

    public INfp B { get; internal set; }

    public INfp nfp { get; internal set; }

    public double ARotation { get; internal set; }

    public double BRotation { get; internal set; }

    public int Asource { get; internal set; }

    public int Bsource { get; internal set; }
  }
}
