namespace DeepNestSharp.Domain.Docking
{
  using System.ComponentModel;
  using System.Windows.Input;

  public interface IFileViewModel : INotifyPropertyChanged
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