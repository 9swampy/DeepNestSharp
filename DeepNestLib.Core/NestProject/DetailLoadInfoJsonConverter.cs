namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.IO;

  public class DetailLoadInfoJsonConverter : JsonConverterFactory
  {
    private readonly IRelativePathHelper relativePathHelper;

    public DetailLoadInfoJsonConverter(IRelativePathHelper relativePathHelper)
    {
      this.relativePathHelper = relativePathHelper;
    }

    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IDetailLoadInfo));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new DetailLoadInfoJsonConverterInner(relativePathHelper);
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class DetailLoadInfoJsonConverterInner : JsonConverter<IDetailLoadInfo>
    {
      private readonly IRelativePathHelper relativePathHelper;

      public DetailLoadInfoJsonConverterInner(IRelativePathHelper relativePathHelper)
      {
        this.relativePathHelper = relativePathHelper;
      }

      public override IDetailLoadInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var detailLoadInfo = JsonSerializer.Deserialize<DetailLoadInfo>(ref reader, options);
        detailLoadInfo.Path = relativePathHelper.ConvertToFullPath(detailLoadInfo.Path);
        return detailLoadInfo;
      }

      public override void Write(Utf8JsonWriter writer, IDetailLoadInfo value, JsonSerializerOptions options)
      {
        var clone = value.Clone();
        clone.Path = relativePathHelper.ConvertToRelativePath(value.Path);
        JsonSerializer.Serialize(writer, (DetailLoadInfo)clone, options);
      }
    }
  }
}