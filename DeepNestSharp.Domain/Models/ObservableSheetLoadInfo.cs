namespace DeepNestSharp.Domain.Models
{
  using System;
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
      get => sheetLoadInfo.Height;
      set => SetProperty(nameof(Height), () => sheetLoadInfo.Height, v => sheetLoadInfo.Height = v, value);
    }

    public override bool IsDirty => true;

    public string Path
    {
      get => sheetLoadInfo.Path;
      set => SetProperty(nameof(Path), () => sheetLoadInfo.Path, v => sheetLoadInfo.Path = v, value);
    }

    public int Quantity
    {
      get => sheetLoadInfo.Quantity;
      set => SetProperty(nameof(Quantity), () => sheetLoadInfo.Quantity, v => sheetLoadInfo.Quantity = v, value);
    }

    public SheetTypeEnum SheetType
    {
      get => sheetLoadInfo.SheetType;
      set => SetProperty(nameof(SheetType), () => sheetLoadInfo.SheetType, v => sheetLoadInfo.SheetType = v, value);
    }

    public double Width
    {
      get => sheetLoadInfo.Width;
      set => SetProperty(nameof(Width), () => sheetLoadInfo.Width, v => sheetLoadInfo.Width = v, value);
    }

    public int NetArea
    {
      get
      {
        if (netArea == null)
        {
          this.netArea = (int)this.Nfp.NetArea;
        }

        return netArea.Value;
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
          if (sheetLoadInfo.SheetType == SheetTypeEnum.Arbitrary)
          {
            var raw = await DxfParser.LoadDxfFile(sheetLoadInfo.Path);
            this.nfp = raw.ToNfp();
          }
          else
          {
            Sheet rectSheet = Sheet.NewSheet(0, sheetLoadInfo.Width, sheetLoadInfo.Height);
            this.nfp = rectSheet;
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
