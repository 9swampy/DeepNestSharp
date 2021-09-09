namespace DeepNestSharp.Ui.Views
{
  using System.Reflection;
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
      Title = $"About DeepNest# {ProcessorArchitecture}:{InformationalVersion}";
      this.DataContext = this;
    }

    public string Version => this.GetType().Assembly.GetName().Version?.ToString();

    public string ProcessorArchitecture => this.GetType().Assembly.GetName().ProcessorArchitecture.ToString();

    public string InformationalVersion => this.GetType()
                                              .Assembly
                                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                              .InformationalVersion ?? string.Empty;

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
    }

    private void OkClick(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
