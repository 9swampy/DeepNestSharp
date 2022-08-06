namespace DeepNestLib.IO
{
  using System.Collections.Generic;

  public static class ExporterFactory
  {
    public static IExport GetExporter(ICollection<INfp> polygons)
    {
      IExport exporter;
      if (polygons.ContainsDxfs())
      {
        exporter = new DxfExporter();
      }
      else
      {
        exporter = new SvgExporter();
      }

      return exporter;
    }
  }
}
