﻿namespace DeepNestSharp.Ui.Docking
{
  using System.IO;
  using System.Windows.Input;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.ViewModels;
  //using System.Windows.Media;
  using Microsoft.Toolkit.Mvvm.Input;

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
      FilePath = filePath;
      Title = FileName;
      LoadFile(filePath);
      IsDirty = false;

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
      IsDirty = true;
      Title = FileName;
    }

    public string DirectoryName
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(FilePath))
        {
          return new FileInfo(FilePath).DirectoryName;
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
        if (string.IsNullOrWhiteSpace(FilePath))
        {
          return $"New{FileTypeName}" + (IsDirty ? "*" : string.Empty);
        }

        return Path.GetFileName(FilePath) + (IsDirty ? "*" : string.Empty);
      }
    }

    /// <summary>
    /// Gets or sets the full path to file (path and file name).
    /// </summary>
    public string FilePath
    {
      get => filePath ?? string.Empty;
      set
      {
        if (filePath != value)
        {
          filePath = value;
          Title = FileName;
          OnPropertyChanged(nameof(FilePath));
          OnPropertyChanged(nameof(FileName));
          OnPropertyChanged(nameof(Title));
        }
      }
    }

    /// <summary>
    /// Gets the default name for a new file of this type.
    /// </summary>
    public string FileTypeName => GetType().Name.Replace("ViewModel", string.Empty);

    public abstract string TextContent { get; }

    public bool IsDirty
    {
      get => isDirty;
      set
      {
        if (isDirty != value)
        {
          isDirty = value;
          if (!isDirty)
          {
            SaveState();
          }

          OnPropertyChanged(nameof(IsDirty));
          OnPropertyChanged(nameof(FileName));
        }
      }
    }

    public ICommand SaveCommand
    {
      get
      {
        if (saveCommand == null)
        {
          saveCommand = new RelayCommand(OnSave, CanSave);
        }

        return saveCommand;
      }
    }

    public ICommand SaveAsCommand
    {
      get
      {
        if (saveAsCommand == null)
        {
          saveAsCommand = new RelayCommand(OnSaveAs, CanSaveAs);
        }

        return saveAsCommand;
      }
    }

    public ICommand CloseCommand
    {
      get
      {
        if (closeCommand == null)
        {
          closeCommand = new RelayCommand(OnClose, CanClose);
        }

        return closeCommand;
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
        LoadContent();
        ContentId = filePath;
        NotifyContentUpdated();
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
      MainViewModel.Close(this);
    }

    private bool CanSave()
    {
      return IsDirty;
    }

    private void OnSave()
    {
      MainViewModel.Save(this, false);
    }

    private bool CanSaveAs()
    {
      return IsDirty;
    }

    private void OnSaveAs()
    {
      MainViewModel.Save(this, true);
    }
  }
}
