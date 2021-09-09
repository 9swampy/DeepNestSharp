namespace DeepNestSharp.Ui.Views
{
  using DeepNestSharp.Domain.Services;
  using System;

  public class AboutDialogService : IAboutDialogService
  {
    private readonly Func<IAboutDialogService> aboutDialogFactory;

    public AboutDialogService(Func<IAboutDialogService> aboutDialogFactory)
    {
      this.aboutDialogFactory = aboutDialogFactory;
    }

    public bool? ShowDialog()
    {
      var dialog = aboutDialogFactory();
      return dialog.ShowDialog();
    }
  }
}