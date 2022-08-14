namespace DeepNestLib.NestProject
{
  using System;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib.IO;

  public class ProjectInfoJsonConverter : JsonConverterFactory
  {
    private IRelativePathHelper relativePathHelper;

    public ProjectInfoJsonConverter(IRelativePathHelper relativePathHelper)
    {
      this.relativePathHelper = relativePathHelper;
    }

    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableFrom(typeof(IProjectInfo)) || typeToConvert.IsAssignableFrom(typeof(ProjectInfo));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (CanConvert(typeToConvert))
      {
        return new ProjectInfoJsonConverterInner(relativePathHelper);
      }

      throw new ArgumentException($"Cannot convert {nameof(typeToConvert)}.", nameof(typeToConvert));
    }

    public class ProjectInfoJsonConverterInner : JsonConverter<IProjectInfo>
    {
      private IRelativePathHelper relativePathHelper;

      public ProjectInfoJsonConverterInner(IRelativePathHelper relativePathHelper)
      {
        this.relativePathHelper = relativePathHelper;
      }

      public override IProjectInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var newOptions = new JsonSerializerOptions(options);
        var converterToRemove = newOptions.Converters.Where(o => o.CanConvert(typeof(IProjectInfo))).ToList();
        foreach (var converter in converterToRemove)
        {
          newOptions.Converters.Remove(converter);
        }

        var result = JsonSerializer.Deserialize<ProjectInfo>(ref reader, newOptions);
        var json = result.ToJson();
        result = ProjectInfo.FromJson(result.Config, json, relativePathHelper);
        return result;
      }

      public override void Write(Utf8JsonWriter writer, IProjectInfo value, JsonSerializerOptions options)
      {
        JsonSerializer.Serialize(writer, (ProjectInfo)value, options);
      }
    }
  }
}