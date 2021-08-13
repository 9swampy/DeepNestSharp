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
      if (double.IsNaN(payload.Fitness))
      {
        return false;
      }

      if (payload.TotalPartsCount > payload.TotalParts)
      {
        return false;
      }

      if (dispatcherService.InvokeRequired)
      {
        bool result = false;
        dispatcherService.Invoke(() => result = TryAdd(payload));
        return result;
      }
      else
      {
        lock (lockItemsObject)
        {
          var isAdded = false;
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
              if (items.Count < MaxCapacity)
              {
                items.Add(payload);
                isAdded = true;
              }
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
