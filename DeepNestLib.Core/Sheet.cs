namespace DeepNestLib
{
  using System.Text.Json;

  public class Sheet : NoFitPolygon, ISheet
  {
    public Sheet()
    {
    }

    public Sheet(ISheet sheet, WithChildren withChildren)
      : base(sheet, withChildren)
    {
    }

    public Sheet(INfp nfp, WithChildren withChildren)
      : base(nfp, withChildren)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Sheet"/> from the json supplied.
    /// </summary>
    /// <param name="json">Serialised representation of the Sheet to create.</param>
    /// <returns>New <see cref="Sheet"/>.</returns>
    public static new Sheet FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      var result = JsonSerializer.Deserialize<Sheet>(json, options);
      return result;
    }

    public static Sheet NewSheet(int nameSuffix, double w = 3000, double h = 1500)
    {
      var tt = new RectangleSheet();
      tt.Name = "rectSheet" + nameSuffix;
      tt.Build(w, h);
      return tt;
    }

    public static Sheet NewSheet(int nameSuffix, INfp nfp)
    {
      INfp shiftedNfp = nfp.ShiftToOrigin();
      var tt = new ArbitrarySheet();
      tt.Name = "arbSheet" + nameSuffix;
      tt.Build(shiftedNfp);
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
