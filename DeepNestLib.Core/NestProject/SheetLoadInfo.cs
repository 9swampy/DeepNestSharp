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
        SheetType = SheetTypeEnum.Rectangle;
        Path = null;
        Width = width;
        Height = height;
        Quantity = quantity;
      }
      else
      {
        SheetType = SheetTypeEnum.Arbitrary;
        Path = path;
        Width = 0;
        Height = 0;
        Quantity = quantity;
      }
    }

    public SheetLoadInfo(double width, double height, int quantity)
    {
      // There used to be a "this" in front of each of these. Were they needed? Genuinely don't know.
      SheetType = SheetTypeEnum.Rectangle;
      Path = null;
      Width = width;
      Height = height;
      Quantity = quantity;
    }

    public SheetLoadInfo(string path, int quantity)
    {
      // There used to be a "this" in front of each of these. Were they needed? Genuinely don't know.
      SheetType = SheetTypeEnum.Arbitrary;
      Path = path;
      Width = 0;
      Height = 0;
      Quantity = quantity;
    }

    [Obsolete("Only use from ConfigSheetLoadInfo to bypass the constructor - could just pass the config in but if we do it would be less transparent; no other reason to keep it long term is there?")]
    protected SheetLoadInfo()
    {
    }

    public virtual double Height { get; set; }

    public virtual string Path { get; set; } // added later than width/height/quantity: does it also need to be virtual?

    public virtual int Quantity { get; set; }

    public virtual SheetTypeEnum SheetType { get; } // added later than width/height/quantity: does it also need to be virtual?

    public virtual double Width { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
