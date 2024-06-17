namespace DeepNestLib
{
  public class ArbitrarySheet : Sheet
  {
    internal void Build(INfp nfp)
    {
      this.ReplacePoints(nfp.Points);
    }
  }
}
