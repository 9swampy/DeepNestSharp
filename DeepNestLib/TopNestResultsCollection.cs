namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.Placement;

  public class TopNestResultsCollection : IEnumerable<INestResult>
  {
    private readonly ISvgNestConfig config;
    private List<INestResult> items = new List<INestResult>();
    private static volatile object addToTopNestsLock = new object();

    public TopNestResultsCollection(ISvgNestConfig config)
    {
      this.config = config;
    }

    public int Count => items.Count;

    public INestResult Top => items?.FirstOrDefault();

    public bool Add(INestResult payload)
    {
      lock (addToTopNestsLock)
      {
        var isAdded = true;
        if (items.Count == 0)
        {
          items.Insert(0, payload);
          isAdded = true;
        }
        else
        {
          int i = 0;
          while (i < items.Count && items[i].Fitness < payload.Fitness)
          {
            i++;
          }

          if (i == items.Count)
          {
            items.Add(payload);
            isAdded = true;
          }
          else if (Math.Round(items[i].Fitness, 3) != Math.Round(payload.Fitness, 3))
          {
            items.Insert(i, payload);
            isAdded = true;
          }
        }

        if (items.Count > MaxCapacity)
        {
          items.RemoveAt(items.Count - 1);
        }

        return isAdded;
      }
    }

    public int EliteSurvivors
    {
      get
      {
        return config.PopulationSize / 10;
      }
    }

    public int MaxCapacity
    {
      get
      {
        var result = config.PopulationSize * 2 / 10;
        if (result <= 0)
        {
          throw new InvalidOperationException("MaxCapacity is zero so no results will ever be captured. Fix the configuration (or feed in DefaultSvgNestConfig if it's a test).");
        }

        return result;
      }
    }

    public IEnumerator<INestResult> GetEnumerator()
    {
      return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return items.GetEnumerator();
    }

    public int IndexOf(INestResult nestResult)
    {
      return this.items.IndexOf(nestResult);
    }

    internal void Clear()
    {
      this.items.Clear();
    }
  }
}
