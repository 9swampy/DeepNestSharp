namespace DeepNestSharp.Ui.Models
{
  using System;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservablePropertyObject : ObservableObject
  {
    protected void SetProperty<T>(string? propertyName, Func<T> getProp, Action<T> setProp, T value)
    {
      if ((value == null && getProp() != null) || (value != null && value.Equals(getProp())))
      {
        setProp(value);
        OnPropertyChanged(propertyName);
      }
    }
  }
}
