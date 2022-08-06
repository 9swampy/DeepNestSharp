namespace DeepNestLib.IO
{
  using System.Collections.Generic;
  using System.Drawing;

  public interface ILocalContour
  {
    List<PointF> Points { get; }
    bool IsChild { get; }
  }
}