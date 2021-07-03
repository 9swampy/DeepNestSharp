namespace DeepNestLib
{
  using System.Collections.Generic;

  public static class ExporterFactory
  {
    public static IExport GetExporter(List<NFP> polygons)
    {
      IExport exporter;
      if (polygons.ContainsDxfs())
      {
        exporter = new DxfParser();
      }
      else
      {
        exporter = new SvgParser();
      }

      return exporter;
    }
  }
}
