namespace DeepNestLib
{
  using System.ComponentModel;
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Threading;

  public class NestState : INestState, INotifyPropertyChanged
  {
    private int callCounter;
    private int generations;
    private int iterations;
    private int population;
    private int threads;
    private int nestCount;
    private long totalNestTime;
    private long lastPlacementTime;

    public NestState(ISvgNestConfig config, IDispatcherService dispatcherService)
    {
      this.TopNestResults = new TopNestResultsCollection(config, dispatcherService);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public long AverageNestTime => nestCount == 0 ? 0 : totalNestTime / nestCount;

    public int CallCounter => callCounter;

    public int Generations => generations;

    public bool IsErrored { get; private set; }

    public int Iterations => iterations;

    public long LastPlacementTime => lastPlacementTime;

    public int NestCount => nestCount;

    public int Population => population;

    public int Threads => threads;

    public TopNestResultsCollection TopNestResults { get; }

    public static NestState CreateInstance(ISvgNestConfig config, IDispatcherService dispatcherService) => new NestState(config, dispatcherService);

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

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPlacementTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NestCount)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageNestTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Generations)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Iterations)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CallCounter)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopNestResults)));
    }

    internal void IncrementPopulation()
    {
      Interlocked.Increment(ref population);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
    }

    internal void SetLastPlacementTime(long placePartTime)
    {
      lastPlacementTime = placePartTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPlacementTime)));
    }

    internal void IncrementNestCount()
    {
      Interlocked.Increment(ref nestCount);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NestCount)));
    }

    internal void IncrementTotalNestTime(long placePartTime)
    {
      totalNestTime += placePartTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageNestTime)));
    }

    internal void IncrementGenerations()
    {
      Interlocked.Increment(ref generations);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Generations)));
    }

    internal void ResetPopulation()
    {
      Interlocked.Exchange(ref population, 0);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
    }

    internal void IncrementThreads()
    {
      Interlocked.Increment(ref threads);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
    }

    internal void DecrementThreads()
    {
      Interlocked.Decrement(ref threads);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
    }

    internal void IncrementIterations()
    {
      Interlocked.Increment(ref iterations);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Iterations)));
    }

    internal void IncrementCallCounter()
    {
      Interlocked.Increment(ref callCounter);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CallCounter)));
    }

    internal void SetIsErrored()
    {
      this.IsErrored = true;
    }
  }
}
