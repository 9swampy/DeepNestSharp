namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib;
  using DeepNestLib.IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
  {
    public SheetLoadInfo(ISvgNestConfig config)
      : this(config.SheetWidth, config.SheetHeight, config.SheetQuantity)
    {
    }

    [JsonConstructor]
    public SheetLoadInfo(double width, double height, int quantity)
    {
      this.SheetType = SheetTypeEnum.Rectangle;
      this.Path = null;
      this.Width = width;
      this.Height = height;
      this.Quantity = quantity;
    }

    public SheetLoadInfo(string path, int quantity)
    {
      this.SheetType = SheetTypeEnum.Arbitrary;
      this.Path = path;
      this.Width = 0;
      this.Height = 0;
      this.Quantity = quantity;
    }

    [Obsolete("Only use from ConfigSheetLoadInfo to bypass the constructor - could just pass the config in but if we do it would be less transparent; no other reason to keep it long term is there?")]
    protected SheetLoadInfo()
    {
    }

    public virtual double Height { get; set; }

    public virtual string Path { get; set; } // added later than width/height/quantity: does it also need to be virtual?

    public virtual int Quantity { get; set; }

    public virtual SheetTypeEnum SheetType { get; set; } // added later than width/height/quantity: does it also need to be virtual?

    public virtual double Width { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
