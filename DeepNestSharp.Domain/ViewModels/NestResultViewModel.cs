namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.Docking;
  using Microsoft.Toolkit.Mvvm.Input;

  public class NestResultViewModel : FileViewModel
  {
    private readonly IMouseCursorService mouseCursorService;
    private readonly IMessageService messageService;
    private ObservableNestResult nestResult;
    private int selectedIndex;
    private ObservableSheetPlacement selectedItem;
    private RelayCommand<ISheetPlacement> loadSheetPlacementCommand;
    private AsyncRelayCommand loadAllExactCommand;
    private AsyncRelayCommand<ISheetPlacement> exportSheetPlacementCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    private NestResultViewModel(IMainViewModel mainViewModel, IMouseCursorService mouseCursorService, IMessageService messageService)
      : base(mainViewModel)
    {
      this.mouseCursorService = mouseCursorService;
      this.messageService = messageService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestResultViewModel(IMainViewModel mainViewModel, string filePath, IMouseCursorService mouseCursorService, IMessageService messageService)
      : base(mainViewModel, filePath)
    {
      this.mouseCursorService = mouseCursorService;
      this.messageService = messageService;
    }

    public NestResultViewModel(IMainViewModel mainViewModel, INestResult nestResult, IMouseCursorService mouseCursorService, IMessageService messageService)
      : this(mainViewModel, mouseCursorService, messageService)
    {
      if (nestResult is ObservableNestResult obs)
      {
        this.nestResult = obs;
      }
      else
      {
        this.nestResult = new ObservableNestResult(nestResult);
      }
    }

    public IRelayCommand<ISheetPlacement> ExportSheetPlacementCommand => exportSheetPlacementCommand ?? (exportSheetPlacementCommand = new AsyncRelayCommand<ISheetPlacement>(OnExportSheetPlacementAsync));

    public override string FileDialogFilter => DeepNestLib.Placement.NestResult.FileDialogFilter;

    public IRelayCommand<ISheetPlacement> LoadSheetPlacementCommand => loadSheetPlacementCommand ?? (loadSheetPlacementCommand = new RelayCommand<ISheetPlacement>(OnLoadSheetPlacement));

    public IRelayCommand LoadAllExactCommand => loadAllExactCommand ?? (loadAllExactCommand = new AsyncRelayCommand(OnLoadAllExactAsync));

    public INestResult NestResult => this.nestResult;

    public override string TextContent => this.NestResult?.ToJson() ?? string.Empty;

    public int SelectedIndex
    {
      get => selectedIndex;
      set => SetProperty(ref selectedIndex, value);
    }

    public ObservableSheetPlacement SelectedItem
    {
      get => selectedItem;
      set
      {
        if (value is ObservableSheetPlacement observableSheetPlacement)
        {
          SetProperty(ref selectedItem, observableSheetPlacement, nameof(SelectedItem));
        }
        else
        {
          throw new ArgumentException(nameof(SelectedItem));
        }
      }
    }

    protected override void LoadContent()
    {
      var nestResult = DeepNestLib.Placement.NestResult.LoadFromFile(this.FilePath);
      this.nestResult = new ObservableNestResult(nestResult);

      this.nestResult.PropertyChanged += this.NestResult_PropertyChanged;
      NotifyContentUpdated();
    }

    private void NestResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      NotifyContentUpdated();
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(NestResult));
      OnPropertyChanged(nameof(SelectedItem));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }

    private async Task OnExportSheetPlacementAsync(ISheetPlacement sheetPlacement)
    {
      try
      {
        await MainViewModel.ExportSheetPlacementAsync(sheetPlacement).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        messageService.DisplayMessage(ex);
      }
    }

    private void OnLoadSheetPlacement(ISheetPlacement sheetPlacement)
    {
      try
      {
        if (sheetPlacement != null)
        {
          MainViewModel.LoadSheetPlacement(sheetPlacement);
        }
      }
      catch (Exception ex)
      {
        messageService.DisplayMessage(ex);
      }
    }

    private async Task OnLoadAllExactAsync()
    {
      try
      {
        mouseCursorService.OverrideCursor = Cursors.Wait;
        foreach (var sp in this.NestResult.UsedSheets)
        {
          var partPlacementList = sp.PartPlacements.Cast<ObservablePartPlacement>().ToList();
          foreach (var pp in partPlacementList)
          {
            await pp.OnLoadExact();
          }
        }
      }
      catch (Exception ex)
      {
        messageService.DisplayMessage(ex);
      }
      finally
      {
        NotifyContentUpdated();
        loadAllExactCommand?.NotifyCanExecuteChanged();
        mouseCursorService.OverrideCursor = Cursors.Null;
      }
    }
  }
}