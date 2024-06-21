namespace DeepNestSharp.Domain.Models
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Threading.Tasks;
  using CommunityToolkit.Mvvm.ComponentModel;
  using DeepNestLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;

  public class ObservableSheetPlacement : ObservableObject, ISheetPlacement, IWrapper<ISheetPlacement, SheetPlacement>
  {
    private readonly SheetPlacement sheetPlacement;
    private readonly ObservableCollection<ObservablePartPlacement> observablePartPlacements;

    private ObservableSheetPlacement() => this.observablePartPlacements = new ObservableCollection<ObservablePartPlacement>();

    public ObservableSheetPlacement(SheetPlacement sheetPlacement)
      : this()
    {
      this.sheetPlacement = sheetPlacement;
      this.Set(sheetPlacement);
    }

    public double X => this.Sheet.X;

    public double Y => this.Sheet.Y;

    public bool IsSet => this.sheetPlacement != null;

    private void Set(ISheetPlacement item)
    {
      this.observablePartPlacements.Clear();
      // this.points?.Clear();
      var order = 0;
      foreach (IPartPlacement partPlacement in item.PartPlacements)
      {
        ObservablePartPlacement obsPart = new ObservablePartPlacement(partPlacement, order);
        order++;
        obsPart.PropertyChanged += this.ObsPart_PropertyChanged;
        this.observablePartPlacements.Add(obsPart);
      }

      this.OnPropertyChanged(nameof(this.PartPlacements));
      this.OnPropertyChanged(nameof(this.IsSet));
      this.OnPropertyChanged(nameof(this.Sheet));
    }

    private void ObsPart_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender is ObservablePartPlacement obsPart &&
          !obsPart.IsDragging &&
          (e.PropertyName == nameof(ObservablePartPlacement.X) || e.PropertyName == nameof(ObservablePartPlacement.Y)))
      {
        if (this.sheetPlacement != this.sheetPlacement)
        {
          this.Set(this.sheetPlacement);
        }

        this.OnPropertyChanged(nameof(this.PartPlacements));
      }
    }

    public OriginalFitnessSheet Fitness => this.sheetPlacement.Fitness;

    public INfp Hull => this.sheetPlacement.Hull;

    public bool IsDirty => true;

    public SheetPlacement Item => this.sheetPlacement;

    public double MaxX => this.sheetPlacement?.MaxX ?? this.MinX;

    public double MaxY => this.sheetPlacement?.MaxY ?? this.MinY;

    public double MaterialUtilization => this.sheetPlacement?.MaterialUtilization ?? 0;

    public double MergedLength => ((ISheetPlacement)this.Item).MergedLength;

    public double MinX => this.sheetPlacement?.MinX ?? 0;

    public double MinY => this.sheetPlacement?.MinY ?? 0;

    public IReadOnlyList<IPartPlacement> PartPlacements => this.observablePartPlacements;

    public PlacementTypeEnum PlacementType => this.sheetPlacement?.PlacementType ?? PlacementTypeEnum.Gravity;

    public PolygonBounds RectBounds => this.sheetPlacement.RectBounds;

    public ISheet Sheet => this.sheetPlacement?.Sheet;

    public ISheet OriginalSheet => this.sheetPlacement?.OriginalSheet;

    public int SheetId => this.sheetPlacement.SheetId;

    public int SheetSource => this.sheetPlacement.SheetSource;

    public INfp Simplify => this.sheetPlacement.Simplify;

    public double TotalPartsArea => this.sheetPlacement.TotalPartsArea;

    public IEnumerable<NoFitPolygon> PolygonsForExport => this.sheetPlacement.PolygonsForExport;

    public string ToJson(bool writeIndented = false)
    {
      return this.sheetPlacement.ToJson(writeIndented);
    }

    public void SaveState()
    {
      /*Havn't coded yet... but let's not throw just continue IsDirty.
      observablePartPlacements[0].SaveState();*/
    }

    public async Task ExportDxf(Stream stream, bool mergeLines, bool differentiateChildren)
    {
      await this.sheetPlacement.ExportDxf(stream, mergeLines, differentiateChildren);
    }
  }
}