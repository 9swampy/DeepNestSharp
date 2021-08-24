using System.Windows.Input;

namespace DeepNestSharp.Ui.Docking
{
  public interface IFileViewModel
  {
    ICommand CloseCommand { get; }
    string FileDialogFilter { get; }
    string FileName { get; }
    string FilePath { get; set; }
    string FileTypeName { get; }
    bool IsDirty { get; set; }
    ICommand SaveAsCommand { get; }
    ICommand SaveCommand { get; }
    string TextContent { get; }
  }
}