namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;
  using System.Text.Json;
  using System.Linq;

  public class MinkowskiDictionaryJsonConverter : JsonConverter<MinkowskiDictionary>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == typeof(MinkowskiDictionary);
    }

    public override MinkowskiDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var kvpList = JsonSerializer.Deserialize<List<KeyValuePair<MinkowskiKey, INfp>>>(ref reader, options);
      var json = JsonSerializer.Serialize<List<KeyValuePair<MinkowskiKey, INfp>>>(kvpList, options);
      var result = new MinkowskiDictionary();
      foreach (var kvp in kvpList)
      {
        // System.Diagnostics.Debug.Print(string.Join(",", kvp.Key.Item7));
        result.Add(kvp.Key, kvp.Value, false);
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
