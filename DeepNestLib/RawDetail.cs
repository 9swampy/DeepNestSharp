namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;

  public class RawDetail
  {
    private List<LocalContour> outers = new List<LocalContour>();

    public IReadOnlyCollection<LocalContour> Outers => outers;

    public RectangleF BoundingBox()
    {
      GraphicsPath gp = new GraphicsPath();
      foreach (var item in Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      return gp.GetBounds();
    }

    public void AddContour(LocalContour contour)
    {
      outers.Add(contour);
    }

    public void AddRangeContour(IEnumerable<LocalContour> collection)
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

    public INfp ToNfp()
    {
      NFP result = null;
      List<NFP> nfps = new List<NFP>();
      foreach (var item in this.Outers)
      {
        var nn = new NFP();
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
      return new Sheet(this.ToNfp(), width, height);
    }

    public ISheet ToSheet()
    {
      return new Sheet(this.ToNfp());
    }

    internal bool TryConvertToSheet(int firstSheetIdSrc, out ISheet firstSheet)
    {
      INfp nfp;
      if (TryConvertToNfp(firstSheetIdSrc, out nfp))
      {
        firstSheet = new Sheet(nfp);
        return true;
      }

      firstSheet = default;
      return false;
    }
  }
}