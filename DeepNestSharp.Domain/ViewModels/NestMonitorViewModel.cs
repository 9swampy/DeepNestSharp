namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.ViewModels;
  using Light.GuardClauses.FrameworkExtensions;

  public class NestMonitorViewModel : ToolViewModel, INestMonitorViewModel
  {
    private static volatile object syncLock = new object();

    private readonly IMainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private readonly IMouseCursorService mouseCursorService;
    private bool isRunning;
    private bool isStopping;
    private NestExecutionHelper nestExecutionHelper = new NestExecutionHelper();
    private NestingContext context; // this is the context that stores the original sheets, in the order they appear in the NestProjectEditor. remove before flight
    private NestWorker nestWorker;
    private ConfiguredTaskAwaitable? nestWorkerConfiguredTaskAwaitable;
    private Task nestWorkerTask;
    private string lastLogMessage = string.Empty;
    private double progress;
    private IProgressDisplayer progressDisplayer;
    private double progressSecondary;
    private int selectedIndex;
    private INestResult selectedItem;

    private RelayCommand stopNestCommand;
    private RelayCommand continueNestCommand;
    private RelayCommand restartNestCommand;
    private RelayCommand loadSheetPlacementCommand;
    private RelayCommand<INestResult> loadNestResultCommand;
    private bool isSecondaryProgressVisible;

    public NestMonitorViewModel(IMainViewModel mainViewModel, IMessageService messageService, IMouseCursorService mouseCursorService)
      : base("Monitor")
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.mouseCursorService = mouseCursorService;
    }

    public ICommand ContinueNestCommand => this.continueNestCommand ?? (this.continueNestCommand = new RelayCommand(this.OnContinueNest, () => false));

    public IZoomPreviewDrawingContext ZoomDrawingContext { get; } = new ZoomPreviewDrawingContext();

    public bool IsRunning
    {
      get => this.isRunning;

      private set
      {
        this.SetProperty(ref this.isRunning, value);
        this.Contextualise();
      }
    }

    public bool IsSecondaryProgressVisible
    {
      get => this.isSecondaryProgressVisible;
      set => this.SetProperty(ref this.isSecondaryProgressVisible, value);
    }

    public bool IsStopping
    {
      get => this.isStopping;

      private set
      {
        this.SetProperty(ref this.isStopping, value);
        this.Contextualise();
      }
    }

    public string LastLogMessage
    {
      get => this.lastLogMessage;
      set => this.SetProperty(ref this.lastLogMessage, value);
    }

    public ICommand LoadSheetPlacementCommand => this.loadSheetPlacementCommand ?? (this.loadSheetPlacementCommand = new RelayCommand(this.OnLoadSheetPlacement, () => false));

    public ICommand LoadNestResultCommand => this.loadNestResultCommand ?? (this.loadNestResultCommand = new RelayCommand<INestResult>(this.OnLoadNestResult, x => true));

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
      get => this.progress;
      set => this.SetProperty(ref this.progress, value);
    }

    public double ProgressSecondary
    {
      get => this.progressSecondary;
      set => this.SetProperty(ref this.progressSecondary, value);
    }

    public ICommand RestartNestCommand => this.restartNestCommand ?? (this.restartNestCommand = new RelayCommand(this.OnRestartNest, () => false));

    public INestState State => this.Context.State;

    public ICommand StopNestCommand => this.stopNestCommand ?? (this.stopNestCommand = new RelayCommand(this.OnStopNest, () => this.IsRunning && !this.IsStopping));

    public int SelectedIndex
    {
      get => this.selectedIndex;
      set => this.SetProperty(ref this.selectedIndex, value);
    }

    public INestResult SelectedItem
    {
      get => this.selectedItem;
      set
      {
        if (value == null)
        {
          this.ZoomDrawingContext.Clear();
          if (!this.TopNestResults.IsEmpty)
          {
            _ = Task.Factory.StartNew(() =>
              {
                this.SelectedIndex = 0;
              });
          }
        }
        else
        {
          this.SetProperty(ref this.selectedItem, value);
          this.ZoomDrawingContext.For(value.UsedSheets[0]);
          this.OnPropertyChanged(nameof(this.ZoomDrawingContext));
        }
      }
    }

    public TopNestResultsCollection TopNestResults => this.Context.State.TopNestResults;

    private NestingContext Context
    {
      get
      {
        lock (syncLock)
        {
          if (this.context == null)
          {
            IProgressDisplayer progressDisplayer = this.ProgressDisplayer;
            NestState nestState = new NestState(this.mainViewModel.SvgNestConfigViewModel.SvgNestConfig, this.mainViewModel.DispatcherService);
            this.context = new NestingContext(this.messageService, progressDisplayer, nestState, this.mainViewModel.SvgNestConfigViewModel.SvgNestConfig);
          }
        }

        return this.context;
      }
    }

    private IProgressDisplayer ProgressDisplayer => this.progressDisplayer ?? (this.progressDisplayer = new ProgressDisplayer(this, this.messageService, this.mainViewModel.DispatcherService));

    public async Task<bool> TryStartAsync(INestProjectViewModel nestProjectViewModel)
    {
      lock (syncLock)
      {
        if (this.isRunning)
        {
          return false;
        }

        this.IsRunning = true;
      }

      this.nestExecutionHelper.InitialiseNest(
        this.Context,
        nestProjectViewModel.ProjectInfo.SheetLoadInfos,
        nestProjectViewModel.ProjectInfo.DetailLoadInfos,
        this.ProgressDisplayer);
      if (this.Context.Sheets.Count == 0)
      {
        this.ProgressDisplayer.DisplayMessageBox("There are no sheets. Please add some and try again.", "DeepNest", MessageBoxIcon.Error);
      }
      else if (this.Context.Polygons.Count == 0)
      {
        this.ProgressDisplayer.DisplayMessageBox("There are no parts. Please add some and try again.", "DeepNest", MessageBoxIcon.Error);
      }
      else
      {
        this.nestWorker = new NestWorker(this);
        this.nestWorkerTask = this.nestWorker.ExecuteAsync();
        this.nestWorkerConfiguredTaskAwaitable = this.nestWorkerTask.ConfigureAwait(false);
        this.nestWorkerConfiguredTaskAwaitable?.GetAwaiter().OnCompleted(() =>
        {
          this.IsRunning = false;
        });

        this.nestWorkerTask.Start();
      }

      return true;
    }

    public void Stop()
    {
      lock (syncLock)
      {
        Debug.Print("NestMonitorViewModel.Stop()");
        this.IsStopping = true;
        this.context?.StopNest();
        this.nestWorkerTask?.Wait(1000); // Why shouldn't this be shorter than the previous value of 5 seconds?
      }
    }

    public void UpdateNestsList()
    {
      this.OnPropertyChanged(nameof(this.TopNestResults));
    }

    private void Contextualise()
    {
      if (this.mainViewModel.DispatcherService.InvokeRequired)
      {
        this.mainViewModel.DispatcherService.Invoke(this.Contextualise);
      }
      else
      {
        this.stopNestCommand?.NotifyCanExecuteChanged();
        this.restartNestCommand?.NotifyCanExecuteChanged();
        this.continueNestCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnLoadNestResult(INestResult nestResult)
    {
      if (nestResult != null)
      {
        this.mainViewModel.OnLoadNestResult(nestResult);
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
      Debug.Print("NestMonitorViewModel.OnStopNest()");
      this.mouseCursorService.OverrideCursor = Cursors.Wait;
      this.Stop();
    }

    //private INestResult NestResultWithOriginalSheets(INestResult nestResultWithOffsetSheets)
    //{
    //  for (int i = 0; i < nestResultWithOffsetSheets.UsedSheets.Count; i++)
    //  {
    //    // Somewhere here, I'm certain there's some shenanigans with modifying the reference for the offset sheets.
    //    int partCount = this.Context.Nest.NestItems.PartsLocal.Count();
    //    int sheetSource = nestResultWithOffsetSheets.UsedSheets[i].SheetSource - partCount;
    //    INfp originalSheetNfp = this.Context.Sheets[sheetSource];
    //    nestResultWithOffsetSheets.UsedSheets[i].Sheet.ReplacePoints(originalSheetNfp);
    //  }

    //  return nestResultWithOffsetSheets;
    //}

    private class NestWorker
    {
      private readonly NestMonitorViewModel nestMonitorViewModel;

      public NestWorker(NestMonitorViewModel nestMonitorViewModel)
      {
        this.nestMonitorViewModel = nestMonitorViewModel;
      }

      public async Task ExecuteAsync()
      {
        try
        {
          Debug.Print("NestMonitorViewModel.Start-Execute");
          await this.nestMonitorViewModel.Context.StartNest();
          this.nestMonitorViewModel.ProgressDisplayer.UpdateNestsList();
          while (!this.nestMonitorViewModel.IsStopping)
          {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await this.NestIterate();
            await this.UpdateNestsList();
            sw.Stop();
            if (SvgNest.Config.UseParallel)
            {
              await this.DisplayToolStripMessage($"Iteration time:{sw.ElapsedMilliseconds}ms Average:{this.nestMonitorViewModel.Context.State.AveragePlacementTime}ms");
            }
            else
            {
              await this.DisplayToolStripMessage($"Nesting time:{sw.ElapsedMilliseconds}ms Average:{this.nestMonitorViewModel.Context.State.AveragePlacementTime}ms");
            }

            if (this.nestMonitorViewModel.Context.State.IsErrored)
            {
              break;
            }
          }

          Debug.Print("NestMonitorViewModel.Exit-Execute");
        }
        catch (Exception ex)
        {
          this.nestMonitorViewModel.State.SetIsErrored();
          Debug.Print("NestMonitorViewModel.Error-Execute");
          Debug.Print(ex.Message);
          Debug.Print(ex.StackTrace);
        }
        finally
        {
          await this.nestMonitorViewModel.mainViewModel.DispatcherService.InvokeAsync(() =>
          {
            this.nestMonitorViewModel.mouseCursorService.OverrideCursor = null;
            this.nestMonitorViewModel.IsStopping = false;
            this.nestMonitorViewModel.ProgressDisplayer.ClearTransientMessage();
            this.nestMonitorViewModel.ProgressDisplayer.IsVisibleSecondaryProgressBar = false;
          });

          Debug.Print("NestMonitorViewModel.Finally-Execute");
        }
      }

      private async Task DisplayToolStripMessage(string message)
      {
        if (!this.nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => this.nestMonitorViewModel.ProgressDisplayer.DisplayTransientMessage(message)).ConfigureAwait(false);
        }
      }

      private async Task UpdateNestsList()
      {
        if (!this.nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => this.nestMonitorViewModel.ProgressDisplayer.UpdateNestsList()).ConfigureAwait(false);
        }
      }

      private async Task NestIterate()
      {
        if (!this.nestMonitorViewModel.IsStopping)
        {
          if (this.nestMonitorViewModel.Context.Nest.IsStopped)
          {
            this.nestMonitorViewModel.Stop();
          }
          else
          {
            await Task.Run(() =>
            {
              this.nestMonitorViewModel.Context.NestIterate(SvgNest.Config);
            }).ConfigureAwait(false);
          }
        }
      }
    }
  }
}