namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using Microsoft.VisualStudio.Threading;

  public class ObservableDetailLoadInfo : ObservablePropertyObject, IWrapper<IDetailLoadInfo, DetailLoadInfo>, IDetailLoadInfo
  {
    private readonly DetailLoadInfo detailLoadInfo;
    private int? netArea;
    private INfp nfp;

    private static JoinableTaskContext joinableTaskContext = new JoinableTaskContext();

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
      if (e.PropertyName != nameof(this.IsDirty))
      {
        this.OnPropertyChanged(nameof(this.IsDirty));
      }
    }

    public IList<AnglesEnum> AnglesList => Enum.GetValues(typeof(AnglesEnum)).OfType<AnglesEnum>().ToList();

    public override bool IsDirty => this.detailLoadInfo.IsDirty;

    public bool IsValid => this.IsExists && !(this.Nfp is InvalidNoFitPolygon);

    public bool IsExists => this.detailLoadInfo.IsExists;

    public bool IsIncluded
    {
      get => this.detailLoadInfo.IsIncluded;
      set => this.SetProperty(nameof(this.IsIncluded), () => this.detailLoadInfo.IsIncluded, v => this.detailLoadInfo.IsIncluded = v, value);
    }

    public bool IsMultiplied
    {
      get => this.detailLoadInfo.IsMultiplied;
      set => this.SetProperty(nameof(this.IsMultiplied), () => this.detailLoadInfo.IsMultiplied, v => this.detailLoadInfo.IsMultiplied = v, value);
    }

    public bool IsPriority
    {
      get => this.detailLoadInfo.IsPriority;
      set => this.SetProperty(nameof(this.IsPriority), () => this.detailLoadInfo.IsPriority, v => this.detailLoadInfo.IsPriority = v, value);
    }

    public string Name
    {
      get => this.detailLoadInfo.Name;
    }

    public string Path
    {
      get => this.detailLoadInfo.Path;
      set => this.SetProperty(nameof(this.Path), () => this.detailLoadInfo.Path, v => this.detailLoadInfo.Path = v, value);
    }

    public int Quantity
    {
      get => this.detailLoadInfo.Quantity;
      set => this.SetProperty(nameof(this.Quantity), () => this.detailLoadInfo.Quantity, v => this.detailLoadInfo.Quantity = v, value);
    }

    public int NetArea
    {
      get
      {
        if (this.netArea == null)
        {
          this.netArea = (int)this.Nfp.NetArea;
        }

        return this.netArea.Value;
      }
    }

    public AnglesEnum StrictAngle
    {
      get => this.detailLoadInfo.StrictAngle;
      set => this.SetProperty(nameof(this.StrictAngle), () => this.detailLoadInfo.StrictAngle, v => this.detailLoadInfo.StrictAngle = v, value);
    }

    public DetailLoadInfo Item => this.detailLoadInfo;

    internal INfp Nfp
    {
      get
      {
        _ = joinableTaskContext.Factory.RunAsync(async () => this.netArea = (int)(await this.LoadAsync().ConfigureAwait(false)).NetArea);
        return this.nfp;
      }
    }

    public async Task<INfp> LoadAsync()
    {
      try
      {
        if (this.nfp == null)
        {
          if (new FileInfo(this.detailLoadInfo.Path).Exists)
          {
            IRawDetail raw = await DxfParser.LoadDxfFile(this.detailLoadInfo.Path);
            this.nfp = raw.ToNfp();
          }
          else
          {
            this.nfp = new NoFitPolygon();
          }
        }
      }
      catch
      {
        this.nfp = new InvalidNoFitPolygon();
      }

      return this.nfp;
    }
  }
}
