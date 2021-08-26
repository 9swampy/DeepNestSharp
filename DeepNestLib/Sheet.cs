namespace DeepNestLib
{
  using System.Text.Json;

  public class Sheet : NFP, ISheet
  {
    public Sheet()
    {
    }

    public Sheet(ISheet sheet, WithChildren withChildren)
      : base(sheet, withChildren)
    {
      Width = sheet.Width;
      Height = sheet.Height;
    }

    public Sheet(INfp nfp, double width, double height, WithChildren withChildren)
      : base(nfp, withChildren)
    {
      Width = width;
      Height = height;
    }

    public Sheet(INfp nfp, WithChildren withChildren)
      : this(nfp, nfp.WidthCalculated, nfp.HeightCalculated, withChildren)
    {
    }

    public double Width { get; set; }

    public double Height { get; set; }

    /// <summary>
    /// Creates a new <see cref="Sheet"/> from the json supplied.
    /// </summary>
    /// <param name="json">Serialised representation of the Sheet to create.</param>
    /// <returns>New <see cref="Sheet"/>.</returns>
    public static new Sheet FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      return JsonSerializer.Deserialize<Sheet>(json, options);
    }

    public static Sheet NewSheet(int nameSuffix, int w = 3000, int h = 1500)
    {
      var tt = new RectangleSheet();
      tt.Name = "rectSheet" + nameSuffix;
      tt.Height = h;
      tt.Width = w;
      tt.Rebuild();

      return tt;
    }

    public override string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.WriteIndented = true;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
