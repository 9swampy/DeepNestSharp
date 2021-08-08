namespace DeepNestLib.NestProject
{
  using System;
using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class DetailLoadInfoJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IDetailLoadInfo));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new DetailLoadInfoJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class DetailLoadInfoJsonConverterInner : JsonConverter<IDetailLoadInfo>
    {
      public override IDetailLoadInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<DetailLoadInfo>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IDetailLoadInfo value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<DetailLoadInfo>(writer, (DetailLoadInfo)value, options);
      }
    }
  }

  public class WrappableListDetailLoadInfoJsonConverter : JsonConverterFactory
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IList<IDetailLoadInfo, DetailLoadInfo>));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new DetailLoadInfoJsonConverterInner();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class DetailLoadInfoJsonConverterInner : JsonConverter<IList<IDetailLoadInfo, DetailLoadInfo>>
    {
      public override IList<IDetailLoadInfo, DetailLoadInfo> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<WrappableList<IDetailLoadInfo, DetailLoadInfo>>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IList<IDetailLoadInfo, DetailLoadInfo> value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<WrappableList<IDetailLoadInfo, DetailLoadInfo>>(writer, (WrappableList<IDetailLoadInfo, DetailLoadInfo>)value, options);
      }
    }
  }

  public class WrappableListJsonConverter<TCommonInterface, TConcrete> : JsonConverterFactory
    where TConcrete : class, TCommonInterface
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IList<TCommonInterface, TConcrete>));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new JsonConverterInner<TCommonInterface, TConcrete>();
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class JsonConverterInner<TInnerCommonInterface, TInnerConcrete> : JsonConverter<IList<TInnerCommonInterface, TInnerConcrete>>
      where TInnerConcrete : class, TInnerCommonInterface
    {
      public override IList<TInnerCommonInterface, TInnerConcrete> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        return JsonSerializer.Deserialize<WrappableList<TInnerCommonInterface, TInnerConcrete>>(ref reader, options);
      }

      public override void Write(Utf8JsonWriter writer, IList<TInnerCommonInterface, TInnerConcrete> value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize<WrappableList<TInnerCommonInterface, TInnerConcrete>>(writer, (WrappableList< TInnerCommonInterface, TInnerConcrete>)value, options);
      }
    }
  }
}