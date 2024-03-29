﻿namespace DeepNestSharp.Domain.ViewModels
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
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.ViewModels;
  using Microsoft.Toolkit.Mvvm.Input;

  public class NestMonitorViewModel : ToolViewModel, INestMonitorViewModel
  {
    private static volatile object syncLock = new object();

    private readonly IMainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private readonly IMouseCursorService mouseCursorService;
    private bool isRunning;
    private bool isStopping;
    private NestExecutionHelper nestExecutionHelper = new NestExecutionHelper();
    private NestingContext context;
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

    public ICommand ContinueNestCommand => continueNestCommand ?? (continueNestCommand = new RelayCommand(OnContinueNest, () => false));

    public IZoomPreviewDrawingContext ZoomDrawingContext { get; } = new ZoomPreviewDrawingContext();

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

    public bool IsSecondaryProgressVisible
    {
      get => isSecondaryProgressVisible;
      set => SetProperty(ref isSecondaryProgressVisible, value);
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
      set => SetProperty(ref lastLogMessage, value);
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
      set => SetProperty(ref progress, value);
    }

    public double ProgressSecondary
    {
      get => progressSecondary;
      set => SetProperty(ref progressSecondary, value);
    }

    public ICommand RestartNestCommand => restartNestCommand ?? (restartNestCommand = new RelayCommand(OnRestartNest, () => false));

    public INestState State => Context.State;

    public ICommand StopNestCommand => stopNestCommand ?? (stopNestCommand = new RelayCommand(OnStopNest, () => IsRunning && !IsStopping));

    public int SelectedIndex
    {
      get => selectedIndex;
      set => SetProperty(ref selectedIndex, value);
    }

    public INestResult SelectedItem
    {
      get => selectedItem;
      set
      {
        if (value == null)
        {
          ZoomDrawingContext.Clear();
          if (!this.TopNestResults.IsEmpty)
          {
            _ = Task.Factory.StartNew(() =>
              {
                SelectedIndex = 0;
              });
          }
        }
        else
        {
          SetProperty(ref selectedItem, value);
          ZoomDrawingContext.For(value.UsedSheets[0]);
          OnPropertyChanged(nameof(ZoomDrawingContext));
        }
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
            var progressDisplayer = ProgressDisplayer;
            var nestState = new NestState(mainViewModel.SvgNestConfigViewModel.SvgNestConfig, mainViewModel.DispatcherService);
            this.context = new NestingContext(messageService, progressDisplayer, nestState, mainViewModel.SvgNestConfigViewModel.SvgNestConfig);
          }
        }

        return this.context;
      }
    }

    private IProgressDisplayer ProgressDisplayer => progressDisplayer ?? (progressDisplayer = new ProgressDisplayer(this, messageService, mainViewModel.DispatcherService));

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
        this.nestWorkerTask?.Wait(5000);
      }
    }

    public void UpdateNestsList()
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

    private void OnLoadNestResult(INestResult nestResult)
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
      System.Diagnostics.Debug.Print("NestMonitorViewModel.OnStopNest()");
      this.mouseCursorService.OverrideCursor = Cursors.Wait;
      this.Stop();
    }

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
          await nestMonitorViewModel.Context.StartNest();
          nestMonitorViewModel.ProgressDisplayer.UpdateNestsList();
          while (!nestMonitorViewModel.IsStopping)
          {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await NestIterate();
            await UpdateNestsList();
            sw.Stop();
            if (SvgNest.Config.UseParallel)
            {
              await DisplayToolStripMessage($"Iteration time:{sw.ElapsedMilliseconds}ms Average:{nestMonitorViewModel.Context.State.AveragePlacementTime}ms");
            }
            else
            {
              await DisplayToolStripMessage($"Nesting time:{sw.ElapsedMilliseconds}ms Average:{nestMonitorViewModel.Context.State.AveragePlacementTime}ms");
            }

            if (nestMonitorViewModel.Context.State.IsErrored)
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
          await nestMonitorViewModel.mainViewModel.DispatcherService.InvokeAsync(() =>
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
        if (!nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => nestMonitorViewModel.ProgressDisplayer.DisplayTransientMessage(message)).ConfigureAwait(false);
        }
      }

      private async Task UpdateNestsList()
      {
        if (!nestMonitorViewModel.IsStopping)
        {
          await Task.Run(() => nestMonitorViewModel.ProgressDisplayer.UpdateNestsList()).ConfigureAwait(false);
        }
      }

      private async Task NestIterate()
      {
        if (!nestMonitorViewModel.IsStopping)
        {
          if (nestMonitorViewModel.Context.Nest.IsStopped)
          {
            nestMonitorViewModel.Stop();
          }
          else
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
}