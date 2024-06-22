namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;

  /// <summary>
  /// Interaction logic for NestResultEditor.xaml
  /// </summary>
  public partial class NestResultEditor : UserControl
  {
    private HorizontalScrollHandler horizontalScrollHandler;

    public NestResultEditor()
    {
      horizontalScrollHandler = new HorizontalScrollHandler();
      this.InitializeComponent();
    }

    private void HandleHorizontalScroll(object sender, MouseWheelEventArgs e)
    {
      horizontalScrollHandler.ListView_PreviewMouseWheel(sender, e);
    }
  }
}
