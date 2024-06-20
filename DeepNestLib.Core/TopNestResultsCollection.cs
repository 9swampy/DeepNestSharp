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
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.Items_CollectionChanged(sender, e));
      }
      else
      {
        this.CollectionChanged?.Invoke(this, e);
      }
    }

    public int Count => this.items.Count;

    public INestResult Top => this.items?.FirstOrDefault();

    public int EliteSurvivors
    {
      get
      {
        return Math.Max(this.config.PopulationSize / 10, UiSurvivorsMin);
      }
    }

    public int MaxCapacity
    {
      get
      {
        var result = this.config.PopulationSize * 2 / 10;
        if (result <= 0)
        {
          throw new InvalidOperationException("MaxCapacity is zero so no results will ever be captured. Fix the configuration (or feed in DefaultSvgNestConfig if it's a test).");
        }

        return Math.Max(result, this.EliteSurvivors);
      }
    }

    public bool IsEmpty => this.items.Count == 0;

    public IEnumerator<INestResult> GetEnumerator()
    {
      return this.items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.items.GetEnumerator();
    }

    public int IndexOf(INestResult nestResult)
    {
      return this.items.IndexOf(nestResult);
    }

    internal static bool IsANovelNest(double payload, double incumbent, int index, double topDiversity)
    {
      if (index == 0)
      {
        return Math.Round(incumbent, 2) != Math.Round(payload, 2);
      }

      return Math.Abs(incumbent - payload) > (incumbent * topDiversity);
    }

    internal void Clear()
    {
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.Clear());
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
      TryAddResult result = TryAddResult.NotAdded;
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => result = this.TryAdd(payload));
      }
      else
      {
        lock (lockItemsObject)
        {
          if (this.items.Count == 0)
          {
            this.items.Insert(0, payload);
            result = TryAddResult.Added;
          }
          else
          {
            int i = 0;
            while (i < this.items.Count && this.items[i].Fitness < payload.Fitness)
            {
              i++;
            }

            if (i == this.items.Count)
            {
              if (this.items.Count < this.MaxCapacity)
              {
                this.items.Add(payload);
                result = TryAddResult.Added;
              }
            }
            else if (!IsANovelNest(payload.Fitness, this.items[i].Fitness, i, this.config.TopDiversity))
            {
              // Duplicate - respond true so the TryAdd consumer can report duplicate as
              // it won't find the result in the list
              result = TryAddResult.Duplicate;
            }
            else
            {
              this.items.Insert(i, payload);
              result = TryAddResult.Added;
            }
          }

          if (this.items.Count > this.MaxCapacity)
          {
            this.items.RemoveAt(this.items.Count - 1);
          }
        }
      }

      return result;
    }
  }
}
