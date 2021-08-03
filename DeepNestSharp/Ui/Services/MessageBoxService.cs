namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Windows;
  using DeepNestLib;

  internal class MessageBoxService : IMessageService
  {
    public void DisplayMessage(string message)
    {
      MessageBox.Show(message);
    }

    public void DisplayMessage(Exception ex)
    {
      MessageBox.Show($"{ex.Message}/n{ex.StackTrace}");
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      MessageBox.Show(text, caption, MessageBoxButton.OK, (MessageBoxImage)icon);
    }
  }
}