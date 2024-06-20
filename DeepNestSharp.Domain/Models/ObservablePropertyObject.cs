namespace DeepNestSharp.Domain.Models
{
  using System;
  using CommunityToolkit.Mvvm.ComponentModel;

  public abstract class ObservablePropertyObject : ObservableObject
  {
    public abstract bool IsDirty { get; }

    protected void SetProperty<T>(string propertyName, Func<T> getProp, Action<T> setProp, T value)
    {
      try
      {
        if ((value == null && getProp() != null) || (value != null && !value.Equals(getProp())))
        {
          setProp(value);
          this.OnPropertyChanged(propertyName);
          this.OnPropertyChanged("IsDirty");
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
        throw;
      }
    }
  }
}
