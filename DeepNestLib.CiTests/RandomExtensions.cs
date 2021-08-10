namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using Xunit;

  public static class RandomExtensions
  {
    public static bool NextBool(this Random random)
    {
      return random.Next(-1, 0) == -1;
    }

    public static string NextString(this Random random)
    {
      return Guid.NewGuid().ToString();
    }

    public static TEnum Next<TEnum>(this Random random)
      where TEnum : Enum
    {
      var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
      var index = random.Next(0, values.Length - 1);
      return values[index];
    }
  }
}
