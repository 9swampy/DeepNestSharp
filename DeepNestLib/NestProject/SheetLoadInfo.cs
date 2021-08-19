namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib;
  using DeepNestLib.IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
  {
    [Obsolete("Only use from ConfigSheetLoadInfo to bypass the constructor - could just pass the config in but if we do it would be less transparent; no other reason to keep it long term is there?")]
    protected SheetLoadInfo()
    {
    }

    public SheetLoadInfo(ISvgNestConfig config)
      : this(config.SheetWidth, config.SheetHeight, config.SheetQuantity)
    { }

    [JsonConstructor]
    public SheetLoadInfo(int width, int height, int quantity)
    {
      this.Width = width;
      this.Height = height;
      this.Quantity = quantity;
    }

    public virtual int Width { get; set; }

    public virtual int Height { get; set; }

    public virtual int Quantity { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}