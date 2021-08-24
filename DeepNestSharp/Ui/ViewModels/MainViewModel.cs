namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Input;
  using AvalonDock.Layout;
  using AvalonDock.Themes;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Domain.ViewModels;
  using Microsoft.Toolkit.Mvvm.ComponentModel;
  using Microsoft.Toolkit.Mvvm.Input;

  public class DockingMainViewModel : MainViewModel
  {
    public DockingMainViewModel(IMessageService messageService, IDispatcherService dispatcherService, ISvgNestConfig config, IFileIoService fileIoService, IMouseCursorService mouseCursorService)
      : base(messageService, dispatcherService, config, fileIoService, mouseCursorService)
    {
    }
  }

  public class MainViewModel : ObservableRecipient, IMainViewModel
  {
    private readonly IFileIoService fileIoService;
    private readonly IMouseCursorService mouseCursorService;
    private readonly IMessageService messageService;
    private readonly ObservableCollection<IFileViewModel> files;
    private RelayCommand loadLayoutCommand;
    private RelayCommand saveLayoutCommand;
    private RelayCommand exitCommand;
    private AsyncRelayCommand loadNestProjectCommand;
    private AsyncRelayCommand loadPartCommand;
    private AsyncRelayCommand loadSheetNfpCommand;
    private AsyncRelayCommand loadNfpCandidatesCommand;
    private AsyncRelayCommand loadSheetPlacementCommand;
    private AsyncRelayCommand loadNestResultCommand;
    private RelayCommand activeDocumentSaveCommand;
    private RelayCommand activeDocumentSaveAsCommand;
    private RelayCommand createNestProjectCommand;

    private IToolViewModel[] tools;

    private PreviewViewModel? previewViewModel;
    private Tuple<string, Theme>? selectedTheme;
    private IFileViewModel? activeDocument;
    private IPropertiesViewModel? propertiesViewModel;
    private INestMonitorViewModel? nestMonitorViewModel;

    public MainViewModel(IMessageService messageService, IDispatcherService dispatcherService, ISvgNestConfig config, IFileIoService fileIoService, IMouseCursorService mouseCursorService)
    {
      SvgNestConfigViewModel = new SvgNestConfigViewModel(config);

      files = new ObservableCollection<IFileViewModel>();
      Files = new ReadOnlyObservableCollection<IFileViewModel>(files);

      this.Themes = new List<Tuple<string, Theme>>
      {
        new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),

        // new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
        // new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
        // new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
        // new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
        // new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
        // new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
        // new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
        // new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
      };

      this.SelectedTheme = Themes.First();
      this.messageService = messageService;
      this.DispatcherService = dispatcherService;
      this.fileIoService = fileIoService;
      this.mouseCursorService = mouseCursorService;
      this.ActiveDocumentChanged += this.MainViewModel_ActiveDocumentChanged;
    }

    public event EventHandler? ActiveDocumentChanged;

    public List<Tuple<string, Theme>> Themes { get; set; }

    public Tuple<string, Theme>? SelectedTheme
    {
      get => selectedTheme;

      set
      {
        selectedTheme = value;
        OnPropertyChanged(nameof(SelectedTheme));
      }
    }

    public IEnumerable<IToolViewModel> Tools
    {
      get
      {
        if (tools == null)
        {
          tools = new IToolViewModel[] { PreviewViewModel, SvgNestConfigViewModel, PropertiesViewModel, NestMonitorViewModel };
        }

        return tools;
      }
    }

    public ReadOnlyObservableCollection<IFileViewModel> Files { get; }

    public INestMonitorViewModel NestMonitorViewModel
    {
      get
      {
        if (nestMonitorViewModel == null)
        {
          nestMonitorViewModel = new NestMonitorViewModel(this, this.messageService, mouseCursorService);
        }

        return nestMonitorViewModel;
      }
    }

    public IPreviewViewModel PreviewViewModel
    {
      get
      {
        if (previewViewModel == null)
        {
          previewViewModel = new PreviewViewModel(this);
        }

        return previewViewModel;
      }
    }

    public IPropertiesViewModel PropertiesViewModel
    {
      get
      {
        if (propertiesViewModel == null)
        {
          propertiesViewModel = new PropertiesViewModel(this);
        }

        return propertiesViewModel;
      }
    }

    public ISvgNestConfigViewModel SvgNestConfigViewModel { get; }

    public ICommand LoadSheetPlacementCommand => loadSheetPlacementCommand ?? (loadSheetPlacementCommand = new AsyncRelayCommand(OnLoadSheetPlacementAsync));

    public ICommand LoadPartCommand => loadPartCommand ?? (loadPartCommand = new AsyncRelayCommand(OnLoadPartAsync));

    public ICommand LoadSheetNfpCommand => loadSheetNfpCommand ?? (loadSheetNfpCommand = new AsyncRelayCommand(OnLoadSheetNfpAsync));

    public ICommand LoadNfpCandidatesCommand => loadNfpCandidatesCommand ?? (loadNfpCandidatesCommand = new AsyncRelayCommand(OnLoadNfpCandidatesAsync));

    public ICommand LoadNestResultCommand => loadNestResultCommand ?? (loadNestResultCommand = new AsyncRelayCommand(OnLoadNestResultAsync));

    public ICommand CreateNestProjectCommand => createNestProjectCommand ?? (createNestProjectCommand = new RelayCommand(OnCreateNestProject));

    public ICommand LoadNestProjectCommand => loadNestProjectCommand ?? (loadNestProjectCommand = new AsyncRelayCommand(OnLoadNestProjectAsync));

    public ICommand ActiveDocumentSaveCommand => activeDocumentSaveCommand ?? (activeDocumentSaveCommand = new RelayCommand(() => Save(this.ActiveDocument, false), () => this.ActiveDocument?.IsDirty ?? false));

    public ICommand ActiveDocumentSaveAsCommand => activeDocumentSaveAsCommand ?? (activeDocumentSaveAsCommand = new RelayCommand(() => Save(this.ActiveDocument, true), () => true));

    public ICommand ExitCommand => exitCommand ?? (exitCommand = new RelayCommand(OnExit, CanExit));

    public ICommand LoadLayoutCommand => loadLayoutCommand ?? (saveLayoutCommand = new RelayCommand(OnSaveLayout, CanSaveLayout));

    public ICommand SaveLayoutCommand => saveLayoutCommand ?? (loadLayoutCommand = new RelayCommand(OnLoadLayout, CanLoadLayout));

    public IFileViewModel? ActiveDocument
    {
      get => activeDocument;
      set
      {
        if (activeDocument != value)
        {
          activeDocument = value;
          OnPropertyChanged(nameof(ActiveDocument));
          ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    public IDockingManagerFacade? DockManager { get; set; }

    public IDispatcherService DispatcherService { get; }

    public IMessageService MessageService => this.messageService;

    public async Task OnLoadNestProjectAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(ProjectInfo.FileDialogFilter).ConfigureAwait(false);
      OnLoadNestProject(filePath);
    }

    public void OnLoadNestProject(string filePath)
    {
      if (DispatcherService.InvokeRequired)
      {
        DispatcherService.Invoke(() => OnLoadNestProject(filePath));
      }
      else
      {
        string locatedFilePath;
        if (TryLocateFile(filePath, out locatedFilePath))
        {
          var loaded = new NestProjectViewModel(this, locatedFilePath, fileIoService);
          loaded.PropertyChanged += this.NestProjectViewModel_PropertyChanged;
          this.files.Add(loaded);
          this.ActiveDocument = loaded;
        }
      }
    }

    public async Task OnLoadNestResultAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(NestResult.FileDialogFilter).ConfigureAwait(false);
      LoadNestResult(filePath);
    }

    public void OnLoadNestResult(INestResult nestResult)
    {
      var loaded = new NestResultViewModel(this, nestResult);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadNestResult(string filePath)
    {
      string locatedFilePath;
      if (TryLocateFile(filePath, out locatedFilePath))
      {
        var loaded = new NestResultViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public async Task OnLoadPartAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(NFP.FileDialogFilter).ConfigureAwait(false);
      LoadPart(filePath);
    }

    public void LoadPart(string filePath)
    {
      string locatedFilePath;
      if (TryLocateFile(filePath, out locatedFilePath))
      {
        var loaded = new PartEditorViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadNfpCandidates(string filePath)
    {
      string locatedFilePath;
      if (TryLocateFile(filePath, out locatedFilePath))
      {
        var loaded = new NfpCandidateListViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadSheetNfp(string filePath)
    {
      string locatedFilePath;
      if (TryLocateFile(filePath, out locatedFilePath))
      {
        var loaded = new NfpCandidateListViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public async Task OnLoadSheetPlacementAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(SheetPlacement.FileDialogFilter).ConfigureAwait(false);
      LoadSheetPlacement(filePath);
    }

    public void LoadSheetPlacement(string filePath)
    {
      string locatedFilePath;
      if (TryLocateFile(filePath, out locatedFilePath))
      {
        var loaded = new SheetPlacementViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadSheetPlacement(ISheetPlacement sheetPlacement)
    {
      var loaded = new SheetPlacementViewModel(this, sheetPlacement, mouseCursorService);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void Close(IFileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        var res = messageService.DisplayYesNoCancel(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "DeepNestSharp", MessageBoxIcon.Question);
        if (res == DeepNestLib.MessageBoxResult.Cancel)
        {
          return;
        }

        if (res == DeepNestLib.MessageBoxResult.Yes)
        {
          Save(fileToClose);
        }
      }

      files.Remove(fileToClose);
    }

    public async Task ExportSheetPlacementAsync(ISheetPlacement? sheetPlacement)
    {
      if (sheetPlacement == null)
      {
        return;
      }

      var parts = sheetPlacement.PartPlacements.Select(o => o.Part).ToList();
      if (parts.ContainsDxfs() && parts.ContainsSvgs())
      {
        MessageService.DisplayMessageBox("It's not possible to export when your parts were a mix of Svg's and Dxf's.", "DeepNestPort: Not Implemented", MessageBoxIcon.Information);
      }
      else
      {
        IExport exporter = ExporterFactory.GetExporter(parts, SvgNestConfigViewModel.SvgNestConfig);
        var filePath = fileIoService.GetSaveFilePath(exporter.SaveFileDialogFilter);
        if (!string.IsNullOrWhiteSpace(filePath))
        {
          await exporter.Export(filePath, sheetPlacement);
        }
      }
    }

    public void Save(IFileViewModel? fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave != null)
      {
        if (fileToSave.FilePath == null || saveAsFlag)
        {
          var filePath = fileIoService.GetSaveFilePath(fileToSave.FileDialogFilter);
          if (!string.IsNullOrWhiteSpace(filePath))
          {
            fileToSave.FilePath = filePath;
          }
        }

        if (string.IsNullOrEmpty(fileToSave?.FilePath))
        {
          return;
        }

        File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
        if (ActiveDocument != null)
        {
          ActiveDocument.IsDirty = false;
        }
      }
    }

    private static IEnumerable<LayoutContent> GatherLayoutContent(ILayoutElement le)
    {
      if (le is LayoutContent)
      {
        yield return (LayoutContent)le;
      }

      IEnumerable<ILayoutElement> children = new ILayoutElement[0];
      if (le is LayoutRoot)
      {
        children = ((LayoutRoot)le).Children;
      }
      else if (le is LayoutPanel)
      {
        children = ((LayoutPanel)le).Children;
      }
      else if (le is LayoutDocumentPaneGroup)
      {
        children = ((LayoutDocumentPaneGroup)le).Children;
      }
      else if (le is LayoutAnchorablePane)
      {
        children = ((LayoutAnchorablePane)le).Children;
      }
      else if (le is LayoutDocumentPane)
      {
        children = ((LayoutDocumentPane)le).Children;
      }

      foreach (var child in children)
      {
        foreach (var x in GatherLayoutContent(child))
        {
          yield return x;
        }
      }
    }

    private async Task OnLoadNfpCandidatesAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(NfpCandidateList.FileDialogFilter).ConfigureAwait(false);
      LoadNfpCandidates(filePath);
    }

    private async Task OnLoadSheetNfpAsync()
    {
      var filePath = await fileIoService.GetOpenFilePathAsync(SheetNfp.FileDialogFilter).ConfigureAwait(false);
      LoadSheetNfp(filePath);
    }

    private bool TryLocateFile(string filePath, out string locatedFilePath)
    {
      if (!string.IsNullOrWhiteSpace(filePath) && new FileInfo(filePath).Exists)
      {
        locatedFilePath = filePath;
        return true;
      }

      if (string.IsNullOrWhiteSpace(filePath))
      {
        locatedFilePath = filePath;
        return false;
      }

      this.MessageService.DisplayMessageBox($"Unable to locate {filePath}.", "File Missing", MessageBoxIcon.Information);
      locatedFilePath = string.Empty;
      return false;
    }

    private void OnCreateNestProject()
    {
      var newFile = new NestProjectViewModel(this, fileIoService);
      this.files.Add(newFile);
      this.ActiveDocument = newFile;
    }

    private bool CanExit()
    {
      return true;
    }

    private bool CanLoadLayout()
    {
      return File.Exists(@".\AvalonDock.Layout.config");
    }

    private bool CanSaveLayout()
    {
      return true;
    }

    private void OnExit()
    {
      Application.Current.MainWindow.Close();
    }

    private void OnLoadLayout()
    {
      this.DockManager?.LoadLayout();
    }

    private void MainViewModel_ActiveDocumentChanged(object? sender, EventArgs e)
    {
      this.activeDocumentSaveCommand?.NotifyCanExecuteChanged();
      this.activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
    }

    private void NestProjectViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == $"{nameof(IFileViewModel.IsDirty)}")
      {
        activeDocumentSaveCommand?.NotifyCanExecuteChanged();
        activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnSaveLayout()
    {
      this.DockManager?.SaveLayout();
    }
  }
}
