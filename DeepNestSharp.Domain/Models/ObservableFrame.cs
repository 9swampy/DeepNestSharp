namespace DeepNestSharp.Domain.Models
{
  using DeepNestLib;

  public class ObservableFrame : ObservableNfp
  {
    public ObservableFrame(INfp nfp)
      : base(new NoFitPolygon(nfp, WithChildren.Excluded))
    {
    }
  }
}
