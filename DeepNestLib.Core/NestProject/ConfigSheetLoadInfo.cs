namespace DeepNestLib.NestProject
{
  using System.Text.Json;
  using DeepNestLib;

  public sealed class ConfigSheetLoadInfo : SheetLoadInfo
  {
    private readonly ISvgNestConfig config;

    public ConfigSheetLoadInfo(ISvgNestConfig config)
#pragma warning disable CS0618 // Type or member is obsolete
      : base()
#pragma warning restore CS0618 // Type or member is obsolete
    {
      this.config = config;
    }

    public override double Width
    {
      get { return this.config.SheetWidth; }
      set { this.config.SheetWidth = value; }
    }

    public override double Height
    {
      get { return this.config.SheetHeight; }
      set { this.config.SheetHeight = value; }
    }

    public override int Quantity
    {
      get { return this.config.SheetQuantity; }
      set { this.config.SheetQuantity = value; }
    }

    public override string ToJson(bool writeIndented = false)
    {
      JsonSerializerOptions options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}