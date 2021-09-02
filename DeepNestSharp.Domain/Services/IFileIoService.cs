namespace DeepNestSharp.Domain.Services
{
  using System.Threading.Tasks;

  public interface IFileIoService
  {
    Task<string> GetOpenFilePathAsync(string filter, string initialDirectory = null);

    Task<string[]> GetOpenFilePathsAsync(string filter, string initialDirectory = null, bool allowMultiSelect = true);

    bool Exists(string filePath);

    string GetSaveFilePath(string fileDialogFilter, string fileName = null, string initialDirectory = null);
  }
}