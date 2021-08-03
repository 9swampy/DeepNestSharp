namespace DeepNestLib
{
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Threading;

  public class NestState : INestState
  {
    private int callCounter;
    private int generations;
    private int iterations;
    private int population;
    private int threads;
    private int nestCount;
    private long totalNestTime;
    private long lastPlacementTime;

    private NestState(ISvgNestConfig config)
    {
      this.TopNestResults = new TopNestResultsCollection(config);
    }

    public static NestState Default { get; } = new NestState(SvgNest.Config);

    public static NestState CreateInstance(ISvgNestConfig config) => new NestState(config);

    public long AverageNestTime => nestCount == 0 ? 0 : totalNestTime / nestCount;

    public long LastPlacementTime => lastPlacementTime;

    public bool IsErrored { get; private set; }

    public int Iterations => iterations;

    public int NestCount => nestCount;

    public int Population => population;

    public int Threads => threads;

    public TopNestResultsCollection TopNestResults { get; }

    public int Generations => generations;

    public int CallCounter => callCounter;

    internal void Reset()
    {
      Interlocked.Exchange(ref nestCount, 0);
      Interlocked.Exchange(ref totalNestTime, 0);
      Interlocked.Exchange(ref generations, 0);
      Interlocked.Exchange(ref population, 0);
      Interlocked.Exchange(ref lastPlacementTime, 0);
      Interlocked.Exchange(ref iterations, 0);
      Interlocked.Exchange(ref callCounter, 0);
      this.IsErrored = false;
      TopNestResults.Clear();
    }

    internal void IncrementPopulation()
    {
      Interlocked.Increment(ref population);
    }

    internal void SetLastPlacementTime(long placePartTime)
    {
      lastPlacementTime = placePartTime;
    }

    internal void IncrementNestCount()
    {
      Interlocked.Increment(ref nestCount);
    }

    internal void IncrementTotalNestTime(long placePartTime)
    {
      totalNestTime += placePartTime;
    }

    internal void IncrementGenerations()
    {
      Interlocked.Increment(ref generations);
    }

    internal void ResetPopulation()
    {
      Interlocked.Exchange(ref population, 0);
    }

    internal void IncrementThreads()
    {
      Interlocked.Increment(ref threads);
    }

    internal void DecrementThreads()
    {
      Interlocked.Decrement(ref threads);
    }

    internal void IncrementIterations()
    {
      Interlocked.Increment(ref iterations);
    }

    internal void IncrementCallCounter()
    {
      Interlocked.Increment(ref callCounter);
    }

    internal void SetIsErrored()
    {
      this.IsErrored = true;
    }
  }
}
