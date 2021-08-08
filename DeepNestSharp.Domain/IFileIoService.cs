namespace DeepNestSharp.Domain
{
  public interface IFileIoService
  {
    string GetOpenFilePath(string filter);

    bool Exists(string filePath);

    string GetSaveFilePath(string fileDialogFilter, string fileName = null, string initialDirectory = null);
  }
}