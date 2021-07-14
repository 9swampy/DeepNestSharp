namespace DeepNestSharp
{
  using System;
  using System.Windows.Forms;
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
  }
}