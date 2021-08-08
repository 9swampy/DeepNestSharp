namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using DeepNestLib;
  using DeepNestLib.IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
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

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}