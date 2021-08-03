namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Linq;
  using System.Threading;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using GeneticSharp.Domain.Populations;

  public class SvgNestState
  {
    private static int generations = 0;
    private static int population = 0;
    private static int threads = 0;
    private static int nestCount = 0;
    private static long totalNestTime = 0;
    private static long lastPlacementTime = 0;

    public static SvgNestState Default { get; } = new SvgNestState();

    public long AverageNestTime => nestCount == 0 ? 0 : totalNestTime / nestCount;

    public long LastPlacementTime => lastPlacementTime;

    public int NestCount => nestCount;

    public int Population => population;

    public int Threads => threads;

    public int Generations => generations;

    internal void Reset()
    {
      Interlocked.Exchange(ref nestCount, 0);
      Interlocked.Exchange(ref totalNestTime, 0);
      Interlocked.Exchange(ref generations, 0);
      Interlocked.Exchange(ref population, 0);
      Interlocked.Exchange(ref lastPlacementTime, 0);
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
  }
}
