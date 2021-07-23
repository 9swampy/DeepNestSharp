namespace DeepNestLib
{
  using System.Collections.Generic;

  public static class ExporterFactory
  {
    public static IExport GetExporter(ICollection<NFP> polygons, ISvgNestConfig config)
    {
      IExport exporter;
      if (polygons.ContainsDxfs())
      {
        exporter = new DxfParser();
      }
      else
      {
        exporter = new SvgParser(config);
      }

      return exporter;
    }
  }
}
