namespace DeepNestSharp
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Text;
  using System.Windows;
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Domain.ViewModels;
  using DeepNestSharp.Ui.Services;
  using DeepNestSharp.Ui.ViewModels;
  using DeepNestSharp.Ui.Views;
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
             services.AddSingleton<IRelativePathHelper>(new RelativePathHelper(ResourceAssembly.Location));
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
          var args = Environment.GetCommandLineArgs();
          if (HasCmdLineArgs(args))
          {
            HandleCmdLineArgs(services, args);
          }

          var mainWindow = services.GetRequiredService<MainWindow>();
          mainWindow?.Show();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          throw;
        }
      }
    }

    private static void HandleCmdLineArgs(IServiceProvider services, string[] args)
    {
      var mainViewModel = services.GetRequiredService<IMainViewModel>();
      foreach (var arg in args)
      {
        var filePath = arg;
        filePath = filePath.Replace("${SolutionDir}", services.GetRequiredService<IRelativePathHelper>().GetSolutionDirectory());
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists)
        {
          if (fileInfo.Extension == ".dll" ||
              fileInfo.Extension == ".exe")
          {
            //NOP
          }
          else if (NestResult.FileDialogFilter.Contains(fileInfo.Extension))
          {
            mainViewModel.LoadNestResult(fileInfo.FullName);
          }
          else if (ProjectInfo.FileDialogFilter.Contains(fileInfo.Extension))
          {
            mainViewModel.OnLoadNestProject(fileInfo.FullName);
          }
          else if (SheetPlacement.FileDialogFilter.Contains(fileInfo.Extension))
          {
            mainViewModel.LoadSheetPlacement(fileInfo.FullName);
          }
          else if (NoFitPolygon.FileDialogFilter.Contains(fileInfo.Extension))
          {
            mainViewModel.LoadPart(fileInfo.FullName);
          }
          else
          {
            ShowExitDialog(fileInfo, $"Unable to load {fileInfo.Name}.");
          }
        }
        else
        {
          ShowExitDialog(fileInfo, $"{fileInfo.Name} does not exist.");
        }
      }
    }

    private static void ShowExitDialog(FileInfo fileInfo, string caption)
    {
      if (Application.Current != null)
      {
        var message = new StringBuilder();
        message.AppendLine("Files supported include: ");
        message.AppendLine($"  {TruncateWildcardExtension(ProjectInfo.FileDialogFilter)}");
        message.AppendLine($"  {TruncateWildcardExtension(NestResult.FileDialogFilter)}");
        message.AppendLine($"  {TruncateWildcardExtension(SheetPlacement.FileDialogFilter)}");
        message.AppendLine($"  {TruncateWildcardExtension(NoFitPolygon.FileDialogFilter)}");
        if (MessageBox.Show(message.ToString(), $"DeepNest: {caption}", MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.Cancel)
        {
          Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
          while (Application.Current != null)
          {
            Process.GetCurrentProcess().Kill();
          }
        }
      }
    }

    private static string TruncateWildcardExtension(string fileDialogFilter)
    {
      return fileDialogFilter.Substring(0, NestResult.FileDialogFilter.IndexOf("|All"));
    }

    private static bool HasCmdLineArgs(string[] args)
    {
      return args != null && args.Length > 0;
    }
  }
}
