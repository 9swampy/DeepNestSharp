namespace DeepNestSharp.Domain.Docking
{
  using System.ComponentModel;
  using System.Windows.Input;

  public interface IFileViewModel : INotifyPropertyChanged
  {
    ICommand CloseCommand { get; }

    string FileDialogFilter { get; }

    /// <summary>
    /// Gets name of the file (with extension but without path).
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Gets or sets the full path to the file.
    /// </summary>
    string FilePath { get; set; }

    string DirectoryName { get; }

    string FileTypeName { get; }

    bool IsDirty { get; set; }

    ICommand SaveAsCommand { get; }

    ICommand SaveCommand { get; }

    string TextContent { get; }
  }
}