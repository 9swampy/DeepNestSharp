namespace DeepNestSharp.Ui.Services
{
  using System.IO;
  using System.Linq;
  using DeepNestSharp.Domain;
  using Microsoft.Win32;

  public class FileIoService : IFileIoService
  {
    public bool Exists(string filePath)
    {
      var fileInfo = new FileInfo(filePath);
      return fileInfo.Exists;
    }

    public string GetOpenFilePath(string filter)
    {
      return GetOpenFilePaths(filter, false).First();
    }

    public string[] GetOpenFilePaths(string filter, bool allowMultiSelect = true)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog()
      {
        Filter = filter,
        Multiselect = allowMultiSelect,
      };

      if (openFileDialog.ShowDialog() == true)
      {
        return openFileDialog.FileNames;
      }

      return new string[] { string.Empty };
    }

    public string GetSaveFilePath(string fileDialogFilter, string fileName = null, string initialDirectory = null)
    {
      var dlg = new SaveFileDialog()
      {
        Filter = fileDialogFilter,
      };

      if (!string.IsNullOrWhiteSpace(fileName))
      {
        dlg.FileName = fileName;
      }

      if (!string.IsNullOrWhiteSpace(initialDirectory))
      {
        dlg.InitialDirectory = initialDirectory;
      }

      var response = dlg.ShowDialog();
      if (response.HasValue && response.Value)
      {
        return dlg.FileName;
      }

      return string.Empty;
    }
  }
}