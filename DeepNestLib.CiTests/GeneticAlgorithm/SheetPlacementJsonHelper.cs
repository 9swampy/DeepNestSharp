namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;

  public static class SheetPlacementJsonHelper
  {
    public static ISheetPlacement LoadSheetPlacement(string relativeResourcePath)
    {
      ShouldRoundTripSerialize(relativeResourcePath);
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(relativeResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        return SheetPlacement.FromJson(reader.ReadToEnd());
      }
    }

    public static void ShouldRoundTripSerialize(string relativeResourcePath)
    {
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(relativeResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        var jsonIn = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
        var jsonOut = SheetPlacement.FromJson(jsonIn).ToJson();
        jsonOut.Should().Be(jsonIn);
      }
    }
  }
}
