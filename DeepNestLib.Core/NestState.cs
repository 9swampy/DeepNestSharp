namespace DeepNestLib
{
  using System;
  using System.ComponentModel;
  using System.Linq;
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Threading;

  public class NestState : INestState, INestStateBackground, INestStateSvgNest, INestStateMinkowski, INestStateNestingContext, INotifyPropertyChanged
  {
    private int clipperCallCounter;
    private int dllCallCounter;
    private int generations;
    private int iterations;
    private int population;
    private int rejected;
    private int threads;
    private int nestCount;
    private long totalNestTime;
    private long lastPlacementTime;
    private double nfpPairCachePercentCached;
    private long lastNestTime;
    private long totalPlacementTime;

    public NestState(ITopNestResultsConfig config, IDispatcherService dispatcherService)
    {
      this.TopNestResults = new TopNestResultsCollection(config, dispatcherService);
      this.TopNestResults.CollectionChanged += this.TopNestResults_CollectionChanged;
    }

    private void TopNestResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
      {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastTopFoundTimestamp)));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [Description("Average time per Nest Result since start of the run.")]
    [Category("Performance")]
    [DisplayName("Average Nest Time")]
    public long AverageNestTime => this.nestCount == 0 ? 0 : this.totalNestTime / this.nestCount;

    [Description("Average time per Nest Result since start of the run.")]
    [Category("Performance")]
    [DisplayName("Average Placement Time")]
    public long AveragePlacementTime => this.nestCount == 0 ? 0 : this.totalPlacementTime / this.nestCount;

    [Description("The number of times the external C++ Minkowski library has been called. This should stabilise at the number of distinct parts in the nest times the number of rotations. If it keeps growing then the caching mechanism may not be working as intended; possibly due to complexity of the parts, possibly due to overflow failures in the Minkoski Sum. That said, if your parts have holes then the calls to hole NfpSums aren't cached?")]
    [Category("Minkowski")]
    [DisplayName("Dll Call Counter")]
    public int DllCallCounter => this.dllCallCounter;

    [Category("Minkowski")]
    [DisplayName("Clipper Call Counter")]
    public int ClipperCallCounter => this.clipperCallCounter;

    [Description("The number of generations processed.")]
    [Category("Genetic Algorithm")]
    public int Generations => this.generations;

    [Category("Progress")]
    public bool IsErrored { get; private set; }

    [Category("Progress")]
    public int Iterations => this.iterations;

    [Description("Last Nest Time (milliseconds). The total time for the nest including Pmap, DeepNest and Placement.")]
    [Category("Performance")]
    [DisplayName("Last Nest Time")]
    public long LastNestTime => this.lastNestTime;

    [Description("Last Placement Time (milliseconds).")]
    [Category("Performance")]
    [DisplayName("Last Placement Time")]
    public long LastPlacementTime => this.lastPlacementTime;

    [Description("Time last top placement found.")]
    [Category("Performance")]
    [DisplayName("Last Top Found")]
    public DateTime? LastTopFoundTimestamp
    {
      get
      {
        if (this.TopNestResults.Count == 0)
        {
          return null;
        }
        else
        {
          return this.TopNestResults.Max(o => o.CreatedAt);
        }
      }
    }

    [Category("Progress")]
    [DisplayName("Nest Count")]
    public int NestCount => this.nestCount;

    [Category("Performance")]
    [DisplayName("NfpPair % Cached")]
    public double NfpPairCachePercentCached => this.nfpPairCachePercentCached;

    /// <inheritdoc />
    [Description("Population of the current generation.")]
    [Category("Genetic Algorithm")]
    public int Population => this.population;

    [Description("Number of rejected Nests.")]
    [Category("Performance")]
    public int Rejected => this.rejected;

    [Description("Number of active Nest threads.")]
    [Category("Performance")]
    public int Threads => this.threads;

    [Browsable(false)]
    public TopNestResultsCollection TopNestResults { get; }

    public static NestState CreateInstance(ISvgNestConfig config, IDispatcherService dispatcherService) => new NestState(config, dispatcherService);

    void INestStateNestingContext.Reset()
    {
      Interlocked.Exchange(ref this.nestCount, 0);
      Interlocked.Exchange(ref this.totalNestTime, 0);
      Interlocked.Exchange(ref this.totalPlacementTime, 0);
      Interlocked.Exchange(ref this.generations, 0);
      Interlocked.Exchange(ref this.population, 0);
      Interlocked.Exchange(ref this.lastNestTime, 0);
      Interlocked.Exchange(ref this.lastPlacementTime, 0);
      Interlocked.Exchange(ref this.iterations, 0);
      Interlocked.Exchange(ref this.dllCallCounter, 0);
      Interlocked.Exchange(ref this.clipperCallCounter, 0);
      this.IsErrored = false;
      this.TopNestResults.Clear();
      this.SetNfpPairCachePercentCached(0);

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastTopFoundTimestamp)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Population)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastPlacementTime)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastNestTime)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NestCount)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AveragePlacementTime)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AverageNestTime)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Generations)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Threads)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Iterations)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DllCallCounter)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ClipperCallCounter)));
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TopNestResults)));
    }

    void INestStateSvgNest.IncrementPopulation()
    {
      Interlocked.Increment(ref this.population);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Population)));
    }

    void INestStateSvgNest.SetLastPlacementTime(long placePartTime)
    {
      this.lastPlacementTime = placePartTime;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastPlacementTime)));
    }

    void INestStateSvgNest.SetLastNestTime(long backgroundTime)
    {
      this.lastNestTime = backgroundTime;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastNestTime)));
    }

    void INestStateSvgNest.IncrementNestCount()
    {
      Interlocked.Increment(ref this.nestCount);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NestCount)));
    }

    void INestStateSvgNest.IncrementNestTime(long backgroundTime)
    {
      this.totalPlacementTime += backgroundTime;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AverageNestTime)));
    }

    void INestStateSvgNest.IncrementPlacementTime(long placePartTime)
    {
      this.totalNestTime += placePartTime;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AveragePlacementTime)));
    }

    void INestStateSvgNest.IncrementGenerations()
    {
      Interlocked.Increment(ref this.generations);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Generations)));
    }

    void INestStateSvgNest.ResetPopulation()
    {
      Interlocked.Exchange(ref this.population, 0);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Population)));
    }

    void INestStateSvgNest.IncrementThreads()
    {
      Interlocked.Increment(ref this.threads);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Threads)));
    }

    void INestStateSvgNest.DecrementThreads()
    {
      Interlocked.Decrement(ref this.threads);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Threads)));
    }

    void INestStateNestingContext.IncrementIterations()
    {
      Interlocked.Increment(ref this.iterations);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Iterations)));
    }

    void INestStateMinkowski.IncrementDllCallCounter()
    {
      Interlocked.Increment(ref this.dllCallCounter);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DllCallCounter)));
    }

    void INestStateMinkowski.IncrementClipperCallCounter()
    {
      Interlocked.Increment(ref this.clipperCallCounter);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ClipperCallCounter)));
    }

    public void IncrementRejected()
    {
      Interlocked.Increment(ref this.rejected);
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rejected)));
    }

    public void SetIsErrored()
    {
      this.IsErrored = true;
    }

    public void SetNfpPairCachePercentCached(double percentCached)
    {
      this.nfpPairCachePercentCached = percentCached;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NfpPairCachePercentCached)));
    }
  }
}
