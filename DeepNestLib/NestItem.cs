namespace DeepNestLib
{
  public class NestItem<TNfp>
    where TNfp : INfp
  {
    public TNfp Polygon { get; set; }

    public int Quantity { get; set; }

    public bool IsSheet => Polygon is ISheet;
  }
}
