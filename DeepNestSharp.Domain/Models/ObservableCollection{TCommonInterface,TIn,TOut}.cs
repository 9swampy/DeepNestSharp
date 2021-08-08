[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DeepNestSharp.CiTests")]

namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib.NestProject;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using Light.GuardClauses;
  using System.Linq;

  public class ObservableCollection<TCommonInterface, TIn, TOut> : ObservableCollection<TOut>, IList<TCommonInterface, TIn>
    where TIn : class, TCommonInterface
    where TOut : class, TCommonInterface, IWrapper<TCommonInterface, TIn>
  {
    private readonly Func<TIn, TOut> factory;
    protected internal IList<TCommonInterface> ItemsWrapped { get; }

    public bool IsReadOnly => false;

    TCommonInterface IList<TCommonInterface>.this[int index]
    {
      get => base[index];
      set
      {
        if (value is TOut tOut)
        {
          base[index] = tOut;
          this.ItemsWrapped[index] = tOut.Item;
        }
        else if (value is TIn tIn)
        {
          base[index] = factory(tIn);
          this.ItemsWrapped[index] = tIn;
        }
        else
        {
          throw new ArgumentException();
        }
      }
    }

    public ObservableCollection(Func<TIn, TOut> factory)
      : this(new WrappableList<TCommonInterface, TIn>(), factory)
    {
    }

    public ObservableCollection(IList<TCommonInterface, TIn> items, Func<TIn, TOut> factory)
    {
      this.ItemsWrapped = items;
      this.factory = factory;
      foreach (var item in items.ToList())
      {
        this.Add(factory((TIn)item));
      }
    }

    public void Add(TIn item)
    {
      this.ItemsWrapped.Add(item);
      this.Add(factory(item));
    }

    public int IndexOf(TCommonInterface item)
    {
      if (item is TIn tIn)
      {
        return this.ItemsWrapped.IndexOf(tIn);
      }
      else if (item is TOut tOut)
      {
        return base.IndexOf(tOut);
      }

      return -1;
    }

    public void Insert(int index, TCommonInterface item)
    {
      item.MustBeOfType<TIn>();
      ItemsWrapped.Insert(index, item);
      base.Insert(index, factory((TIn)item));
    }

    public void Add(TCommonInterface item)
    {
      item.MustBeOfType<TIn>();
      Add((TIn)item);
    }

    private void Add(TOut item)
    {
      base.Add(item);
    }

    public bool Contains(TCommonInterface item)
    {
      if (item is TIn tIn)
      {
        return this.ItemsWrapped.Contains(tIn);
      }
      else if (item is TOut tOut)
      {
        return base.Contains(tOut);
      }

      return false;
    }

    public void CopyTo(TCommonInterface[] array, int arrayIndex)
    {
      this.Items.ToArray<TCommonInterface>().CopyTo(array, arrayIndex);
    }

    public bool Remove(TCommonInterface item)
    {
      if (item is TIn tIn)
      {
        var tOut = base.Items.FirstOrDefault(o => o.Item == tIn);
        return this.Remove(tOut);
      }
      else if (item is TOut tOut)
      {
        this.ItemsWrapped.Remove(tOut.Item);
        return base.Remove(tOut);
      }

      return false;
    }

    IEnumerator<TCommonInterface> IEnumerable<TCommonInterface>.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }
}
