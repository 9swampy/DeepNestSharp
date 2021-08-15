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
    private static volatile object lockItemsObject = new object();

    private readonly ISvgNestConfig config;
    private readonly IDispatcherService dispatcherService;

    private ObservableCollection<INestResult> items = new ObservableCollection<INestResult>();

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public TopNestResultsCollection(ISvgNestConfig config, IDispatcherService dispatcherService)
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

    public bool TryAdd(INestResult payload)
    {
      var result = false;
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => result = TryAdd(payload));
      }
      else
      {
        lock (lockItemsObject)
        {
          if (items.Count == 0)
          {
            items.Insert(0, payload);
            result = true;
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
              if (items.Count < MaxCapacity)
              {
                items.Add(payload);
                result = true;
              }
            }
            else if (Math.Round(items[i].Fitness, 3) == Math.Round(payload.Fitness, 3))
            {
              // Duplicate - respond true so the TryAdd consumer can report duplicate as
              // it won't find the result in the list
              result = true;
            }
            else
            {
              items.Insert(i, payload);
              result = true;
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
  }
}
