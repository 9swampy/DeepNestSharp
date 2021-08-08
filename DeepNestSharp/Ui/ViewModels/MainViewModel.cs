namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Input;
  using AvalonDock;
  using AvalonDock.Layout;
  using AvalonDock.Layout.Serialization;
  using AvalonDock.Themes;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Ui.Docking;
  using Microsoft.Toolkit.Mvvm.ComponentModel;
  using Microsoft.Toolkit.Mvvm.Input;

  public class MainViewModel : ObservableRecipient
  {
    private readonly IFileIoService fileIoService;
    private readonly ObservableCollection<FileViewModel> files;
    private readonly RelayCommand loadLayoutCommand;
    private readonly RelayCommand saveLayoutCommand;
    private readonly RelayCommand exitCommand;
    private readonly RelayCommand loadNestProjectCommand;
    private readonly RelayCommand loadSheetPlacementCommand;
    private readonly RelayCommand activeDocumentSaveCommand;
    private readonly RelayCommand activeDocumentSaveAsCommand;
    private readonly RelayCommand createNestProjectCommand;

    private ToolViewModel[] tools;

    private PreviewViewModel? previewViewModel;
    private Tuple<string, Theme>? selectedTheme;
    private FileViewModel? activeDocument;
    private PropertiesViewModel? propertiesViewModel;
    private NestMonitorViewModel? nestMonitorViewModel;

    public MainViewModel(IDispatcherService dispatcherService, ISvgNestConfig config, IFileIoService fileIoService)
    {
      SvgNestConfigViewModel = new SvgNestConfigViewModel(config);

      createNestProjectCommand = new RelayCommand(ExecuteCreateNestProject);
      loadNestProjectCommand = new RelayCommand(LoadNestProject);
      loadSheetPlacementCommand = new RelayCommand(LoadSheetPlacement);
      activeDocumentSaveCommand = new RelayCommand(() => Save(this.ActiveDocument, false), () => this.ActiveDocument?.IsDirty ?? false);
      activeDocumentSaveAsCommand = new RelayCommand(() => Save(this.ActiveDocument, true), () => true);
      exitCommand = new RelayCommand(OnExit, CanExit);
      saveLayoutCommand = new RelayCommand(OnSaveLayout, CanSaveLayout);
      loadLayoutCommand = new RelayCommand(OnLoadLayout, CanLoadLayout);
      files = new ObservableCollection<FileViewModel>();
      Files = new ReadOnlyObservableCollection<FileViewModel>(files);

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
      this.DispatcherService = dispatcherService;
      this.fileIoService = fileIoService;

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

    public IEnumerable<ToolViewModel> Tools
    {
      get
      {
        if (tools == null)
        {
          tools = new ToolViewModel[] { PreviewViewModel, SvgNestConfigViewModel, PropertiesViewModel, NestMonitorViewModel };
        }

        return tools;
      }
    }

    public ReadOnlyObservableCollection<FileViewModel> Files { get; }

    public NestMonitorViewModel NestMonitorViewModel
    {
      get
      {
        if (nestMonitorViewModel == null)
        {
          nestMonitorViewModel = new NestMonitorViewModel(this, new MessageBoxService());
        }

        return nestMonitorViewModel;
      }
    }

    public PreviewViewModel PreviewViewModel
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

    public PropertiesViewModel PropertiesViewModel
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

    public SvgNestConfigViewModel SvgNestConfigViewModel { get; }

    public ICommand ExecuteLoadSheetPlacement => loadSheetPlacementCommand;

    public ICommand CreateNestProject => createNestProjectCommand;

    public ICommand ExecuteLoadNestProject => loadNestProjectCommand;

    public ICommand ExecuteActiveDocumentSave => activeDocumentSaveCommand;

    public ICommand ExecuteActiveDocumentSaveAs => activeDocumentSaveAsCommand;

    public ICommand ExitCommand => exitCommand;

    public ICommand LoadLayoutCommand => loadLayoutCommand;

    public ICommand SaveLayoutCommand => saveLayoutCommand;

    public FileViewModel? ActiveDocument
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

    public DockingManager? DockManager { get; internal set; }

    public IDispatcherService DispatcherService { get; }

    public void LoadNestProject()
    {
      var filePath = fileIoService.GetOpenFilePath(ProjectInfo.FileDialogFilter);
      if (!string.IsNullOrWhiteSpace(filePath) && fileIoService.Exists(filePath))
      {
        LoadNestProject(filePath);
      }
    }

    public void LoadNestProject(string fileName)
    {
      var loaded = new NestProjectViewModel(this, fileName, fileIoService);
      loaded.PropertyChanged += this.NestProjectViewModel_PropertyChanged;
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadPart()
    {
      var filePath = fileIoService.GetOpenFilePath(NFP.FileDialogFilter);
      if (!string.IsNullOrWhiteSpace(filePath))
      {
        LoadPart(filePath);
      }
    }

    public void LoadPart(string fileName)
    {
      var loaded = new PartEditorViewModel(this, fileName);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadSheetPlacement()
    {
      var filePath = fileIoService.GetOpenFilePath(DeepNestLib.Placement.SheetPlacement.FileDialogFilter);
      if (!string.IsNullOrWhiteSpace(filePath) && fileIoService.Exists(filePath))
      {
        LoadSheetPlacement(filePath);
      }
    }

    public void LoadSheetPlacement(string fileName)
    {
      var loaded = new SheetPlacementViewModel(this, fileName);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    internal void Close(FileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "DeepNestSharp", MessageBoxButton.YesNoCancel);
        if (res == MessageBoxResult.Cancel)
        {
          return;
        }

        if (res == MessageBoxResult.Yes)
        {
          Save(fileToClose);
        }
      }

      files.Remove(fileToClose);
    }

    internal void Save(FileViewModel? fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave != null)
      {
        if (fileToSave.FilePath == null || saveAsFlag)
        {
          var filePath = fileIoService.GetSaveFilePath(fileToSave.FileDialogFilter);
          if (!string.IsNullOrWhiteSpace(filePath) && fileIoService.Exists(filePath))
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

    private void ExecuteCreateNestProject()
    {
      var newFile = new NestProjectViewModel(this, fileIoService);
      this.files.Add(newFile);
      this.ActiveDocument = newFile;
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
      // Walk down the layout and gather the LayoutContent elements.
      // AD bails out when it tries to invoke RemoveViewFromLogicalChild
      // on them.
      /*var l = GatherLayoutContent(DockManager.Layout).ToArray();
      // Remove the views by force
      foreach (var x in l)
      {
        DockManager.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.Name.Equals("RemoveViewFromLogicalChild"))
            .First()
            .Invoke(DockManager, new object[] { x });
      }*/

      var layoutSerializer = new XmlLayoutSerializer(DockManager);
      /*layoutSerializer.LayoutSerializationCallback += (s, e) =>
      //{
      //  object o = e.Content;
      };*/

      var configFile = new FileInfo(@".\AvalonDock.Layout.config");
      if (configFile.Exists)
      {
        layoutSerializer.Deserialize(configFile.FullName);
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object? sender, EventArgs e)
    {
      this.activeDocumentSaveCommand?.NotifyCanExecuteChanged();
      this.activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
    }

    private void NestProjectViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == $"{nameof(FileViewModel.IsDirty)}")
      {
        activeDocumentSaveCommand?.NotifyCanExecuteChanged();
        activeDocumentSaveAsCommand?.NotifyCanExecuteChanged();
      }
    }

    private void OnSaveLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(DockManager);
      layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
    }
  }
}
