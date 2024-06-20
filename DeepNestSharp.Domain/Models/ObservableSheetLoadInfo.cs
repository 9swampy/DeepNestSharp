namespace DeepNestSharp.Domain.Models
{
  using System;
  using System.ComponentModel;
  using System.IO;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using Light.GuardClauses;
  using Microsoft.VisualStudio.Threading;

  public class ObservableSheetLoadInfo : ObservablePropertyObject, IWrapper<ISheetLoadInfo, SheetLoadInfo>, ISheetLoadInfo
  {
    private readonly SheetLoadInfo sheetLoadInfo;
    private int? netArea;
    private INfp nfp;

    private static JoinableTaskContext joinableTaskContext = new JoinableTaskContext();

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableSheetLoadInfo"/> class.
    /// </summary>
    /// <param name="sheetLoadInfo">The ProjectInfo to wrap.</param>
    public ObservableSheetLoadInfo(SheetLoadInfo sheetLoadInfo) => this.sheetLoadInfo = sheetLoadInfo;

    public double Height
    {
      get => this.sheetLoadInfo.Height;
      set
      {
        this.nfp = null; // Is this hacky?
        this.SetProperty(nameof(this.Height), () => this.sheetLoadInfo.Height, v => this.sheetLoadInfo.Height = v, value);
      }
    }

    public override bool IsDirty => true;

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

    public string Path
    {
      get => this.sheetLoadInfo.Path;
      set => this.SetProperty(nameof(this.Path), () => this.sheetLoadInfo.Path, v => this.sheetLoadInfo.Path = v, value);
    }

    public int Quantity
    {
      get => this.sheetLoadInfo.Quantity;
      set => this.SetProperty(nameof(this.Quantity), () => this.sheetLoadInfo.Quantity, v => this.sheetLoadInfo.Quantity = v, value);
    }

    public SheetTypeEnum SheetType
    {
      get => this.sheetLoadInfo.SheetType;
    }

    public double Width
    {
      get => this.sheetLoadInfo.Width;
      set
      {
        this.nfp = null; // Is this hacky?
        this.SetProperty(nameof(this.Width), () => this.sheetLoadInfo.Width, v => this.sheetLoadInfo.Width = v, value);
      }
    }

    public SheetLoadInfo Item => this.sheetLoadInfo;

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
          if (this.sheetLoadInfo.SheetType == SheetTypeEnum.Arbitrary)
          {
            IRawDetail raw = await DxfParser.LoadDxfFile(this.sheetLoadInfo.Path);
            this.nfp = raw.ToNfp();
          }
          else
          {
            Sheet rectSheet = Sheet.NewSheet(0, this.sheetLoadInfo.Width, this.sheetLoadInfo.Height);
            this.nfp = rectSheet;
          }
        }
      }
      catch
      {
        this.nfp = new InvalidNoFitPolygon();
      }

      this.netArea = null; // Is this hacky?
      return this.nfp;
    }
  }
}
