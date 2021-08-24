namespace DeepNestSharp.Ui.ViewModels
{
  using System.IO;
  using DeepNestLib;
  using DeepNestSharp.Ui.Docking;

  public class NfpCandidateListViewModel : FileViewModel
  {
    private INfpCandidateList? nfpCandidateList;
    private int selectedIndex;
    private INfp selectedItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="NfpCandidateListViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public NfpCandidateListViewModel(IMainViewModel mainViewModel)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NfpCandidateListViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NfpCandidateListViewModel(IMainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public INfpCandidateList? NfpCandidateList
    {
      get
      {
        return this.nfpCandidateList;
      }

      set
      {
        SetProperty(ref nfpCandidateList, value, nameof(NfpCandidateList));
      }
    }

    public override string FileDialogFilter => SheetNfp.FileDialogFilter;

    public int SelectedIndex
    {
      get => selectedIndex;
      set => SetProperty(ref selectedIndex, value);
    }

    public INfp SelectedItem
    {
      get => selectedItem;
      set
      {
        SetProperty(ref selectedItem, value, nameof(SelectedItem));
      }
    }

    public override string TextContent => this.NfpCandidateList?.ToJson() ?? string.Empty;

    protected override void LoadContent()
    {
      var fileInfo = new FileInfo(this.FilePath);
      if (!fileInfo.Exists)
      {
        this.MainViewModel.MessageService.DisplayMessageBox($"File not found: {this.FilePath}.", "File Not Found", MessageBoxIcon.Information);
        return;
      }

      if (fileInfo.Extension == ".dnsnfp")
      {
        this.NfpCandidateList = SheetNfp.LoadFromFile(this.FilePath);
      }
      else
      {
        this.NfpCandidateList = DeepNestLib.NfpCandidateList.LoadFromFile(this.FilePath);
      }
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(NfpCandidateList));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }
  }
}