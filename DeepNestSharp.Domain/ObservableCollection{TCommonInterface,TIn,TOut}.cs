namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib.NestProject;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  public class ObservableCollection<TCommonInterface, TIn, TOut> : ObservableCollection<TCommonInterface>, IList<TCommonInterface, TIn>
    where TIn : class, TCommonInterface
    where TOut : class, TCommonInterface
  {
    private readonly Func<TIn, TOut> factory;
    public readonly IList<TCommonInterface> items;

    public ObservableCollection(IList<TCommonInterface> items, Func<TIn, TOut> factory)
    {
      this.items = items;
      this.factory = factory;
      foreach (var item in items)
      {
        this.Add(item);
      }
    }

    public void Add(TIn item)
    {
      this.items.Add(item);
      this.Add(factory(item));
    }
  }
}
