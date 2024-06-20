namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using CommunityToolkit.Mvvm.ComponentModel;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Services;

  public abstract class MainViewModel : ObservableRecipient, IMainViewModel
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
    private RelayCommand aboutDialogCommand;

    private IToolViewModel[] tools;

    private PreviewViewModel previewViewModel;
    private IFileViewModel activeDocument;
    private IPropertiesViewModel propertiesViewModel;
    private INestMonitorViewModel nestMonitorViewModel;

    public MainViewModel(IMessageService messageService, IDispatcherService dispatcherService, ISvgNestConfig config, IFileIoService fileIoService, IMouseCursorService mouseCursorService)
    {
      this.SvgNestConfigViewModel = new SvgNestConfigViewModel(config);

      this.files = new ObservableCollection<IFileViewModel>();
      this.Files = new ReadOnlyObservableCollection<IFileViewModel>(this.files);

      this.messageService = messageService;
      this.DispatcherService = dispatcherService;
      this.fileIoService = fileIoService;
      this.mouseCursorService = mouseCursorService;
      this.ActiveDocumentChanged += this.MainViewModel_ActiveDocumentChanged;
    }

    public event EventHandler ActiveDocumentChanged;

    public IAboutDialogService AboutDialogService { get; set; }

    public ICommand AboutDialogCommand => this.aboutDialogCommand ?? (this.aboutDialogCommand = new RelayCommand(() => this.OnAboutDialog()));

    public ICommand ActiveDocumentSaveCommand => this.activeDocumentSaveCommand ?? (this.activeDocumentSaveCommand = new RelayCommand(() => this.Save(this.ActiveDocument, false), () => this.ActiveDocument?.IsDirty ?? false));

    public ICommand ActiveDocumentSaveAsCommand => this.activeDocumentSaveAsCommand ?? (this.activeDocumentSaveAsCommand = new RelayCommand(() => this.Save(this.ActiveDocument, true), () => true));

    public IEnumerable<IToolViewModel> Tools
    {
      get
      {
        if (this.tools == null)
        {
          this.tools = new IToolViewModel[] { this.PreviewViewModel, this.SvgNestConfigViewModel, this.PropertiesViewModel, this.NestMonitorViewModel };
        }

        return this.tools;
      }
    }

    public ReadOnlyObservableCollection<IFileViewModel> Files { get; }

    public INestMonitorViewModel NestMonitorViewModel
    {
      get
      {
        if (this.nestMonitorViewModel == null)
        {
          this.nestMonitorViewModel = new NestMonitorViewModel(this, this.messageService, this.mouseCursorService);
        }

        return this.nestMonitorViewModel;
      }
    }

    public IPreviewViewModel PreviewViewModel
    {
      get
      {
        if (this.previewViewModel == null)
        {
          this.previewViewModel = new PreviewViewModel(this);
        }

        return this.previewViewModel;
      }
    }

    public IPropertiesViewModel PropertiesViewModel
    {
      get
      {
        if (this.propertiesViewModel == null)
        {
          this.propertiesViewModel = new PropertiesViewModel(this);
        }

        return this.propertiesViewModel;
      }
    }

    public ISvgNestConfigViewModel SvgNestConfigViewModel { get; }

    public ICommand LoadSheetPlacementCommand => this.loadSheetPlacementCommand ?? (this.loadSheetPlacementCommand = new AsyncRelayCommand(this.OnLoadSheetPlacementAsync));

    public ICommand LoadPartCommand => this.loadPartCommand ?? (this.loadPartCommand = new AsyncRelayCommand(this.OnLoadPartAsync));

    public ICommand LoadSheetNfpCommand => this.loadSheetNfpCommand ?? (this.loadSheetNfpCommand = new AsyncRelayCommand(this.OnLoadSheetNfpAsync));

    public ICommand LoadNfpCandidatesCommand => this.loadNfpCandidatesCommand ?? (this.loadNfpCandidatesCommand = new AsyncRelayCommand(this.OnLoadNfpCandidatesAsync));

    public ICommand LoadNestResultCommand => this.loadNestResultCommand ?? (this.loadNestResultCommand = new AsyncRelayCommand(this.OnLoadNestResultAsync));

    public ICommand CreateNestProjectCommand => this.createNestProjectCommand ?? (this.createNestProjectCommand = new RelayCommand(this.OnCreateNestProject));

    public ICommand LoadNestProjectCommand => this.loadNestProjectCommand ?? (this.loadNestProjectCommand = new AsyncRelayCommand(this.OnLoadNestProjectAsync));

    public ICommand ExitCommand => this.exitCommand ?? (this.exitCommand = new RelayCommand(this.OnExit, this.CanExit));

    protected abstract void OnExit();

    public ICommand LoadLayoutCommand => this.loadLayoutCommand ?? (this.loadLayoutCommand = new RelayCommand(this.OnLoadLayout, this.CanLoadLayout));

    public ICommand SaveLayoutCommand => this.saveLayoutCommand ?? (this.saveLayoutCommand = new RelayCommand(this.OnSaveLayout, this.CanSaveLayout));

    public IFileViewModel ActiveDocument
    {
      get => this.activeDocument;
      set
      {
        if (this.activeDocument != value)
        {
          this.activeDocument = value;
          this.OnPropertyChanged(nameof(this.ActiveDocument));
          this.ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
          this.SetSelectedToolView(value);
        }
      }
    }

    public IDockingManagerFacade DockManager { get; set; }

    public IDispatcherService DispatcherService { get; }

    public IMessageService MessageService => this.messageService;

    public string Title => $"DeepNest# {this.GetType().Assembly.GetName().Version?.ToString()}";

    public void SetSelectedToolView(IFileViewModel fileViewModel)
    {
      if (fileViewModel is NestProjectViewModel)
      {
        this.NestMonitorViewModel.IsSelected = true;
      }
      else
      {
        this.PreviewViewModel.IsSelected = true;
      }
    }

    private void OnAboutDialog()
    {
      this.AboutDialogService.ShowDialog();
    }

    public async Task OnLoadNestProjectAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(ProjectInfo.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastNestFilePath).ConfigureAwait(false);
      this.OnLoadNestProject(filePath);
    }

    public void OnLoadNestProject(string filePath)
    {
      if (this.DispatcherService.InvokeRequired)
      {
        this.DispatcherService.Invoke(() => this.OnLoadNestProject(filePath));
      }
      else
      {
        string locatedFilePath;
        if (this.TryLocateFile(filePath, out locatedFilePath))
        {
          NestProjectViewModel loaded = new NestProjectViewModel(this, locatedFilePath, this.fileIoService);
          loaded.PropertyChanged += this.NestProjectViewModel_PropertyChanged;
          this.files.Add(loaded);
          this.ActiveDocument = loaded;
        }
      }
    }

    public async Task OnLoadNestResultAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(NestResult.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastNestFilePath).ConfigureAwait(false);
      this.LoadNestResult(filePath);
    }

    public void OnLoadNestResult(INestResult nestResult)
    {
      NestResultViewModel loaded = new NestResultViewModel(this, nestResult);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadNestResult(string filePath)
    {
      string locatedFilePath;
      if (this.TryLocateFile(filePath, out locatedFilePath))
      {
        NestResultViewModel loaded = new NestResultViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public async Task OnLoadPartAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(NoFitPolygon.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastNestFilePath).ConfigureAwait(false);
      this.LoadPart(filePath);
    }

    public void LoadPart(string filePath)
    {
      string locatedFilePath;
      if (this.TryLocateFile(filePath, out locatedFilePath))
      {
        PartEditorViewModel loaded = new PartEditorViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadNfpCandidates(string filePath)
    {
      string locatedFilePath;
      if (this.TryLocateFile(filePath, out locatedFilePath))
      {
        NfpCandidateListViewModel loaded = new NfpCandidateListViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadSheetNfp(string filePath)
    {
      string locatedFilePath;
      if (this.TryLocateFile(filePath, out locatedFilePath))
      {
        NfpCandidateListViewModel loaded = new NfpCandidateListViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public async Task OnLoadSheetPlacementAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(SheetPlacement.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastDebugFilePath).ConfigureAwait(false);
      this.LoadSheetPlacement(filePath);
    }

    public void LoadSheetPlacement(string filePath)
    {
      string locatedFilePath;
      if (this.TryLocateFile(filePath, out locatedFilePath))
      {
        SheetPlacementViewModel loaded = new SheetPlacementViewModel(this, locatedFilePath);
        this.files.Add(loaded);
        this.ActiveDocument = loaded;
      }
    }

    public void LoadSheetPlacement(ISheetPlacement sheetPlacement)
    {
      SheetPlacementViewModel loaded = new SheetPlacementViewModel(this, sheetPlacement, this.mouseCursorService);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void Close(IFileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        MessageBoxResult res = this.messageService.DisplayYesNoCancel(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "DeepNestSharp", MessageBoxIcon.Question);
        if (res == MessageBoxResult.Cancel)
        {
          return;
        }

        if (res == MessageBoxResult.Yes)
        {
          this.Save(fileToClose);
        }
      }

      this.files.Remove(fileToClose);
    }

    public async Task ExportSheetPlacementAsync(ISheetPlacement sheetPlacement)
    {
      try
      {
        if (sheetPlacement == null)
        {
          return;
        }

        List<INfp> parts = sheetPlacement.PartPlacements.Select(o => o.Part).ToList();
        if (parts.ContainsDxfs() && parts.ContainsSvgs())
        {
          this.MessageService.DisplayMessageBox("It's not possible to export when your parts were a mix of Svg's and Dxf's.", "DeepNestPort: Not Implemented", MessageBoxIcon.Information);
        }
        else
        {
          IExport exporter = ExporterFactory.GetExporter(sheetPlacement.PartPlacements.Select(o => o.Part).ToList());
          var filePath = this.fileIoService.GetSaveFilePath(exporter.SaveFileDialogFilter);
          if (!string.IsNullOrWhiteSpace(filePath))
          {
            await exporter.Export(filePath, sheetPlacement, SvgNest.Config.MergeLines, SvgNest.Config.DifferentiateChildren);
          }
        }
      }
      catch (Exception ex)
      {
        this.MessageService.DisplayMessageBox(ex.Message, "Error Saving", MessageBoxIcon.Exclamation);
      }
    }

    public void Save(IFileViewModel fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave != null)
      {
        if (fileToSave.FilePath == null || saveAsFlag)
        {
          var filePath = this.fileIoService.GetSaveFilePath(fileToSave.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastNestFilePath);
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
        if (this.ActiveDocument != null)
        {
          this.ActiveDocument.IsDirty = false;
        }
      }
    }

    private async Task OnLoadNfpCandidatesAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(NfpCandidateList.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastDebugFilePath).ConfigureAwait(false);
      this.LoadNfpCandidates(filePath);
    }

    private async Task OnLoadSheetNfpAsync()
    {
      var filePath = await this.fileIoService.GetOpenFilePathAsync(SheetNfp.FileDialogFilter, this.SvgNestConfigViewModel.SvgNestConfig.LastDebugFilePath).ConfigureAwait(false);
      this.LoadSheetNfp(filePath);
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
      NestProjectViewModel newFile = new NestProjectViewModel(this, this.fileIoService);
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

    private void OnLoadLayout()
    {
      this.DockManager?.LoadLayout();
    }

    private void MainViewModel_ActiveDocumentChanged(object sender, EventArgs e)
    {
      this.activeDocumentSaveCommand?.NotifyCanExecuteChanged();
      this.activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
    }

    private void NestProjectViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == $"{nameof(IFileViewModel.IsDirty)}")
      {
        this.activeDocumentSaveCommand?.NotifyCanExecuteChanged();
        this.activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnSaveLayout()
    {
      this.DockManager?.SaveLayout();
    }
  }
}
