namespace DeepNestLib
{
  using System.Text.Json;

  public class Sheet : NFP, ISheet
  {
    public Sheet()
    {
    }

    public Sheet(ISheet sheet)
      : base(sheet)
    {
      Width = sheet.Width;
      Height = sheet.Height;
    }

    public Sheet(INfp nfp, double width, double height)
      : base(nfp)
    {
      Width = width;
      Height = height;
    }

    public Sheet(INfp nfp)
      : this(nfp, nfp.WidthCalculated, nfp.HeightCalculated)
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
      return JsonSerializer.Deserialize<Sheet>(json);
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
      return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
  }
}
