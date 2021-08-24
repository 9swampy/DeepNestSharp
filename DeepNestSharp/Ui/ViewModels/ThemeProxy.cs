namespace DeepNestSharp.Ui.ViewModels
{
  using AvalonDock.Themes;
  using System;
  using System.Windows;

  public class ThemeProxy : DependencyObject
  {
    private readonly Theme theme;

    public ThemeProxy(Theme theme)
    {
      this.theme = theme;
    }

    public Uri GetResourceUri() => theme.GetResourceUri();
  }
}