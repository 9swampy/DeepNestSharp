namespace DeepNestLib.CiTests
{
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using System;

  public abstract class TerminatingRunFullFixture
  {
    private static volatile object testSyncLock = new object();

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
        if (!HasRun()) return false;
        if (IsTopFitnessGoodEnough) return true;
        return false;
      }
    }

    private bool IsTopFitnessGoodEnough
    {
      get
      {
        return this.nestingContext.State.TopNestResults.Top.FitnessTotal <= ExpectedFitness + ExpectedFitnessTolerance;
      }
    }

    private bool HasRun()
    {
      return this.nestingContext.State.TopNestResults.Top != default;
    }

    protected bool HasImportedRawDetail { get; set; }

    protected void ExecuteTest()
    {
      lock (testSyncLock)
      {
        while (!HasAchievedExpectedFitness && !HasRetriedMaxRuns)
        {
          if (!HasImportedRawDetail)
          {
            HasImportedRawDetail = LoadRawDetail();
          }
          
          HasImportedRawDetail.Should().BeTrue();
          ResetIteration();
          PrepIteration();
          nestingContext.StartNest().Wait();
          nestingContext.State.StartedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
          while (!HasMetTerminationConditions)
          {
            AwaitIterate();
          }
        }
      }
    }

    protected abstract void PrepIteration();

    protected abstract bool LoadRawDetail();

    private void ResetIteration()
    {
      iterations = 0;
      numRuns++;
      this.nestingContext = new NestingContext(new SystemDiagnosticMessageService(), progressCapture, new NestState(Config, A.Fake<IDispatcherService>()), this.Config);
    }

    protected void AwaitIterate()
    {
      iterations++;
      progressCapture.Are.Reset();
      _ = this.nestingContext.NestIterate(this.Config);
      progressCapture.Are.WaitOne(100);
    }
  }
}
