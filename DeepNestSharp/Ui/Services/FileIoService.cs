namespace DeepNestSharp.Ui.Services
{
  using System.IO;
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
      OpenFileDialog openFileDialog = new OpenFileDialog()
      {
        Filter = filter,
      };

      if (openFileDialog.ShowDialog() == true)
      {
        return openFileDialog.FileName;
      }

      return string.Empty;
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