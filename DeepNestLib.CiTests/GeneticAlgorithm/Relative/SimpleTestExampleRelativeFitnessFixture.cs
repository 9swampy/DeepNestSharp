namespace DeepNestLib.CiTests.GeneticAlgorithm.Relative
{
  using DeepNestLib.Placement;
  using FluentAssertions;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Xunit;

  public class SimpleTestExampleRelativeFitnessFixture
  {
    private readonly IList<INestResult> scenarios = new INestResult[6];

    public SimpleTestExampleRelativeFitnessFixture()
    {
      scenarios[0] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-Best-1.dnr");
      scenarios[1] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-2.dnr");
      scenarios[2] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-3.dnr");
      scenarios[3] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-4.dnr");
      scenarios[4] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-5.dnr");
      scenarios[5] = GravityFitnessOneSheetBetterThanTwoFixture.LoadNestResult("GeneticAlgorithm.Relative.SimpleTestExample-Worst-6.dnr");
    }

    [Fact]
    public void GivenScenario1WasOriginallyOutOfOrderWhenRestFitnessComparedThenShouldBeSameOrder()
    {
      var rest = scenarios.ToList();
      rest.RemoveAt(1);
      rest.OrderBy(o => o.FitnessTotal).Should().BeEquivalentTo(rest, opt => opt.WithStrictOrdering());
    }

    [Fact]
    public void GivenScenariosInOrderWhenFitnessComparedThenShouldBeSameOrder()
    {
      scenarios.OrderBy(o => o.FitnessTotal).Should().BeEquivalentTo(scenarios, opt => opt.WithStrictOrdering());
    }

    [Fact]
    public void GivenScenario3WhenFitnessTotalComparedWith2ThenShouldBeWorse()
    {
      scenarios[1].FitnessTotal.Should().BeLessThan(scenarios[2].FitnessTotal);
    }

    [Fact]
    public void GivenScenario3WhenFitnessBoundsComparedWith2ThenShouldBeWorse()
    {
      scenarios[1].FitnessBounds.Should().BeLessThan(scenarios[2].FitnessBounds);
    }

    [Fact]
    public void GivenNoUnplacedPartsThenAllFitnessUnplacedShouldBe0()
    {
      scenarios.All(o => o.FitnessUnplaced == 0).Should().BeTrue();
    }

    [Fact]
    public void GivenSheetsSameThenAllFitnessSheetsShouldBeSame()
    {
      var rest = scenarios.ToList();
      rest.RemoveAt(0); //Top nest fits all on only 2 sheets
      rest.Select(o => o.FitnessSheets).Distinct().Count().Should().Be(1);
    }
  }
}
