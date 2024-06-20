namespace DeepNestLib.IO
{
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;
  using DeepNestLib.GeneticAlgorithm;

  public class RawDetail<TSourceEntity> : IRawDetail
  {
    private List<LocalContour<TSourceEntity>> outers = new List<LocalContour<TSourceEntity>>();

    public IReadOnlyCollection<ILocalContour> Outers => this.outers;

    public RectangleF BoundingBox()
    {
      GraphicsPath gp = new GraphicsPath();
      foreach (ILocalContour item in this.Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      return gp.GetBounds();
    }

    public void AddContour(LocalContour<TSourceEntity> contour)
    {
      this.outers.Add(contour);
    }

    public void AddRangeContour(IEnumerable<LocalContour<TSourceEntity>> collection)
    {
      this.outers.AddRange(collection);
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
      var result = this.TryConvertToNfp(src, out loadedNfp);
      loadedChromosome = new Chromosome(loadedNfp, loadedNfp?.Rotation ?? 0);
      return result;
    }

    bool IRawDetail.TryConvertToNfp(int src, int rotation, out Chromosome loadedChromosome)
    {
      var result = this.TryConvertToNfp(src, out loadedChromosome);
      loadedChromosome.Rotation = rotation;
      return result;
    }

    public (INfp, double) ToChromosome()
    {
      INfp nfp = this.ToNfp();
      return (nfp, nfp.Rotation);
    }

    public INfp ToNfp()
    {
      NoFitPolygon result = null;
      List<NoFitPolygon> nfps = new List<NoFitPolygon>();
      foreach (ILocalContour item in this.Outers)
      {
        NoFitPolygon nn = new NoFitPolygon();
        nfps.Add(nn);
        foreach (PointF pitem in item.Points)
        {
          nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
        }
      }

      if (nfps.Any())
      {
        NoFitPolygon parent = nfps.OrderByDescending(z => z.Area).First();
        result = parent; // Reference caution needed here; should be cloning not messing with the original object?
        result.Name = this.Name;

        foreach (NoFitPolygon child in nfps.Where(o => o != parent))
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

    public ISheet ToSheet()
    {
      return new Sheet(this.ToNfp(), WithChildren.Excluded);
    }

    bool IRawDetail.TryConvertToSheet(int firstSheetIdSrc, out ISheet firstSheet)
    {
      INfp nfp;
      if (this.TryConvertToNfp(firstSheetIdSrc, out nfp))
      {
        firstSheet = new Sheet(nfp, WithChildren.Excluded);
        return true;
      }

      firstSheet = default;
      return false;
    }
  }
}