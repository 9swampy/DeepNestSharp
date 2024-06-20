namespace DeepNestSharp.Domain.Models
{
  using DeepNestLib;

  public class ObservablePoint : ObservableNfp
  {
    public ObservablePoint(INfp nfp)
      : base(FirstPoint(nfp))
    {

    }

    private static INfp FirstPoint(INfp nfp)
    {
      NoFitPolygon result = new NoFitPolygon(nfp, WithChildren.Excluded);
      result.ReplacePoints(new SvgPoint[] { nfp.Points[0] });
      return result;
    }
  }
}
