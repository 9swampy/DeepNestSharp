namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestSharp.Ui.Docking;

  public class NestMonitorViewModel : ToolViewModel
  {
    private readonly MainViewModel mainViewModel;

    public NestMonitorViewModel(MainViewModel mainViewModel)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
    }
  }
}