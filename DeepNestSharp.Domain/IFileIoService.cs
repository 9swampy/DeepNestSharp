namespace DeepNestSharp.Domain
{
  public interface IFileIoService
  {
    string GetOpenFilePath(string filter);

    string[] GetOpenFilePaths(string filter, bool allowMultiSelect = true);

    bool Exists(string filePath);

    string GetSaveFilePath(string fileDialogFilter, string fileName = null, string initialDirectory = null);
  }
}