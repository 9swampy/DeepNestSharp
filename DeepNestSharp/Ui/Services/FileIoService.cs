namespace DeepNestSharp.Ui.Services
{
  using System.IO;
  using DeepNestSharp.Domain;
  using Microsoft.Win32;
  using static System.Net.WebRequestMethods;

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

    public string GetSaveFilePath(string fileDialogFilter)
    {
      var dlg = new SaveFileDialog()
      {
        Filter = fileDialogFilter,
      };

      var response = dlg.ShowDialog();
      if (response.HasValue && response.Value)
      {
        return dlg.FileName;
      }

      return string.Empty;
    }
  }
}