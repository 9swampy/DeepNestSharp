namespace DeepNestSharp
{
  using System;
  using System.IO;
  using System.Text;
  using System.Windows;
  using DeepNestLib;
  using DeepNestSharp.Domain;
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
             services.AddScoped<IFileIoService, FileIoService>();
             services.AddScoped<IMessageService, MessageBoxService>();
             services.AddTransient<INestProjectViewModel, NestProjectViewModel>();
             services.AddSingleton(SvgNest.Config);
             services.AddSingleton<IDispatcherService>(new DispatcherService(this.Dispatcher));
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

          var args = Environment.GetCommandLineArgs();
          if (args != null && args.Length > 0)
          {
            var mainViewModel = services.GetRequiredService<MainViewModel>();
            foreach (var arg in args)
            {
              var fileInfo = new FileInfo(arg);
              if (fileInfo.Exists)
              {
                if (fileInfo.Extension == ".dll")
                {
                  //NOP
                }
                else if(fileInfo.Extension == ".dnr")
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
                else if (fileInfo.Extension == ".dxf")
                {
                  mainViewModel.LoadPart(fileInfo.FullName);
                }
                else
                {
                  var message = new StringBuilder();
                  message.AppendLine("Files supported include: ");
                  message.AppendLine("  DeepNestProject (*.dnest)");
                  message.AppendLine("  DeepNestResult (*.dnr)");
                  message.AppendLine("  DeepNestSheetPlacement (*.dnsp) and");
                  message.AppendLine("  AutoCad Drawing Exchange Format (*.dxf)");
                  if (MessageBox.Show(message.ToString(), "DeepNest", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
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
        }
      }
    }
  }
}
