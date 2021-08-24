namespace DeepNestPort
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

    public void DisplayMessageBox(string text, string caption, DeepNestLib.MessageBoxIcon icon)
    {
      MessageBox.Show(text, caption, MessageBoxButtons.OK, (System.Windows.Forms.MessageBoxIcon)icon);
    }

    public MessageBoxResult DisplayOkCancel(string text, string caption, DeepNestLib.MessageBoxIcon icon)
    {
      throw new NotImplementedException();
    }

    public MessageBoxResult DisplayYesNoCancel(string text, string caption, DeepNestLib.MessageBoxIcon icon)
    {
      throw new NotImplementedException();
    }
  }
}