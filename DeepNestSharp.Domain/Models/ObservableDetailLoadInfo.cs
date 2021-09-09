namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestLib.NestProject;

  public class ObservableDetailLoadInfo : ObservablePropertyObject, IWrapper<IDetailLoadInfo, DetailLoadInfo>, IDetailLoadInfo
  {
    private readonly DetailLoadInfo detailLoadInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDetailLoadInfo"/> class.
    /// </summary>
    /// <param name="sheetLoadInfo">The ProjectInfo to wrap.</param>
    public ObservableDetailLoadInfo(DetailLoadInfo detailLoadInfo)
    {
      this.detailLoadInfo = detailLoadInfo;
      this.PropertyChanged += this.ObservableDetailLoadInfo_PropertyChanged;
    }

    private void ObservableDetailLoadInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName != nameof(IsDirty))
      {
        OnPropertyChanged(nameof(IsDirty));
      }
    }

    public IList<AnglesEnum> AnglesList => Enum.GetValues(typeof(AnglesEnum)).OfType<AnglesEnum>().ToList();

    public override bool IsDirty => this.detailLoadInfo.IsDirty;

    public bool IsExists => this.detailLoadInfo.IsExists;

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

    public DetailLoadInfo Item => detailLoadInfo;

    public async Task<INfp> LoadAsync()
    {
      if (new FileInfo(detailLoadInfo.Path).Exists)
      {
        var raw = await DxfParser.LoadDxfFile(detailLoadInfo.Path);
        return raw.ToNfp();
      }

      return new NoFitPolygon();
    }
  }
}
