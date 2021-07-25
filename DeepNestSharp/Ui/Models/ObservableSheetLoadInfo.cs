namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib.NestProject;

  public class ObservableSheetLoadInfo : ObservablePropertyObject, ISheetLoadInfo
  {
    private readonly ISheetLoadInfo sheetLoadInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableSheetLoadInfo"/> class.
    /// </summary>
    /// <param name="sheetLoadInfo">The ProjectInfo to wrap.</param>
    public ObservableSheetLoadInfo(ISheetLoadInfo sheetLoadInfo) => this.sheetLoadInfo = sheetLoadInfo;

    public int Height
    {
      get => sheetLoadInfo.Height;
      set => SetProperty(nameof(Height), () => sheetLoadInfo.Height, v => sheetLoadInfo.Height = v, value);
    }

    public int Quantity
    {
      get => sheetLoadInfo.Quantity;
      set => SetProperty(nameof(Quantity), () => sheetLoadInfo.Quantity, v => sheetLoadInfo.Quantity = v, value);
    }

    public int Width
    {
      get => sheetLoadInfo.Width;
      set => SetProperty(nameof(Width), () => sheetLoadInfo.Width, v => sheetLoadInfo.Width = v, value);
    }
  }
}
