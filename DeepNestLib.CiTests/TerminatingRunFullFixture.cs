namespace DeepNestLib.CiTests
{
  using FakeItEasy;

  public class TerminatingRunFullFixture
  {
    private readonly int maxIterations;

    protected NestingContext nestingContext;

    protected double ExpectedFitness { get; }

    protected double ExpectedFitnessTolerance { get; }

    protected TestSvgNestConfig config { get; }

    private readonly ProgressTestResponse progressCapture = new ProgressTestResponse();
    private int iterations = 0;

    protected TerminatingRunFullFixture(PlacementTypeEnum placementType, double expectedFitness, double expectedFitnessTolerance, int maxIterations)
    {
      this.config = new TestSvgNestConfig();
      this.config.PlacementType = placementType;
      this.config.UseMinkowskiCache = false;
      this.config.CurveTolerance = 1;
      this.config.UseDllImport = false;
      this.config.PopulationSize = 40;
      this.config.UseParallel = false;
      this.config.ExportExecutions = false;

      ResetIteration();
      this.ExpectedFitness = expectedFitness;
      this.ExpectedFitnessTolerance = expectedFitnessTolerance;
      this.maxIterations = maxIterations;
    }

    protected bool HasMetTerminationConditions
    {
      get
      {
        return iterations >= maxIterations || HasAchievedExpectedFitness;
      }
    }

    protected bool HasAchievedExpectedFitness
    {
      get
      {
        return !(this.nestingContext.State.TopNestResults.Top == default ||
                 this.nestingContext.State.TopNestResults.Top.Fitness > ExpectedFitness + ExpectedFitnessTolerance);
      }
    }

    protected void ResetIteration()
    {
      iterations = 0;
      this.nestingContext = new NestingContext(new SystemDiagnosticMessageService(), progressCapture, new NestState(config, A.Fake<IDispatcherService>()), this.config);
    }

    protected void AwaitIterate()
    {
      iterations++;
      _ = this.nestingContext.NestIterate(this.config);
      progressCapture.Are.WaitOne(100);
    }
  }
}
