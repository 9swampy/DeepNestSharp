namespace DeepNestSharp
{
  using System.IO;
  using System.Windows;
  using DeepNestSharp.Ui.ViewModels;

  public partial class MainWindow : Window
  {
    public MainWindow(MainViewModel viewModel)
    {
      InitializeComponent();
      this.DataContext = viewModel;
      viewModel.DockManager = this.dockManager;
      this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
      this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
      // ViewModel.LoadSheetPlacement(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\300x200GoodSwitchbackNest.dnsp");
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
  }
}
