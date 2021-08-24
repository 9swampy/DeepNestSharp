namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Ui.Docking;
  using System.ComponentModel;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows.Input;

  public interface INestMonitorViewModel : IToolViewModel, INotifyPropertyChanged
  {
    ICommand ContinueNestCommand { get; }
    bool IsRunning { get; }
    bool IsSecondaryProgressVisible { get; }
    bool IsStopping { get; }
    string LastLogMessage { get; }
    ICommand LoadNestResultCommand { get; }
    ICommand LoadSheetPlacementCommand { get; }
    string MessageLog { get; }
    StringBuilder MessageLogBuilder { get; }
    double Progress { get; }
    double ProgressSecondary { get; }
    ICommand RestartNestCommand { get; }
    int SelectedIndex { get; set; }
    INestResult? SelectedItem { get; set; }
    INestState State { get; }
    ICommand StopNestCommand { get; }
    TopNestResultsCollection TopNestResults { get; }
    IZoomPreviewDrawingContext ZoomDrawingContext { get; }
    bool IsActive { get; set; }

    void Stop();
    Task<bool> TryStartAsync(INestProjectViewModel nestProjectViewModel);
  }
}