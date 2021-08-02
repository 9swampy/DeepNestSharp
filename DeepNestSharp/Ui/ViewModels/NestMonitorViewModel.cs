namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Ui.Docking;
  using System;
  using System.Windows;

  public class NestMonitorViewModel : ToolViewModel
  {
    private readonly MainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private INestProjectViewModel nestProjectViewModel;
    private bool isRunning;
    private static volatile object syncLock = new object();
    private NestExecutionHelper nestExecutionHelper = new NestExecutionHelper();
    private NestingContext context;
    private readonly IProgressDisplayer progressDisplayer;

    public NestMonitorViewModel(MainViewModel mainViewModel, IMessageService messageService, IProgressDisplayer progressDisplayer)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
    }

    private NestingContext Context
    {
      get
      {
        lock (syncLock)
        {
          if (this.context == null)
          {
            this.context = new NestingContext(new MessageBoxService(), progressDisplayer);
          }
        }

        return this.context;
      }
    }

    public bool IsRunning
    {
      get
      {
        return this.isRunning;
      }

      private set
      {
        SetProperty(ref this.isRunning, value);
      }
    }

    public bool TryStart(INestProjectViewModel nestProjectViewModel)
    {
      lock (syncLock)
      {
        if (this.isRunning)
        {
          return false;
        }

        this.nestProjectViewModel = nestProjectViewModel;
        this.nestExecutionHelper.RebuildNest(
                          this.Context,
                          nestProjectViewModel.ProjectInfo.SheetLoadInfos,
                          nestProjectViewModel.ProjectInfo.DetailLoadInfos,
                          new ProgressDisplayer(messageService));

        this.isRunning = true;
        return true;
      }
    }
  }
}