namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Input;
  using AvalonDock.Themes;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.ComponentModel;
  using Microsoft.Toolkit.Mvvm.Input;
  using Microsoft.Win32;

  public class MainViewModel : ObservableRecipient
  {
    private int selectedPartIndex;
    private ObservablePartPlacement selectedPartItem;
    private ToolViewModel[] tools;

    private ObservableCollection<FileViewModel> files = new ObservableCollection<FileViewModel>();
    private ReadOnlyObservableCollection<FileViewModel> readonlyFiles;

    private MainViewModel mainViewModel;
    private PreviewViewModel previewViewModel;
    private SvgNestConfigViewModel svgNestConfigViewModel;
    private Tuple<string, Theme> selectedTheme;
    private FileViewModel activeDocument;
    private PropertiesViewModel propertiesViewModel;

    public MainViewModel()
    {
      ExecuteLoadSheetPlacement = new RelayCommand(LoadSheetPlacement);
      ExecuteLoadNestProject = new RelayCommand(LoadNestProject);

      ExecuteSaveSheetPlacement = new RelayCommand(SaveSheetPlacement);
      ExecuteSaveNestProject = new RelayCommand(SaveNestProject);

      this.Themes = new List<Tuple<string, Theme>>
      {
        new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),
        //new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
        //new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
        //new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
        //new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
        //new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
        //new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
        //new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
        //new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
      };

      this.SelectedTheme = Themes.First();
    }

    public event EventHandler ActiveDocumentChanged;

    public List<Tuple<string, Theme>> Themes { get; set; }

    public Tuple<string, Theme> SelectedTheme
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
          tools = new ToolViewModel[] { PreviewViewModel, SvgNestConfigViewModel, PropertiesViewModel };
        }

        return tools;
      }
    }

    public ReadOnlyObservableCollection<FileViewModel> Files
    {
      get
      {
        if (readonlyFiles == null)
        {
          readonlyFiles = new ReadOnlyObservableCollection<FileViewModel>(files);
        }

        return readonlyFiles;
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

    public SvgNestConfigViewModel SvgNestConfigViewModel
    {
      get
      {
        if (svgNestConfigViewModel == null)
        {
          svgNestConfigViewModel = new SvgNestConfigViewModel(this);
        }

        return svgNestConfigViewModel;
      }
    }

    public int SelectedPartIndex
    {
      get => selectedPartIndex;
      set
      {
        selectedPartIndex = value;
        OnPropertyChanged(nameof(SelectedPartIndex));
      }
    }

    public ObservablePartPlacement SelectedPartItem
    {
      get => selectedPartItem;
      set
      {
        selectedPartItem = value;
        OnPropertyChanged(nameof(SelectedPartItem));
        SheetPlacement.RaiseOnPropertyChangedDrawingContext();
      }
    }

    public ICommand ExecuteLoadSheetPlacement { get; }

    public ICommand ExecuteLoadNestProject { get; }

    public ICommand ExecuteSaveSheetPlacement { get; }

    public ICommand ExecuteSaveNestProject { get; }

    public ObservableSheetPlacement SheetPlacement { get; } = new ObservableSheetPlacement();

    public FileViewModel ActiveDocument
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

    public void LoadNestProject()
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = ProjectInfo.FileDialogFilter,
      };

      if (openFileDialog.ShowDialog() == true)
      {
        LoadNestProject(openFileDialog.FileName);
      }
    }

    public void LoadNestProject(string fileName)
    {
      var loaded = new NestProjectViewModel(this, fileName);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadPart()
    {
      var openFileDialog = new OpenFileDialog()
      {
        Filter = DeepNestLib.NFP.FileDialogFilter,
      };

      if (openFileDialog.ShowDialog() == true)
      {
        LoadPart(openFileDialog.FileName);
      }
    }

    public void LoadPart(string fileName)
    {
      var loaded = new PartViewModel(this, fileName);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    public void LoadSheetPlacement()
    {
      var openFileDialog = new OpenFileDialog()
      {
        Filter = DeepNestLib.Placement.SheetPlacement.FileDialogFilter,
      };

      if (openFileDialog.ShowDialog() == true)
      {
        LoadSheetPlacement(openFileDialog.FileName);
      }
    }

    public void LoadSheetPlacement(string fileName)
    {
      var loaded = new SheetPlacementViewModel(this, fileName);
      this.SheetPlacement.Set(loaded.SheetPlacement);
      this.files.Add(loaded);
      this.ActiveDocument = loaded;
    }

    internal void Close(FileViewModel fileToClose)
    {
      if (fileToClose.IsDirty)
      {
        var res = MessageBox.Show(string.Format("Save changes for file '{0}'?", fileToClose.FileName), "AvalonDock Test App", MessageBoxButton.YesNoCancel);
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

    internal void Save(FileViewModel fileToSave, bool saveAsFlag = false)
    {
      if (fileToSave.FilePath == null || saveAsFlag)
      {
        var dlg = new SaveFileDialog();
        if (dlg.ShowDialog().GetValueOrDefault())
        {
          fileToSave.FilePath = dlg.SafeFileName;
        }
      }

      if (fileToSave.FilePath == null)
      {
        return;
      }

      File.WriteAllText(fileToSave.FilePath, fileToSave.TextContent);
      ActiveDocument.IsDirty = false;
    }

    private void SaveNestProject()
    {
      throw new NotImplementedException();
    }

    private void SaveSheetPlacement()
    {
      throw new NotImplementedException();
    }
  }
}
