[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DeepNestSharp.CiTests")]

namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Linq;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using Light.GuardClauses;

  public class ObservableCollection<TCommonInterface, TIn, TOut> : ObservableCollection<TOut>, IList<TCommonInterface, TIn>, ISaveable, INotifyPropertyChanged
    where TIn : class, TCommonInterface, ISaveable
    where TOut : class, TCommonInterface, IWrapper<TCommonInterface, TIn>, INotifyPropertyChanged
  {
    private readonly Func<TIn, TOut> factory;
    private bool isCollectionChanged;

    public event EventHandler IsDirtyChanged;

    protected internal IList<TCommonInterface, TIn> ItemsWrapped { get; }

    public bool IsReadOnly => false;

    public bool IsDirty => isCollectionChanged || this.ItemsWrapped.Any(o => ((TIn)o).IsDirty);

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
          base[index] = CreateNotifyingWrapper(tIn);
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
        base.Add(CreateNotifyingWrapper((TIn)item));
      }
    }

    public void Add(TIn item)
    {
      this.ItemsWrapped.Add(item);
      base.Add(CreateNotifyingWrapper(item));
      isCollectionChanged = true;
    }

    public new void Clear()
    {
      this.ItemsWrapped.Clear();
      base.Clear();
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
      base.Insert(index, CreateNotifyingWrapper((TIn)item));
      isCollectionChanged = true;
    }

    public void Add(TCommonInterface item)
    {
      item.MustBeOfType<TIn>();
      Add((TIn)item);
      isCollectionChanged = true;
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
      bool result = false;
      if (item is TIn tIn)
      {
        var tOut = Items.FirstOrDefault(o => o.Item == tIn);
        result = this.Remove(tOut);
      }
      else if (item is TOut tOut)
      {
        this.ItemsWrapped.Remove(tOut.Item);
        result = base.Remove(tOut);
      }

      if (result)
      {
        isCollectionChanged = true;
      }

      return result;
    }

    IEnumerator<TCommonInterface> IEnumerable<TCommonInterface>.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void SaveState()
    {
      foreach (var item in ItemsWrapped.Cast<ISaveable>())
      {
        item.SaveState();
      }
    }

    private TOut CreateNotifyingWrapper(TIn tIn)
    {
      var result = factory(tIn);
      result.PropertyChanged += this.Result_PropertyChanged;
      return result;
    }

    private void Result_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      IsDirtyChanged?.Invoke(this, new EventArgs());
    }
  }
}
