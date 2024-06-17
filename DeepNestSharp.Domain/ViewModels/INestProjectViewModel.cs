namespace DeepNestSharp.Domain.ViewModels
{
  using System.ComponentModel;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Docking;

  public interface INestProjectViewModel : INotifyPropertyChanged, IFileViewModel
  {
    IProjectInfo ProjectInfo { get; }

    IDetailLoadInfo SelectedDetailLoadInfo { get; set; }

    ISheetLoadInfo SelectedSheetLoadInfo { get; set; }

    int SelectedDetailLoadInfoIndex { get; set; }

    int SelectedSheetLoadInfoIndex { get; set; }

    bool UsePriority { get; }
  }
}