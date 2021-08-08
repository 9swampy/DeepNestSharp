namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Ui.Docking;
  using Microsoft.Toolkit.Mvvm.Input;

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
    private string lastLogMessage = string.Empty;
    private double progress;
    private int selectedIndex;
    private INestResult? selectedItem;

    private RelayCommand stopNestCommand;
    private RelayCommand continueNestCommand;
    private RelayCommand restartNestCommand;
    private RelayCommand loadSheetPlacementCommand;
    private RelayCommand<INestResult> loadNestResultCommand;

    public NestMonitorViewModel(MainViewModel mainViewModel, IMessageService messageService)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.progressDisplayer = new ProgressDisplayer(this, messageService, mainViewModel.DispatcherService, mainViewModel);
    }

    public ICommand ContinueNestCommand => continueNestCommand ?? (continueNestCommand = new RelayCommand(OnContinueNest, () => false));

    public ZoomPreviewDrawingContext DrawingContext { get; } = new ZoomPreviewDrawingContext();

    public bool IsRunning
    {
      get
      {
        return this.isRunning;
      }

      private set
      {
        SetProperty(ref this.isRunning, value);
        Contextualise();
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
        Contextualise();
      }
    }

    public string LastLogMessage
    {
      get => lastLogMessage;
      internal set => SetProperty(ref lastLogMessage, value);
    }

    public ICommand LoadSheetPlacementCommand => loadSheetPlacementCommand ?? (loadSheetPlacementCommand = new RelayCommand(OnLoadSheetPlacement, () => false));

    public ICommand LoadNestResultCommand => loadNestResultCommand ?? (loadNestResultCommand = new RelayCommand<INestResult>(OnLoadNestResult, x => true));

    public string MessageLog
    {
      get
      {
        return this.MessageLogBuilder.ToString();
      }
    }

    public StringBuilder MessageLogBuilder { get; } = new StringBuilder();

    public double Progress
    {
      get => progress;
      internal set => SetProperty(ref progress, value);
    }

    public ICommand RestartNestCommand => restartNestCommand ?? (restartNestCommand = new RelayCommand(OnRestartNest, () => false));

    public NestState State => Context.State;

    public ICommand StopNestCommand => stopNestCommand ?? (stopNestCommand = new RelayCommand(OnStopNest, () => IsRunning && !IsStopping));

    public int SelectedIndex
    {
      get => selectedIndex;
      set => SetProperty(ref selectedIndex, value);
    }

    public INestResult? SelectedItem
    {
      get => selectedItem;
      set
      {
        SetProperty(ref selectedItem, value);
        if (value == null)
        {
          DrawingContext.Clear();
        }
        else
        {
          DrawingContext.For(value.UsedSheets[0]);
        }

        OnPropertyChanged(nameof(DrawingContext));
      }
    }

    public TopNestResultsCollection TopNestResults => Context.State.TopNestResults;

    private NestingContext Context
    {
      get
      {
        lock (syncLock)
        {
          if (this.context == null)
          {
            this.context = new NestingContext(messageService, progressDisplayer, new NestState(mainViewModel.SvgNestConfigViewModel.SvgNestConfig, mainViewModel.DispatcherService));
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

        this.IsRunning = true;

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
          this.nestWorkerTask = new Task(() =>
          {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Task.Run(() => this.nestWorker.Execute()).Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
          });
          this.nestWorkerConfiguredTaskAwaitable = this.nestWorkerTask.ConfigureAwait(false);
          this.nestWorkerConfiguredTaskAwaitable?.GetAwaiter().OnCompleted(() =>
          {
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
        this.context?.StopNest();
        this.nestWorkerTask?.Wait(5000);
      }
    }

    internal void UpdateNestsList()
    {
      OnPropertyChanged(nameof(TopNestResults));
    }

    private void Contextualise()
    {
      if (mainViewModel.DispatcherService.InvokeRequired)
      {
        mainViewModel.DispatcherService.Invoke(Contextualise);
      }
      else
      {
        stopNestCommand?.NotifyCanExecuteChanged();
        restartNestCommand?.NotifyCanExecuteChanged();
        continueNestCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnLoadNestResult(INestResult? nestResult)
    {
      if (nestResult != null)
      {
        mainViewModel.OnLoadNestResult(nestResult);
      }
    }

    private void OnContinueNest()
    {
      throw new NotImplementedException();
    }

    private void OnLoadSheetPlacement()
    {
      throw new NotImplementedException();
    }

    private void OnRestartNest()
    {
      throw new NotImplementedException();
    }

    private void OnStopNest()
    {
      Mouse.OverrideCursor = Cursors.Wait;
      this.Stop();
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
          Debug.Print("Start-Execute");
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
              await DisplayToolStripMessage($"Iteration time:{sw.ElapsedMilliseconds}ms Average:{nestMonitorViewModel.Context.State.AverageNestTime}ms");
            }
            else
            {
              await DisplayToolStripMessage($"Nesting time:{sw.ElapsedMilliseconds}ms Average:{nestMonitorViewModel.Context.State.AverageNestTime}ms");
            }

            if (nestMonitorViewModel.Context.State.IsErrored)
            {
              break;
            }
          }

          Debug.Print("Exit-Execute");
        }
        catch (Exception ex)
        {
          this.nestMonitorViewModel.State.SetIsErrored();
          Debug.Print("Error-Execute");
          Debug.Print(ex.Message);
          Debug.Print(ex.StackTrace);
        }
        finally
        {
          nestMonitorViewModel.mainViewModel.DispatcherService.Invoke(() =>
          {
            Mouse.OverrideCursor = null;
            nestMonitorViewModel.IsStopping = false;
          });

          Debug.Print("Finally-Execute");
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