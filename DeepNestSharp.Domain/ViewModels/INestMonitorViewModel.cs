namespace DeepNestSharp.Domain.ViewModels
{
  using System.ComponentModel;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;

  public interface INestMonitorViewModel : IToolViewModel, INotifyPropertyChanged
  {
    ICommand ContinueNestCommand { get; }

    bool IsRunning { get; }

    bool IsSecondaryProgressVisible { get; set; }

    bool IsStopping { get; }

    string LastLogMessage { get; set; }

    ICommand LoadNestResultCommand { get; }

    ICommand LoadSheetPlacementCommand { get; }

    string MessageLog { get; }

    StringBuilder MessageLogBuilder { get; }

    double Progress { get; set; }

    double ProgressSecondary { get; set; }

    ICommand RestartNestCommand { get; }

    int SelectedIndex { get; set; }

    INestResult SelectedItem { get; set; }

    INestState State { get; }

    ICommand StopNestCommand { get; }

    TopNestResultsCollection TopNestResults { get; }

    IZoomPreviewDrawingContext ZoomDrawingContext { get; }

    void Stop();

    Task<bool> TryStartAsync(INestProjectViewModel nestProjectViewModel);

    void UpdateNestsList();
  }
}