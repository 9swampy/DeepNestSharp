namespace DeepNestLib
{
  public class NfpPair
  {
    public INfp A { get; internal set; }

    public INfp B { get; internal set; }

    public INfp Nfp { get; internal set; }

    public double ARotation { get; internal set; }

    public double BRotation { get; internal set; }

    public int Asource { get; internal set; }

    public int Bsource { get; internal set; }
  }
}
