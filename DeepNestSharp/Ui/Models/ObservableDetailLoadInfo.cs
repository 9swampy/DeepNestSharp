namespace DeepNestSharp.Ui.Models
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.NestProject;

  public class ObservableDetailLoadInfo : ObservablePropertyObject, IDetailLoadInfo
  {
    private readonly IDetailLoadInfo detailLoadInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDetailLoadInfo"/> class.
    /// </summary>
    /// <param name="sheetLoadInfo">The ProjectInfo to wrap.</param>
    public ObservableDetailLoadInfo(IDetailLoadInfo detailLoadInfo) => this.detailLoadInfo = detailLoadInfo;

    public bool IsIncluded
    {
      get => detailLoadInfo.IsIncluded;
      set => SetProperty(nameof(IsIncluded), () => detailLoadInfo.IsIncluded, v => detailLoadInfo.IsIncluded = v, value);
    }

    public bool IsMultiplied
    {
      get => detailLoadInfo.IsMultiplied;
      set => SetProperty(nameof(IsMultiplied), () => detailLoadInfo.IsMultiplied, v => detailLoadInfo.IsMultiplied = v, value);
    }

    public bool IsPriority
    {
      get => detailLoadInfo.IsPriority;
      set => SetProperty(nameof(IsPriority), () => detailLoadInfo.IsPriority, v => detailLoadInfo.IsPriority = v, value);
    }

    public string Name
    {
      get => detailLoadInfo.Name;
      set => SetProperty(nameof(Name), () => detailLoadInfo.Name, v => detailLoadInfo.Name = v, value);
    }

    public string Path
    {
      get => detailLoadInfo.Path;
      set => SetProperty(nameof(Path), () => detailLoadInfo.Path, v => detailLoadInfo.Path = v, value);
    }

    public int Quantity
    {
      get => detailLoadInfo.Quantity;
      set => SetProperty(nameof(Quantity), () => detailLoadInfo.Quantity, v => detailLoadInfo.Quantity = v, value);
    }

    public AnglesEnum StrictAngle
    {
      get => detailLoadInfo.StrictAngle;
      set => SetProperty(nameof(StrictAngle), () => detailLoadInfo.StrictAngle, v => detailLoadInfo.StrictAngle = v, value);
    }

    public IList<AnglesEnum> AnglesList => Enum.GetValues(typeof(AnglesEnum)).OfType<AnglesEnum>().ToList();
  }
}
