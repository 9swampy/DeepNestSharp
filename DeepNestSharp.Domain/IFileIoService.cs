namespace DeepNestSharp.Domain
{
  public interface IFileIoService
  {
    string GetFilePath(string filter);

    bool Exists(string filePath);
  }
}