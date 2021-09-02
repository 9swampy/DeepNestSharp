namespace DeepNestSharp.Ui.Views
{
  using System.Windows;
  using System.Windows.Navigation;
  using DeepNestSharp.Domain.Services;

  /// <summary>
  /// Interaction logic for AboutDialog.xaml
  /// </summary>
  public partial class AboutDialog : Window, IAboutDialogService
  {
    public AboutDialog()
    {
      InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
