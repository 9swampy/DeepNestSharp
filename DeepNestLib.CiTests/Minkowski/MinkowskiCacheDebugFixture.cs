namespace DeepNestLib.CiTests
{
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class MinkowskiCacheDebugFixture
  {
    private readonly MinkowskiDictionary cache;
    private readonly INfp a;
    private readonly INfp b;
    private readonly INfp ret;

    public MinkowskiCacheDebugFixture()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiCache.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      cache = MinkowskiDictionary.FromJson(json);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiCacheA.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      a = NoFitPolygon.FromJson(json);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiCacheA.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      b = NoFitPolygon.FromJson(json);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiCacheRet.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      ret = NoFitPolygon.FromJson(json);
    }

    [Fact]
    public void CacheShouldHaveEntries()
    {
      cache.ToList().Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public void CacheHasValueMatchingRet()
    {
      // var match = cache.Values.ToList().Single(o => o.Equals(ret));
    }
  }
}
