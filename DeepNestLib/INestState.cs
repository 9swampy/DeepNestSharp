namespace DeepNestLib
{
  public interface INestState
  {
    long AveragePlacementTime { get; }

    long AverageNestTime { get; }

    int CallCounter { get; }

    int Generations { get; }

    bool IsErrored { get; }

    int Iterations { get; }

    long LastPlacementTime { get; }

    int NestCount { get; }

    double NfpPairCachePercentCached { get; }

    int Population { get; }

    int Threads { get; }

    TopNestResultsCollection TopNestResults { get; }

    void SetIsErrored();
  }

  public interface INestStateBackground : INestState
  {
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
    void IncrementCallCounter();
  }

  public interface INestStateNestingContext : INestState
  {
    void IncrementIterations();

    void Reset();
  }
}