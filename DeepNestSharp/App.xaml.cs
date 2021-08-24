namespace DeepNestSharp
{
  using System;
  using System.IO;
  using System.Text;
  using System.Windows;
  using DeepNestLib;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Domain.ViewModels;
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
             services.AddScoped<IMouseCursorService, MouseCursorService>();
             services.AddScoped<IFileIoService, FileIoService>();
             services.AddScoped<IMessageService, MessageBoxService>();
             services.AddTransient<INestProjectViewModel, NestProjectViewModel>();
             services.AddSingleton(SvgNest.Config);
             services.AddSingleton<IDispatcherService>(new DispatcherService(this.Dispatcher));
             services.AddSingleton<IMainViewModel, DockingMainViewModel>();
             services.AddSingleton<MainWindow>();
           }).Build();

      using (var serviceScope = host.Services.CreateScope())
      {
        var services = serviceScope.ServiceProvider;
        try
        {
          var mainWindow = services.GetRequiredService<MainWindow>();
          mainWindow?.Show();

          var args = Environment.GetCommandLineArgs();
          if (args != null && args.Length > 0)
          {
            var mainViewModel = services.GetRequiredService<IMainViewModel>();
            foreach (var arg in args)
            {
              var fileInfo = new FileInfo(arg);
              if (fileInfo.Exists)
              {
                if (fileInfo.Extension == ".dll" ||
                    fileInfo.Extension == ".exe")
                {
                  //NOP
                }
                else if (fileInfo.Extension == ".dnr")
                {
                  mainViewModel.LoadNestResult(fileInfo.FullName);
                }
                else if (fileInfo.Extension == ".dnest")
                {
                  mainViewModel.OnLoadNestProject(fileInfo.FullName);
                }
                else if (fileInfo.Extension == ".dnsp")
                {
                  mainViewModel.LoadSheetPlacement(fileInfo.FullName);
                }
                else if (fileInfo.Extension == ".dxf" ||
                         fileInfo.Extension == ".dnpoly")
                {
                  mainViewModel.LoadPart(fileInfo.FullName);
                }
                else
                {
                  var message = new StringBuilder();
                  message.AppendLine($"Unable to load {fileInfo.Name}");
                  message.AppendLine("Files supported include: ");
                  message.AppendLine("  DeepNest Project (*.dnest)");
                  message.AppendLine("  DeepNest Result (*.dnr)");
                  message.AppendLine("  DeepNest Sheet Placement (*.dnsp)");
                  message.AppendLine("  AutoCad Drawing Exchange Format (*.dxf) and ");
                  message.AppendLine("  DeepNest Polygon (*.dnpoly).");
                  if (MessageBox.Show(message.ToString(), "DeepNest", MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.Cancel)
                  {
                    Application.Current.MainWindow.Close();
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          throw;
        }
      }
    }
  }
}
