namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.ViewModels;  

  public interface IMainViewModel
  {
    IFileViewModel? ActiveDocument { get; set; }
    ICommand ActiveDocumentSaveAsCommand { get; }
    ICommand ActiveDocumentSaveCommand { get; }
    ICommand CreateNestProjectCommand { get; }
    IDispatcherService DispatcherService { get; }
    IDockingManagerFacade? DockManager { get; set; }
    ICommand ExitCommand { get; }
    ReadOnlyObservableCollection<IFileViewModel> Files { get; }
    ICommand LoadLayoutCommand { get; }
    ICommand LoadNestProjectCommand { get; }
    ICommand LoadNestResultCommand { get; }
    ICommand LoadNfpCandidatesCommand { get; }
    ICommand LoadPartCommand { get; }
    ICommand LoadSheetNfpCommand { get; }
    ICommand LoadSheetPlacementCommand { get; }
    IMessageService MessageService { get; }
    INestMonitorViewModel NestMonitorViewModel { get; }
    IPreviewViewModel PreviewViewModel { get; }
    IPropertiesViewModel PropertiesViewModel { get; }
    ICommand SaveLayoutCommand { get; }

    ISvgNestConfigViewModel SvgNestConfigViewModel { get; }

    IEnumerable<IToolViewModel> Tools { get; }

    event EventHandler ActiveDocumentChanged;

    void LoadNestResult(string filePath);
    void LoadNfpCandidates(string filePath);
    void LoadPart(string filePath);
    void LoadSheetNfp(string filePath);
    void LoadSheetPlacement(ISheetPlacement sheetPlacement);
    void LoadSheetPlacement(string filePath);
    void OnLoadNestProject(string filePath);
    Task OnLoadNestProjectAsync();
    void OnLoadNestResult(INestResult nestResult);
    Task OnLoadNestResultAsync();
    Task OnLoadPartAsync();
    Task OnLoadSheetPlacementAsync();
    void Close(IFileViewModel fileViewModel);
    void Save(IFileViewModel fileViewModel, bool v);
    Task ExportSheetPlacementAsync(ISheetPlacement? sheetPlacement);
  }
}