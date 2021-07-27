namespace DeepNestLib.NestProject
{
  using DeepNestLib;

  public class SheetLoadInfo : ISheetLoadInfo
  {
    public int Width
    {
      get { return SvgNest.Config.SheetWidth; }
      set { SvgNest.Config.SheetWidth = value; }
    }

    public int Height
    {
      get { return SvgNest.Config.SheetHeight; }
      set { SvgNest.Config.SheetHeight = value; }
    }

    public int Quantity
    {
      get { return SvgNest.Config.SheetQuantity; }
      set { SvgNest.Config.SheetQuantity = value; }
    }
  }
}
