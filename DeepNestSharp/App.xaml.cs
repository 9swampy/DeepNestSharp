namespace DeepNestSharp
{
  using System;
  using System.Windows;
  using DeepNestSharp.Ui.Services;
  using DeepNestSharp.Ui.ViewModels;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;

  /// <summary>
  /// Interaction logic for App.xaml.
  /// </summary>
  public partial class App : Application
  {
    private readonly IHost host;

    public App()
    {
      host = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
           {
             services.AddScoped<ISettingsService, SettingsService>();
             services.AddSingleton<MainViewModel>();
             services.AddSingleton<MainWindow>();
           }).Build();

      using (var serviceScope = host.Services.CreateScope())
      {
        var services = serviceScope.ServiceProvider;
        try
        {
          var mainWindow = services.GetRequiredService<MainWindow>();
          mainWindow?.Show();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }
  }
}
