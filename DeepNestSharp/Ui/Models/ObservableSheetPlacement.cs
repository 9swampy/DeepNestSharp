namespace DeepNestSharp.Ui.Models
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableSheetPlacement : ObservableObject, ISheetPlacement, IWrapper<ISheetPlacement, SheetPlacement>
  {
    private readonly SheetPlacement sheetPlacement;
    private readonly ObservableCollection<ObservablePartPlacement> observablePartPlacements;
    private System.Windows.Media.PointCollection points;

    private ObservableSheetPlacement()
    {
      this.observablePartPlacements = new ObservableCollection<ObservablePartPlacement>();
    }

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
      this.points?.Clear();
      foreach (var partPlacement in item.PartPlacements)
      {
        var obsPart = new ObservablePartPlacement(partPlacement);
        obsPart.PropertyChanged += this.ObsPart_PropertyChanged;
        this.observablePartPlacements.Add(obsPart);
      }

      OnPropertyChanged(nameof(PartPlacements));
      OnPropertyChanged(nameof(IsSet));
      OnPropertyChanged(nameof(Sheet));
    }

    private void ObsPart_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender is ObservablePartPlacement obsPart &&
          !obsPart.IsDragging &&
          (e.PropertyName == nameof(ObservablePartPlacement.X) || e.PropertyName == nameof(ObservablePartPlacement.Y)))
      {
        Set(sheetPlacement);
        OnPropertyChanged(nameof(PartPlacements));
      }
    }

    public OriginalFitnessSheet Fitness => sheetPlacement.Fitness;

    public INfp Hull => sheetPlacement.Hull;

    public double MaxX => this.sheetPlacement?.MaxX ?? MinX;

    public double MaxY => this.sheetPlacement?.MaxY ?? MinY;

    public double MaterialUtilization => sheetPlacement?.MaterialUtilization ?? 0;

    public double MinX => this.sheetPlacement?.MinX ?? 0;

    public double MinY => this.sheetPlacement?.MinY ?? 0;

    public IReadOnlyList<IPartPlacement> PartPlacements => this.observablePartPlacements;

    public PlacementTypeEnum PlacementType => sheetPlacement?.PlacementType ?? PlacementTypeEnum.Gravity;

    public System.Windows.Media.PointCollection Points
    {
      get
      {
        if (points == null || points.Count == 0)
        {
          points = new System.Windows.Media.PointCollection();
          foreach (var p in this.Sheet.Points)
          {
            points.Add(new System.Windows.Point(p.X, p.Y));
          }
        }

        return points;
      }
    }

    public PolygonBounds RectBounds => sheetPlacement.RectBounds;

    public ISheet Sheet => sheetPlacement?.Sheet;

    public int SheetId => sheetPlacement.SheetId;

    public int SheetSource => sheetPlacement.SheetSource;

    public INfp Simplify => sheetPlacement.Simplify;

    public double TotalPartsArea => sheetPlacement.TotalPartsArea;

    public bool IsDirty => true;

    public SheetPlacement Item => sheetPlacement;

    public string ToJson()
    {
      return this.sheetPlacement.ToJson();
    }

    internal void SaveState()
    {
      /*Havn't coded yet... but let's not throw just continue IsDirty.
      observablePartPlacements[0].SaveState();*/
    }
  }
}