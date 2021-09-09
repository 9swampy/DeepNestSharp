namespace DeepNestLib.Placement
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class InterfaceConverterFactory : JsonConverterFactory
  {
    public InterfaceConverterFactory(Type concrete, Type interfaceType)
    {
      this.ConcreteType = concrete;
      this.InterfaceType = interfaceType;
    }

    public Type ConcreteType { get; }

    public Type InterfaceType { get; }

    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert == this.InterfaceType;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      var converterType = typeof(InterfaceConverter<,>).MakeGenericType(this.ConcreteType, this.InterfaceType);

      return (JsonConverter)Activator.CreateInstance(converterType);
    }
  }
}
