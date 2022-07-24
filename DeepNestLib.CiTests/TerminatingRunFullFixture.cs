namespace DeepNestLib.CiTests
{
  using FakeItEasy;

  public class TerminatingRunFullFixture
  {
    protected readonly NestingContext nestingContext;
    protected readonly int terminateNestResultCount;
    private readonly int maxIterations;

    protected double ExpectedFitness { get; }

    protected double ExpectedFitnessTolerance { get; }

    protected TestSvgNestConfig config { get; }

    private readonly ProgressTestResponse progressCapture = new ProgressTestResponse();
    private int iterations = 0;

    protected TerminatingRunFullFixture(PlacementTypeEnum placementType, double expectedFitness, double expectedFitnessTolerance, int terminateNestResultCount, int maxIterations)
    {
      this.config = new TestSvgNestConfig();
      this.config.PlacementType = placementType;
      this.config.UseMinkowskiCache = false;
      this.config.CurveTolerance = 1;
      this.config.UseDllImport = false;
      this.config.PopulationSize = 40;
      this.config.UseParallel = false;
      this.config.ExportExecutions = false;
      this.nestingContext = new NestingContext(A.Fake<IMessageService>(), progressCapture, new NestState(config, A.Fake<IDispatcherService>()), this.config);
      this.ExpectedFitness = expectedFitness;
      this.ExpectedFitnessTolerance = expectedFitnessTolerance;
      this.terminateNestResultCount = terminateNestResultCount;
      this.maxIterations = maxIterations;
    }

    protected bool HasMetTerminationConditions
    {
      get
      {
        return iterations >= maxIterations || (!(!HasAchievedExpectedFitness() &&
               this.nestingContext.State.TopNestResults.Count < terminateNestResultCount));
      }
    }

    protected bool HasAchievedExpectedFitness()
    {
      return !(this.nestingContext.State.TopNestResults.Top == default ||
               this.nestingContext.State.TopNestResults.Top.Fitness > ExpectedFitness + ExpectedFitnessTolerance);
    }

    protected void AwaitIterate()
    {
      iterations++;
      this.nestingContext.NestIterate(this.config);
      progressCapture.Are.WaitOne(100);
    }
  }
}
