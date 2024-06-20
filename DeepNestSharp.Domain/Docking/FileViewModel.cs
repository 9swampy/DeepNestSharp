namespace DeepNestSharp.Ui.Docking
{
  using System.IO;
  using System.Windows.Input;
  //using System.Windows.Media;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.ViewModels;

  public abstract class FileViewModel : PaneViewModel, IFileViewModel
  {
    //private static ImageSourceConverter imageSourceConverter = new ImageSourceConverter();

    private string? filePath;
    private bool isDirty = false;
    private RelayCommand? saveCommand;
    private RelayCommand? saveAsCommand;
    private RelayCommand? closeCommand;

    public FileViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileViewModel"/> class.
    /// Use this to access an existing file.
    /// </summary>
    public FileViewModel(IMainViewModel mainViewModel, string filePath)
    {
      this.MainViewModel = mainViewModel;
      this.FilePath = filePath;
      this.Title = this.FileName;
      this.LoadFile(filePath);
      this.IsDirty = false;

      // Set the icon only for open documents (just a test)
      // IconSource = imageSourceConverter.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileViewModel"/> class.
    /// Use this to access an new file.
    /// </summary>
    public FileViewModel(IMainViewModel mainViewModel)
    {
      this.MainViewModel = mainViewModel;
      this.IsDirty = true;
      this.Title = this.FileName;
    }

    public string DirectoryName
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.FilePath))
        {
          return new FileInfo(this.FilePath).DirectoryName;
        }

        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the filter to apply to Open/Save file dialogs.
    /// </summary>
    public abstract string FileDialogFilter { get; }

    /// <summary>
    /// Gets the name of the file, excluding path.
    /// </summary>
    public string FileName
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.FilePath))
        {
          return $"New{this.FileTypeName}" + (this.IsDirty ? "*" : string.Empty);
        }

        return Path.GetFileName(this.FilePath) + (this.IsDirty ? "*" : string.Empty);
      }
    }

    /// <summary>
    /// Gets or sets the full path to file (path and file name).
    /// </summary>
    public string FilePath
    {
      get => this.filePath ?? string.Empty;
      set
      {
        if (this.filePath != value)
        {
          this.filePath = value;
          this.Title = this.FileName;
          this.OnPropertyChanged(nameof(this.FilePath));
          this.OnPropertyChanged(nameof(this.FileName));
          this.OnPropertyChanged(nameof(this.Title));
        }
      }
    }

    /// <summary>
    /// Gets the default name for a new file of this type.
    /// </summary>
    public string FileTypeName => this.GetType().Name.Replace("ViewModel", string.Empty);

    public abstract string TextContent { get; }

    public bool IsDirty
    {
      get => this.isDirty;
      set
      {
        if (this.isDirty != value)
        {
          this.isDirty = value;
          if (!this.isDirty)
          {
            this.SaveState();
          }

          this.OnPropertyChanged(nameof(this.IsDirty));
          this.OnPropertyChanged(nameof(this.FileName));
        }
      }
    }

    public ICommand SaveCommand
    {
      get
      {
        if (this.saveCommand == null)
        {
          this.saveCommand = new RelayCommand(this.OnSave, this.CanSave);
        }

        return this.saveCommand;
      }
    }

    public ICommand SaveAsCommand
    {
      get
      {
        if (this.saveAsCommand == null)
        {
          this.saveAsCommand = new RelayCommand(this.OnSaveAs, this.CanSaveAs);
        }

        return this.saveAsCommand;
      }
    }

    public ICommand CloseCommand
    {
      get
      {
        if (this.closeCommand == null)
        {
          this.closeCommand = new RelayCommand(this.OnClose, this.CanClose);
        }

        return this.closeCommand;
      }
    }

    protected IMainViewModel MainViewModel { get; }

    protected abstract void NotifyContentUpdated();

    protected abstract void SaveState();

    protected abstract void LoadContent();

    private bool CanClose()
    {
      return true;
    }

    private void LoadFile(string filePath)
    {
      if (File.Exists(filePath))
      {
        this.LoadContent();
        this.ContentId = filePath;
        this.NotifyContentUpdated();
        if (this is NestProjectViewModel ||
            this is NestResultViewModel ||
            this is PartEditorViewModel)
        {
          this.MainViewModel.SvgNestConfigViewModel.SvgNestConfig.LastNestFilePath = this.DirectoryName;
        }
        else if (this is NfpCandidateListViewModel ||
                 this is SheetPlacementViewModel)
        {
          this.MainViewModel.SvgNestConfigViewModel.SvgNestConfig.LastDebugFilePath = this.DirectoryName;
        }
      }
    }

    private void OnClose()
    {
      this.MainViewModel.Close(this);
    }

    private bool CanSave()
    {
      return this.IsDirty;
    }

    private void OnSave()
    {
      this.MainViewModel.Save(this, false);
    }

    private bool CanSaveAs()
    {
      return this.IsDirty;
    }

    private void OnSaveAs()
    {
      this.MainViewModel.Save(this, true);
    }
  }
}
