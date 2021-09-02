namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using AvalonDock.Themes;
  using DeepNestLib;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Domain.ViewModels;

  public class DockingMainViewModel : MainViewModel
  {
    private Tuple<string, Theme>? selectedTheme;

    public DockingMainViewModel(IMessageService messageService, IDispatcherService dispatcherService, ISvgNestConfig config, IFileIoService fileIoService, IMouseCursorService mouseCursorService)
      : base(messageService, dispatcherService, config, fileIoService, mouseCursorService)
    {
      this.Themes = new List<Tuple<string, Theme>>
      {
        new Tuple<string, Theme>(nameof(GenericTheme), new GenericTheme()),

        // new Tuple<string, Theme>(nameof(AeroTheme),new AeroTheme()),
        // new Tuple<string, Theme>(nameof(ExpressionDarkTheme),new ExpressionDarkTheme()),
        // new Tuple<string, Theme>(nameof(ExpressionLightTheme),new ExpressionLightTheme()),
        // new Tuple<string, Theme>(nameof(MetroTheme),new MetroTheme()),
        // new Tuple<string, Theme>(nameof(VS2010Theme),new VS2010Theme()),
        // new Tuple<string, Theme>(nameof(Vs2013BlueTheme),new Vs2013BlueTheme()),
        // new Tuple<string, Theme>(nameof(Vs2013DarkTheme),new Vs2013DarkTheme()),
        // new Tuple<string, Theme>(nameof(Vs2013LightTheme),new Vs2013LightTheme()),
      };

      this.SelectedTheme = Themes.First();
    }

    public List<Tuple<string, Theme>> Themes { get; set; }

    public Tuple<string, Theme>? SelectedTheme
    {
      get => selectedTheme;

      set
      {
        selectedTheme = value;
        OnPropertyChanged(nameof(SelectedTheme));
      }
    }

    protected override void OnExit()
    {
      Application.Current.MainWindow.Close();
    }
  }
}
