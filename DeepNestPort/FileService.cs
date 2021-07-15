namespace DeepNestPort
{
  using System.IO;
  using DeepNestLib;

  public class FileService
  {
    public static FileService Default = new FileService();

    public RawDetail LoadRawDetail(FileInfo f)
    {
      RawDetail det = null;
      if (f.Extension == ".svg")
      {
        det = SvgParser.LoadSvg(f.FullName);
      }

      if (f.Extension == ".dxf")
      {
        det = DxfParser.LoadDxfFile(f.FullName);
      }

      return det;
    }
  }
}
