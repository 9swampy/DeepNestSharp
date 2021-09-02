namespace DeepNestLib.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class IListInterfaceConverterFactory : JsonConverterFactory
  {
    public IListInterfaceConverterFactory(Type interfaceType)
    {
      this.InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }

    public override bool CanConvert(Type typeToConvert)
    {
      if (typeToConvert.Equals(typeof(IList<>).MakeGenericType(this.InterfaceType))
       && typeToConvert.GenericTypeArguments[0].Equals(this.InterfaceType))
      {
        return true;
      }

      return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      return (JsonConverter)Activator.CreateInstance(
          typeof(ListConverter<>).MakeGenericType(this.InterfaceType));
    }
  }
}
