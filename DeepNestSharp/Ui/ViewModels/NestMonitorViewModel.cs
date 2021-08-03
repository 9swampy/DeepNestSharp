namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestSharp.Ui.Docking;

  public class NestMonitorViewModel : ToolViewModel
  {
    private static volatile object syncLock = new object();

    private readonly MainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private readonly IProgressDisplayer progressDisplayer;
    private INestProjectViewModel? nestProjectViewModel;
    private bool isRunning;
    private bool isStopping;
    private NestExecutionHelper nestExecutionHelper = new NestExecutionHelper();
    private NestingContext? context;
    private NestWorker? nestWorker;
    private ConfiguredTaskAwaitable? nestWorkerConfiguredTaskAwaitable;
    private Task? nestWorkerTask;

    public NestMonitorViewModel(MainViewModel mainViewModel, IMessageService messageService)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.progressDisplayer = new ProgressDisplayer(this, messageService);
    }

    internal void UpdateNestsList()
    {
      throw new NotImplementedException();
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

    public bool IsStopping
    {
      get
      {
        return this.isStopping;
      }

      private set
      {
        SetProperty(ref this.isStopping, value);
      }
    }

    public string MessageLog
    {
      get
      {
        return this.MessageLogBuilder.ToString();
      }
    }

    public StringBuilder MessageLogBuilder { get; } = new StringBuilder();

    private NestingContext Context
    {
      get
      {
        lock (syncLock)
        {
          if (this.context == null)
          {
            this.context = new NestingContext(messageService, progressDisplayer);
          }
        }

        return this.context;
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

        this.isRunning = true;

        this.nestProjectViewModel = nestProjectViewModel;
        this.nestExecutionHelper.InitialiseNest(
                          this.Context,
                          nestProjectViewModel.ProjectInfo.SheetLoadInfos,
                          nestProjectViewModel.ProjectInfo.DetailLoadInfos,
                          this.progressDisplayer);
        if (this.Context.Sheets.Count == 0)
        {
          this.progressDisplayer.DisplayMessageBox("There are no sheets. Please add some and try again.", "DeepNest", MessageBoxIcon.Error);
        }
        else if (this.Context.Polygons.Count == 0)
        {
          this.progressDisplayer.DisplayMessageBox("There are no parts. Please add some and try again.", "DeepNest", MessageBoxIcon.Error);
        }
        else
        {
          this.nestWorker = new NestWorker(this);
          this.nestWorkerTask = new Task(async () => await this.nestWorker.Execute());
          this.nestWorkerConfiguredTaskAwaitable = this.nestWorkerTask.ConfigureAwait(false);
          this.nestWorkerConfiguredTaskAwaitable?.GetAwaiter().OnCompleted(() => this.IsRunning = false);
          this.nestWorkerTask.Start();
        }

        return true;
      }
    }

    public void Stop()
    {
      lock (syncLock)
      {
        this.IsStopping = true;
        this.nestWorkerTask?.Wait();
      }
    }

    private class NestWorker
    {
      private readonly NestMonitorViewModel nestMonitorViewModel;

      public NestWorker(NestMonitorViewModel nestMonitorViewModel)
      {
        this.nestMonitorViewModel = nestMonitorViewModel;
      }

      public async Task Execute()
      {
        nestMonitorViewModel.Context.StartNest();
        nestMonitorViewModel.progressDisplayer.UpdateNestsList();
        while (!nestMonitorViewModel.IsStopping)
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          await NestIterate();
          await UpdateNestsList();
          sw.Stop();
          if (SvgNest.Config.UseParallel)
          {
            await DisplayToolStripMessage($"Iteration time: {sw.ElapsedMilliseconds}ms ({nestMonitorViewModel.Context.State.AverageNestTime}ms average)");
          }
          else
          {
            await DisplayToolStripMessage($"Nesting time: {sw.ElapsedMilliseconds}ms");
          }

          if (nestMonitorViewModel.Context.State.IsErrored)
          {
            break;
          }
        }
      }

      private async Task DisplayToolStripMessage(string message)
      {
        if (!nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => nestMonitorViewModel.progressDisplayer.DisplayTransientMessage(message)).ConfigureAwait(false);
        }
      }

      private async Task UpdateNestsList()
      {
        if (!nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => nestMonitorViewModel.progressDisplayer.UpdateNestsList()).ConfigureAwait(false);
        }
      }

      private async Task NestIterate()
      {
        if (!nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => nestMonitorViewModel.Context.NestIterate(SvgNest.Config)).ConfigureAwait(false);
        }
      }
    }
  }
}