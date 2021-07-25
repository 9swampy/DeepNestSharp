namespace DeepNestSharp.Ui.Models
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableNfp : ObservableObject
  {
    private System.Windows.Media.PointCollection points;
    private readonly INfp? item;

    public ObservableNfp(INfp nfp)
    {
      this.DrawingContext = new ObservableCollection<object>();
      this.item = nfp;
      this.DrawingContext.Add(this);
    }

    public ObservableCollection<object> DrawingContext { get; }

    public System.Windows.Media.PointCollection Points
    {
      get
      {
        if (points == null)
        {
          points = new System.Windows.Media.PointCollection();
          INfp loadedNfp;

          //var fi = new FileInfo(partPlacement.Part.Name);
          //var baseDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
          //while (!fi.Exists && baseDir != null)
          //{
          //  fi = new FileInfo(Path.Join(baseDir.FullName, partPlacement.Part.Name));
          //  if (fi.Exists)
          //  {
          //    break;
          //  }
          //  else
          //  {
          //    baseDir = baseDir.Parent;
          //  }
          //}

          //if (fi.Exists && DxfParser.LoadDxfFile(fi.FullName).TryGetNfp(partPlacement.Part.Source, out loadedNfp))
          //{
          //  loadedNfp = loadedNfp.Rotate(partPlacement.Part.Rotation);
          //  loadedNfp.X = partPlacement.Part.X;
          //  loadedNfp.Y = partPlacement.Part.Y;
          //}
          //else
          //{
          loadedNfp = item;
          //}

          foreach (var p in loadedNfp.Points)
          {
            points.Add(new System.Windows.Point(p.X, p.Y));
          }
        }

        return points;
      }
    }
  }
}
