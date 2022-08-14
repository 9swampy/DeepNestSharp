namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;
  using System.Linq;
  using DeepNestLib.Placement;

  public class TopNestResultsCollection : IEnumerable<INestResult>, INotifyCollectionChanged
  {
    private const int UiSurvivorsMin = 20;
    private static volatile object lockItemsObject = new object();
    private readonly ITopNestResultsConfig config;

    private readonly IDispatcherService dispatcherService;

    private ObservableCollection<INestResult> items = new ObservableCollection<INestResult>();

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public TopNestResultsCollection(ITopNestResultsConfig config, IDispatcherService dispatcherService)
    {
      this.config = config;
      this.dispatcherService = dispatcherService;
      this.items.CollectionChanged += this.Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => Items_CollectionChanged(sender, e));
      }
      else
      {
        CollectionChanged?.Invoke(this, e);
      }
    }

    public int Count => items.Count;

    public INestResult Top => items?.FirstOrDefault();

    public int EliteSurvivors
    {
      get
      {
        return Math.Max(config.PopulationSize / 10, UiSurvivorsMin);
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

        return Math.Max(result, EliteSurvivors);
      }
    }

    public bool IsEmpty => this.items.Count == 0;

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

    internal static bool IsANovelNest(double payload, double incumbent, int index, double topDiversity)
    {
      try
      {
        if (index == 0)
        {
          return Math.Round(incumbent, 2) != Math.Round(payload, 2);
        }

        return Math.Abs(incumbent - payload) > (incumbent * topDiversity);
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    internal void Clear()
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => Clear());
      }
      else
      {
        lock (lockItemsObject)
        {
          this.items.Clear();
        }
      }
    }

    internal TryAddResult TryAdd(INestResult payload)
    {
      try
      {
        TryAddResult result = TryAddResult.NotAdded;
        if (dispatcherService.InvokeRequired)
        {
          dispatcherService.Invoke(() => result = TryAdd(payload));
        }
        else
        {
          lock (lockItemsObject)
          {
            if (this.Any(o => o.FitnessTotal == payload.FitnessTotal))
            {
              System.Diagnostics.Debug.Print("Shouldn't normally add...");
            }

            if (items.Count == 0)
            {
              items.Insert(0, payload);
              result = TryAddResult.Added;
            }
            else
            {
              int i = 0;
              while (i < items.Count && items[i].FitnessTotal < payload.FitnessTotal)
              {
                i++;
              }

              if (items.Count < MaxCapacity)
              {
                if (i == items.Count || IsANovelNest(payload.FitnessTotal, items[i].FitnessTotal, i, config.TopDiversity))
                {
                  items.Insert(i, payload);
                  result = TryAddResult.Added;
                }
              }
              else
              {
                if (i < items.Count)
                {
                  if (!IsANovelNest(payload.FitnessTotal, items[i].FitnessTotal, i, config.TopDiversity))
                  {
                    if (IsSubstituteForOldest(i))
                    {
                      items[i] = payload;
                      result = TryAddResult.Substitute;
                    }
                    else
                    {
                      // Duplicate - respond true so the TryAdd consumer can report duplicate as
                      // it won't find the result in the list
                      result = TryAddResult.Duplicate;
                    }
                  }
                  else
                  {
                    items.Insert(i, payload);
                    result = TryAddResult.Added;
                  }
                }
                else
                {
                  result = TryAddResult.NotAdded;
                }
              }
            }

            if (items.Count > MaxCapacity)
            {
              items.RemoveAt(items.Count - 1);
            }
          }
        }

        return result;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    private bool IsSubstituteForOldest(int i)
    {
      try
      {
        return this.OrderByDescending(o => o.PlacePartTime).Take(Math.Max(MaxCapacity / 10, 1)).Any(o => o == items[i]);
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}
