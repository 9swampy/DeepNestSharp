namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;

  public class MinkowskiDictionary : Dictionary<MinkowskiKey, INfp>
  {
    public MinkowskiDictionary()
      : base(new MinkowskiKeyEqualityComparer())
    {
    }

    public void Add(MinkowskiKey key, INfp item, bool roundTripTest = false)
    {
      System.Diagnostics.Debug.Print($"{(roundTripTest ? "Add" : "Test add")} {key.GetHashCode()}");
      base.Add(key, new NoFitPolygon(item, WithChildren.Included));
      if (roundTripTest)
      {
        try
        {
          var json = this.ToJson();
          var deserialized = MinkowskiDictionary.FromJson(json);
        }
        catch (Exception ex)
        {
          // NOP
          // throw;
        }
      }
    }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      return System.Text.Json.JsonSerializer.Serialize(this, options);
    }

    public static MinkowskiDictionary FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      return System.Text.Json.JsonSerializer.Deserialize<MinkowskiDictionary>(json, options);
    }
  }
}
