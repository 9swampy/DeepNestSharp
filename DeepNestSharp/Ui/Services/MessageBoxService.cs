namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Windows;
  using DeepNestLib;

  internal class MessageBoxService : IMessageService
  {
    private readonly IDispatcherService dispatcherService;

    public MessageBoxService(IDispatcherService dispatcherService)
    {
      this.dispatcherService = dispatcherService;
    }

    public void DisplayMessage(string message)
    {
      this.dispatcherService.Invoke(() => MessageBox.Show(message));
    }

    public void DisplayMessage(Exception ex)
    {
      this.dispatcherService.Invoke(() => MessageBox.Show($"{ex.Message}/n{ex.StackTrace}"));
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      this.dispatcherService.Invoke(() => MessageBox.Show(text, caption, MessageBoxButton.OK, (MessageBoxImage)icon));
    }

    public DeepNestLib.MessageBoxResult DisplayOkCancel(string text, string caption, MessageBoxIcon icon)
    {
      DeepNestLib.MessageBoxResult result = DeepNestLib.MessageBoxResult.Cancel;
      this.dispatcherService.Invoke(() => result = (DeepNestLib.MessageBoxResult)MessageBox.Show(text, caption, MessageBoxButton.OKCancel, (MessageBoxImage)icon, System.Windows.MessageBoxResult.Cancel));
      return result;
    }

    public DeepNestLib.MessageBoxResult DisplayYesNoCancel(string text, string caption, MessageBoxIcon icon)
    {
      DeepNestLib.MessageBoxResult result = DeepNestLib.MessageBoxResult.Cancel;
      this.dispatcherService.Invoke(() => result = (DeepNestLib.MessageBoxResult)MessageBox.Show(text, caption, MessageBoxButton.YesNoCancel, (MessageBoxImage)icon, System.Windows.MessageBoxResult.Cancel));
      return result;
    }
  }
}