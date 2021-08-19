namespace DeepNestLib
{
  using DeepNestLib.Placement;
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;
  using System.Text.Json;
  using System.Linq;

  public class MinkowskiDictionary : Dictionary<MinkowskiKey, INfp>
  {
    public MinkowskiDictionary()
      : base(new MinkowskiKeyEqualityComparer())
    {
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

  public class MinkowskiDictionaryJsonConverter : JsonConverter<MinkowskiDictionary>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == typeof(MinkowskiDictionary);
    }

    public override MinkowskiDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var kvpList = JsonSerializer.Deserialize<List<KeyValuePair<MinkowskiKey, INfp>>>(ref reader, options);
      var result = new MinkowskiDictionary();
      foreach (var kvp in kvpList)
      {
        result.Add(kvp.Key, kvp.Value);
      }

      return result;
    }

    public override void Write(Utf8JsonWriter writer, MinkowskiDictionary dictionary, JsonSerializerOptions options)
    {
      var kvpList = dictionary.ToList();
      JsonSerializer.Serialize<List<KeyValuePair<MinkowskiKey, INfp>>>(writer, kvpList, options);
    }
  }
}
