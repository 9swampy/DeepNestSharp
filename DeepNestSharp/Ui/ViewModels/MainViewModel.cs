namespace DeepNestSharp.Ui.ViewModels
{
  using System.IO;
  using System.Windows.Input;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.ComponentModel;
  using Microsoft.Toolkit.Mvvm.Input;
  using Microsoft.Win32;

  public class MainViewModel : ObservableRecipient
  {
    private int selectedPartIndex;
    private ObservablePartPlacement selectedPartItem;
    private ObservableSheetPlacement sheetPlacement = new ObservableSheetPlacement();

    public MainViewModel()
    {
      ExecuteLoadSheetPlacement = new RelayCommand(LoadSheetPlacement);
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

    /// <summary>
    /// Gets or sets the currently selected SheetPlacement.
    /// </summary>
    public ObservableSheetPlacement SheetPlacement
    {
      get => sheetPlacement;
    }

    public void LoadSheetPlacement()
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        var json = File.ReadAllText(openFileDialog.FileName);
        var sheetPlacement = DeepNestLib.Placement.SheetPlacement.FromJson(json);
        this.sheetPlacement.Set(sheetPlacement);
      }
    }
  }
}
