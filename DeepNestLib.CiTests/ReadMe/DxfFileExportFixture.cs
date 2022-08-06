namespace DeepNestLib.CiTests.ReadMe
{
  using DeepNestLib.Placement;
  using FluentAssertions;
  using System.IO;
  using System.Reflection;
  using System.Threading.Tasks;
  using Xunit;

  public class DxfFileExportFixture
  {
    [Fact]
    public async Task GivenNestResultWhenDxfExportedThenMatchExpected()

    {
      var path = "ReadMe.ReadMeExampleSmall.dnr";
      string actual;
      using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
        var nestResult = NestResult.FromJson(fromFile);
        var memoryStream = new MemoryStream();
        
        await nestResult.UsedSheets[0].ExportDxf(memoryStream, false, false).ConfigureAwait(false);
        memoryStream.Position = 0;
        actual = new StreamReader(memoryStream).ReadToEnd();
        actual.Should().NotBeNullOrEmpty();
      }

      path = "ReadMe.ReadMeExampleSmall.dxf";
      using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      using (StreamReader reader = new StreamReader(stream))
      {
        string expected = reader.ReadToEnd();
        var preTimestamp = actual.IndexOf("$TDCREATE");
        actual.Substring(0, preTimestamp).Should().BeEquivalentTo(expected.Substring(0, preTimestamp));        
        actual.Substring(actual.IndexOf("$USRTIMER")).Should().BeEquivalentTo(expected.Substring(expected.IndexOf("$USRTIMER")));
      }
    }

    [Fact]
    public async Task GivenNestResultWhenDxfExportedColouredThenMatchExpected()

    {
      var path = "ReadMe.ReadMeExampleSmall.dnr";
      string actual;
      using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
        var nestResult = NestResult.FromJson(fromFile);
        var memoryStream = new MemoryStream();


        await nestResult.UsedSheets[0].ExportDxf(memoryStream, false, true).ConfigureAwait(false);
        memoryStream.Position = 0;
        actual = new StreamReader(memoryStream).ReadToEnd();
        actual.Should().NotBeNullOrEmpty();
      }

      path = "ReadMe.ReadMeExampleSmallColoured.dxf";
      using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      using (StreamReader reader = new StreamReader(stream))
      {
        string expected = reader.ReadToEnd();
        var preTimestamp = actual.IndexOf("$TDCREATE");
        actual.Substring(0, preTimestamp).Should().BeEquivalentTo(expected.Substring(0, preTimestamp));
        actual.Substring(actual.IndexOf("$USRTIMER")).Should().BeEquivalentTo(expected.Substring(expected.IndexOf("$USRTIMER")));
      }
    }
  }
}
