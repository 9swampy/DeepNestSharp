namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;

  public class RawDetail<TSourceEntity> : IRawDetail
  {
    private List<LocalContour<TSourceEntity>> outers = new List<LocalContour<TSourceEntity>>();

    public IReadOnlyCollection<ILocalContour> Outers => outers;

    public RectangleF BoundingBox()
    {
      GraphicsPath gp = new GraphicsPath();
      foreach (var item in Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      return gp.GetBounds();
    }

    public void AddContour(LocalContour<TSourceEntity> contour)
    {
      outers.Add(contour);
    }

    public void AddRangeContour(IEnumerable<LocalContour<TSourceEntity>> collection)
    {
      outers.AddRange(collection);
    }

    public string Name { get; set; }

    public bool TryConvertToNfp(int src, out INfp loadedNfp)
    {
      if (this == null)
      {
        loadedNfp = null;
        return false;
      }

      loadedNfp = this.ToNfp();
      if (loadedNfp == null)
      {
        return false;
      }

      loadedNfp.Source = src;
      return true;
    }

    public bool TryConvertToNfp(int src, out Chromosome loadedChromosome)
    {
      INfp loadedNfp;
      var result = TryConvertToNfp(src, out loadedNfp);
      loadedChromosome = new Chromosome(loadedNfp, loadedNfp?.Rotation ?? 0);
      return result;
    }

    bool IRawDetail.TryConvertToNfp(int src, int rotation, out Chromosome loadedChromosome)
    {
      var result = TryConvertToNfp(src, out loadedChromosome);
      loadedChromosome.Rotation = rotation;
      loadedChromosome.Part.Rotation = rotation;
      return result;
    }

    public (INfp, double) ToChromosome()
    {
      INfp nfp = ToNfp();
      return (nfp, nfp.Rotation);
    }

    public INfp ToNfp()
    {
      NoFitPolygon result = null;
      List<NoFitPolygon> nfps = new List<NoFitPolygon>();
      foreach (var item in this.Outers)
      {
        var nn = new NoFitPolygon();
        nfps.Add(nn);
        foreach (var pitem in item.Points)
        {
          nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
        }
      }

      if (nfps.Any())
      {
        var parent = nfps.OrderByDescending(z => z.Area).First();
        result = parent; // Reference caution needed here; should be cloning not messing with the original object?
        result.Name = Name;

        foreach (var child in nfps.Where(o => o != parent))
        {
          if (result.Children == null)
          {
            result.Children = new List<INfp>();
          }

          result.Children.Add(child);
        }
      }

      return result;
    }

    public ISheet ToSheet(double width, double height)
    {
      return new Sheet(this.ToNfp(), width, height, WithChildren.Excluded);
    }

    public ISheet ToSheet()
    {
      return new Sheet(this.ToNfp(), WithChildren.Excluded);
    }

    bool IRawDetail.TryConvertToSheet(int firstSheetIdSrc, out ISheet firstSheet)
    {
      INfp nfp;
      if (TryConvertToNfp(firstSheetIdSrc, out nfp))
      {
        firstSheet = new Sheet(nfp, WithChildren.Excluded);
        return true;
      }

      firstSheet = default;
      return false;
    }
  }
}