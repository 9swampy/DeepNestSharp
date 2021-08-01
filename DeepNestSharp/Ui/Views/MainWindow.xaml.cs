namespace DeepNestSharp
{
  using System.IO;
  using System.Windows;
  using System.Windows.Input;
  using AvalonDock.Layout.Serialization;
  using DeepNestSharp.Ui.ViewModels;
  using Microsoft.Toolkit.Mvvm.Input;

  public partial class MainWindow : Window
  {
    private RelayCommand loadLayoutCommand = null;
    private RelayCommand saveLayoutCommand = null;

    public MainWindow(MainViewModel viewModel)
    {
      InitializeComponent();
      this.DataContext = viewModel;

      this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
      this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public ICommand LoadLayoutCommand
    {
      get
      {
        if (loadLayoutCommand == null)
        {
          loadLayoutCommand = new RelayCommand(OnLoadLayout, CanLoadLayout);
        }

        return loadLayoutCommand;
      }
    }

    public ICommand SaveLayoutCommand
    {
      get
      {
        if (saveLayoutCommand == null)
        {
          saveLayoutCommand = new RelayCommand(OnSaveLayout, CanSaveLayout);
        }

        return saveLayoutCommand;
      }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
      ViewModel.LoadSheetPlacement(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\300x200GoodSwitchbackNest.dnsp");
      // ViewModel.LoadNestProject(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\SwitchbacksAndFront295x195.dnest");
      // ViewModel.LoadPart(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\FrontWall.dxf");
#endif
      return;

      var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
      serializer.LayoutSerializationCallback += (s, args) =>
      {
        args.Content = args.Content;
      };

      if (File.Exists(@".\AvalonDock.config"))
      {
        serializer.Deserialize(@".\AvalonDock.config");
      }
    }

    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
      var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
      serializer.Serialize(@".\AvalonDock.config");
    }

    private bool CanLoadLayout()
    {
      return File.Exists(@".\AvalonDock.Layout.config");
    }

    private void OnLoadLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(dockManager);
      layoutSerializer.Deserialize(@".\AvalonDock.Layout.config");
    }

    private bool CanSaveLayout()
    {
      return true;
    }

    private void OnSaveLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(dockManager);
      layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
    }
  }
}
