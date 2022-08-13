namespace DeepNestLib
{
  using System;

  public interface INestState
  {
    /// <summary>
    /// Gets the average time per Nest Result since start of the run.
    /// </summary>
    long AverageThreadPlacementTime { get; }

    /// <summary>
    /// Gets the average time per Nest Result since start of the run.
    /// </summary>
    long AverageThreadNestTime { get; }

    int DllCallCounter { get; }

    int Generations { get; }

    bool IsErrored { get; }

    int Iterations { get; }

    /// <summary>
    /// Gets the last placement Time (milliseconds).
    /// </summary>
    long LastPlacementTime { get; }

    int NestCount { get; }

    /// <summary>
    /// Gets the NfpPair % Cached.
    /// </summary>
    double NfpPairCachePercentCached { get; }

    /// <summary>
    /// Gets the Population of the current generation.
    /// </summary>
    int Population { get; }

    int Threads { get; }

    DateTime? StartedAt { get; }

    TimeSpan? Elapsed { get; }

    TopNestResultsCollection TopNestResults { get; }

    void SetIsErrored();
  }

  public interface INestStateBackground : INestState
  {
    int BackgroundStarted { get; set; }

    void SetNfpPairCachePercentCached(double percentCached);
  }

  public interface INestStateSvgNest : INestState
  {
    void DecrementThreads();

    void IncrementGenerations();

    void IncrementNestCount();

    void IncrementNestTime(long backgroundTime);

    void IncrementPlacementTime(long placePartTime);

    void IncrementPopulation();

    void IncrementRejected();

    void IncrementThreads();

    void ResetPopulation();

    void SetLastNestTime(long backgroundTime);

    void SetLastPlacementTime(long placePartTime);
  }

  public interface INestStateMinkowski : INestState
  {
    void IncrementDllCallCounter();

    void IncrementClipperCallCounter();
  }

  public interface INestStateNestingContext : INestState
  {
    void IncrementIterations();

    void StartNest();

    void Reset();
  }
}