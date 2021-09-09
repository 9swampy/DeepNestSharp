namespace DeepNestLib.PairMap
{
  public class NfpPair
  {
    public INfp A { get; internal set; }

    public INfp B { get; internal set; }

    /// <summary>
    /// Gets the Nfp product of the pair.
    /// The parent is the outer Nfp.
    /// The children are the inner Nfps fitting one inside the holes in the other.
    /// </summary>
    public INfp Nfp { get; internal set; }

    public double ARotation { get; internal set; }

    public double BRotation { get; internal set; }

    public int Asource { get; internal set; }

    public int Bsource { get; internal set; }
  }
}
