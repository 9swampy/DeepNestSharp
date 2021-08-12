using System.Threading.Tasks;

namespace DeepNestSharp.Domain
{
  public interface IFileIoService
  {
    Task<string> GetOpenFilePathAsync(string filter);

    Task<string[]> GetOpenFilePathsAsync(string filter, bool allowMultiSelect = true);

    bool Exists(string filePath);

    string GetSaveFilePath(string fileDialogFilter, string fileName = null, string initialDirectory = null);
  }
}