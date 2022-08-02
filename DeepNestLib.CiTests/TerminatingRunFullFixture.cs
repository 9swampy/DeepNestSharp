namespace DeepNestLib.CiTests
{
  using FakeItEasy;

  public class TerminatingRunFullFixture
  {
    private readonly int maxIterations;
    private readonly int maxRuns;

    protected NestingContext nestingContext;
    int numRuns = 0; //These tests flake fairly regularly, but always pass after a few runs.
                     //If the Ga doesn't start off in the `right` place then it settles on an
                     //insufficiently good TopNestResult... so force it to retry...

    protected double ExpectedFitness { get; }

    protected double ExpectedFitnessTolerance { get; }

    protected bool HasRetriedMaxRuns => numRuns >= maxRuns;

    protected TestSvgNestConfig Config { get; }

    private readonly ProgressTestResponse progressCapture = new ProgressTestResponse();
    private int iterations = 0;

    protected TerminatingRunFullFixture(PlacementTypeEnum placementType, double expectedFitness, double expectedFitnessTolerance, int maxIterations)
      : this(placementType, expectedFitness, expectedFitnessTolerance, maxIterations, 3)
    {
    }

    protected TerminatingRunFullFixture(PlacementTypeEnum placementType, double expectedFitness, double expectedFitnessTolerance, int maxIterations, int maxRuns)
    {
      this.Config = new TestSvgNestConfig();
      this.Config.PlacementType = placementType;
      this.Config.UseMinkowskiCache = false;
      this.Config.CurveTolerance = 1;
      this.Config.UseDllImport = false;
      this.Config.PopulationSize = 40;
      this.Config.UseParallel = false;
      this.Config.ExportExecutions = false;

      ResetIteration();
      this.ExpectedFitness = expectedFitness;
      this.ExpectedFitnessTolerance = expectedFitnessTolerance;
      this.maxIterations = maxIterations;
      this.maxRuns = maxRuns;
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
      numRuns++;
      this.nestingContext = new NestingContext(new SystemDiagnosticMessageService(), progressCapture, new NestState(Config, A.Fake<IDispatcherService>()), this.Config);
    }

    protected void AwaitIterate()
    {
      iterations++;
      _ = this.nestingContext.NestIterate(this.Config);
      progressCapture.Are.WaitOne(100);
    }
  }
}
