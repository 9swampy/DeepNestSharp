namespace DeepNestSharp.Ui.Views
{
  using System.IO;
  using System.Windows;
  using DeepNestSharp.Domain.ViewModels;
  using DeepNestSharp.Ui.Docking;

  public partial class MainWindow : Window
  {
    public MainWindow(IMainViewModel viewModel)
    {
      this.InitializeComponent();
      this.DataContext = viewModel;
      viewModel.DockManager = new DockingManagerFacade(this.dockManager);
      viewModel.AboutDialogService = new AboutDialogService(() => new AboutDialog());
      this.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
      this.Unloaded += new RoutedEventHandler(this.MainWindow_Unloaded);
    }

    public IMainViewModel ViewModel => (IMainViewModel)this.DataContext;

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
      // ViewModel.LoadSheetPlacement(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\300x200GoodSwitchbackNest.dnsp");
      // ViewModel.LoadNestProject(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\SwitchbacksAndFront295x195.dnest");
      // ViewModel.LoadPart(@"C:\Git\CanDispenserCnc\CuttingLists\Std320x164\FrontWall.dxf");
#endif
      return;

      AvalonDock.Layout.Serialization.XmlLayoutSerializer serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(this.dockManager);
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
      AvalonDock.Layout.Serialization.XmlLayoutSerializer serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(this.dockManager);
      serializer.Serialize(@".\AvalonDock.config");
    }
  }
}
