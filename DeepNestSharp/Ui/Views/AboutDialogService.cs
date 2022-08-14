namespace DeepNestSharp.Ui.Views
{
  using System;
  using DeepNestSharp.Domain.Services;

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