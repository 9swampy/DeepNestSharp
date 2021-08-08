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

    public string GetFilePath(string filter)
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
  }
}