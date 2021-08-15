namespace DeepNestSharp.Ui.Models
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;

  public class ObservableNestResult : ObservablePropertyObject, INestResult
  {
    private readonly ObservableCollection<ISheetPlacement, SheetPlacement, ObservableSheetPlacement> observableSheetPlacements;
    private INestResult? item;

    public ObservableNestResult()
    {
      this.observableSheetPlacements = new ObservableCollection<ISheetPlacement, SheetPlacement, ObservableSheetPlacement>(sp => new ObservableSheetPlacement(sp));
    }

    public ObservableNestResult(INestResult item)
      : this() => this.Set(item);

    public DateTime CreatedAt => this.item.CreatedAt;

    public double Fitness => this.item.Fitness;

    public double FitnessBounds => this.item.FitnessBounds;

    public double FitnessSheets => this.item.FitnessSheets;

    public double FitnessUnplaced => this.item.FitnessUnplaced;

    public override bool IsDirty => true;

    public bool IsValid => this.item.IsValid;

    public double MergedLength => this.item.MergedLength;

    public PlacementTypeEnum PlacementType => this.item.PlacementType;

    public long PlacePartTime => this.item.PlacePartTime;

    public double[] Rotation { get => this.item.Rotation; set => this.item.Rotation = value; }

    public IList<INfp> UnplacedParts => this.item.UnplacedParts;

    public IList<ISheetPlacement, SheetPlacement> UsedSheets => this.observableSheetPlacements;

    public int TotalPlacedCount => this.item.TotalPlacedCount;

    public int TotalPartsCount => this.item.TotalPartsCount;

    public double PartsPlacedPercent => this.item.PartsPlacedPercent;

    public double MaterialUtilization => this.item.MaterialUtilization;

    public double TotalSheetsArea => this.item.TotalSheetsArea;

    public double TotalPartsArea => this.item.TotalPartsArea;

    public int TotalParts => this.item.TotalParts;

    public string ToJson()
    {
      return this.item.ToJson();
    }

    private void Set(INestResult item)
    {
      this.item = item;
      this.observableSheetPlacements.Clear();
      foreach (var sheetPlacement in item.UsedSheets)
      {
        this.observableSheetPlacements.Add(sheetPlacement);
      }

      OnPropertyChanged(nameof(this.UsedSheets));
    }
  }
}