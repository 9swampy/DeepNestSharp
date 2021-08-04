namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Data;
  using System.Windows.Threading;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

  public class NestMonitorViewModel : ToolViewModel
  {
    private static volatile object syncLock = new object();

    private readonly MainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private readonly ISvgNestConfig config;
    private readonly IProgressDisplayer progressDisplayer;
    private INestProjectViewModel? nestProjectViewModel;
    private bool isRunning;
    private bool isStopping;
    private NestExecutionHelper nestExecutionHelper = new NestExecutionHelper();
    private NestingContext? context;
    private NestWorker? nestWorker;
    private ConfiguredTaskAwaitable? nestWorkerConfiguredTaskAwaitable;
    private Task? nestWorkerTask;
    private string lastLogMessage;
    private double progress;

    public NestMonitorViewModel(MainViewModel mainViewModel, IMessageService messageService, ISvgNestConfig config)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.config = config;
      this.progressDisplayer = new ProgressDisplayer(this, messageService, mainViewModel.DispatcherService);
    }

    internal void UpdateNestsList()
    {
      OnPropertyChanged(nameof(TopNestResults));
      System.Diagnostics.Debug.Print("UpdateNestsList");
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
            this.context = new NestingContext(messageService, progressDisplayer, new NestState(config, mainViewModel.DispatcherService));
          }
        }

        return this.context;
      }
    }

    public NestState State => Context.State;

    public TopNestResultsCollection TopNestResults => Context.State.TopNestResults;

    public string LastLogMessage
    {
      get => lastLogMessage;
      internal set => SetProperty(ref lastLogMessage, value);
    }

    public double Progress
    {
      get => progress;
      internal set => SetProperty(ref progress, value);
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
          this.nestWorkerTask = new Task(async () =>
          {
            System.Diagnostics.Debug.Print("Pre-Execute");
            await this.nestWorker.Execute();
            System.Diagnostics.Debug.Print("Post-Execute");
          });
          this.nestWorkerConfiguredTaskAwaitable = this.nestWorkerTask.ConfigureAwait(false);
          this.nestWorkerConfiguredTaskAwaitable?.GetAwaiter().OnCompleted(() =>
          {
            System.Diagnostics.Debug.Print("OnCompleted-Execute");
            this.IsRunning = false;
          });
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
        try
        {
          System.Diagnostics.Debug.Print("Start-Execute");
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

          System.Diagnostics.Debug.Print("Exit-Execute");
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.Print("Error-Execute");
          System.Diagnostics.Debug.Print(ex.Message);
          System.Diagnostics.Debug.Print(ex.StackTrace);
        }
        finally
        {
          System.Diagnostics.Debug.Print("Finally-Execute");
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
          await Task.Run(() =>
          {
            nestMonitorViewModel.Context.NestIterate(SvgNest.Config);
          }).ConfigureAwait(false);
        }
      }
    }
  }
}