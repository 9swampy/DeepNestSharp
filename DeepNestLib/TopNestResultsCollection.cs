namespace DeepNestLib
{
  using System.Collections;
  using System.Collections.Generic;
  using DeepNestLib.Placement;

  public class TopNestResultsCollection : IEnumerable<INestResult>
  {
    private readonly ISvgNestConfig config;
    private List<INestResult> items = new List<INestResult>();

    public TopNestResultsCollection(ISvgNestConfig config)
    {
      this.config = config;
    }

    public int Count => items.Count;

    public INestResult Top => items?[0];

    public void Add(INestResult payload)
    {
      if (items.Count == 0)
      {
        items.Insert(0, payload);
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
        }
        else if (items[i].Fitness != payload.Fitness)
        {
          items.Insert(i, payload);
        }

        if (items.Count > MaxCapacity)
        {
          items.RemoveAt(items.Count - 1);
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
        return config.PopulationSize * 2 / 10;
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
  }
}
