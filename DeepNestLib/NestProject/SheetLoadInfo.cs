namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib;
  using DeepNestLib.IO;

#pragma warning disable CS0618 // Type or member is obsolete
  public sealed class ConfigSheetLoadInfo : SheetLoadInfo
  {
    private readonly ISvgNestConfig config;

    public ConfigSheetLoadInfo(ISvgNestConfig config)
      : base()
    {
      this.config = config;
    }

    public override int Width
    {
      get { return config.SheetWidth; }
      set { config.SheetWidth = value; }
    }

    public override int Height
    {
      get { return config.SheetHeight; }
      set { config.SheetHeight = value; }
    }

    public override int Quantity
    {
      get { return config.SheetQuantity; }
      set { config.SheetQuantity = value; }
    }

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
#pragma warning restore CS0618 // Type or member is obsolete

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

    public override string ToJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}