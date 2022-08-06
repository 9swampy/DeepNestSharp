namespace DeepNestLib.Placement
{
  using DeepNestLib;
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
          var json = ToJson();
          var deserialized = FromJson(json);
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
      return JsonSerializer.Serialize(this, options);
    }

    public static MinkowskiDictionary FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      return JsonSerializer.Deserialize<MinkowskiDictionary>(json, options);
    }
  }
}
