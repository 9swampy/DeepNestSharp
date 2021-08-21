namespace DeepNestLib.NestProject
{
  using System.Text.Json;
  using DeepNestLib;

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

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}