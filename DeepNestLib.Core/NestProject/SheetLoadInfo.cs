namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib;
  using DeepNestLib.IO;
  using Light.GuardClauses;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
  {
    public SheetLoadInfo(ISvgNestConfig config)
      : this(config.SheetWidth, config.SheetHeight, config.SheetQuantity)
    {
    }

    [JsonConstructor]
    public SheetLoadInfo(string path, double width, double height, int quantity)
    {
      // There used to be a "this" in front of each of these. Were they needed? Genuinely don't know.
      if (path.IsNullOrEmpty())
      {
        this.SheetType = SheetTypeEnum.Rectangle;
        this.Path = null;
        this.Width = width;
        this.Height = height;
        this.Quantity = quantity;
      }
      else
      {
        this.SheetType = SheetTypeEnum.Arbitrary;
        this.Path = path;
        this.Width = 0;
        this.Height = 0;
        this.Quantity = quantity;
      }
    }

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

    public string Path { get; set; }

    public virtual int Quantity { get; set; }

    public SheetTypeEnum SheetType { get; }

    public virtual double Width { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      JsonSerializerOptions options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
