namespace DeepNestLib.CiTests.NestProject
{
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using System;
  using Xunit;

  public class DetailLoadInfoSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var random = new Random();
      var sut = new DetailLoadInfo()
      {
        IsDifferentiated = random.NextBool(),
        IsIncluded = random.NextBool(),
        IsMultiplied = random.NextBool(),
        IsPriority = random.NextBool(),
        Path = random.NextString(),
        Quantity = random.Next(),
        StrictAngle = random.Next<AnglesEnum>()
      };
      var json = sut.ToJson();
      DetailLoadInfo actual = DetailLoadInfo.FromJson(json);

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
