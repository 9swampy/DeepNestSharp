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
      IAboutDialogService dialog = this.aboutDialogFactory();
      return dialog.ShowDialog();
    }
  }
}