namespace DeepNestSharp.Domain
{
  using System.ComponentModel;
  using DeepNestLib.NestProject;

  public interface INestProjectViewModel : INotifyPropertyChanged
  {
    IProjectInfo ProjectInfo { get; }

    IDetailLoadInfo SelectedDetailLoadInfo { get; set; }

    int SelectedDetailLoadInfoIndex { get; set; }

    bool UsePriority { get; }
  }
}