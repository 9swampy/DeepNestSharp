namespace DeepNestSharp
{
  using System;
  using System.Windows.Forms;
  using DeepNestLib;

  internal class MessageBoxService : IMessageService
  {
    public readonly static MessageBoxService Default = new MessageBoxService();
    private int errorMessageCount = 0;

    public void DisplayMessage(string message)
    {
      MessageBox.Show(message);
    }

    public void DisplayMessage(Exception ex)
    {
      MessageBox.Show($"{ex.Message}/n{ex.StackTrace}");
    }

    public void ShowMessage(Exception ex)
    {
      errorMessageCount++;
      string message = ex.Message + "/r" + ex.GetType().Name + "/r" + ex.StackTrace;

      if (errorMessageCount <= 3)
      {
        this.ShowMessage(message, MessageBoxIcon.Error);
      }
      else if (errorMessageCount > 10)
      {
        this.ShowMessage(message, MessageBoxIcon.Stop);
        Application.Exit();
      }
    }

    public void ShowMessage(string text, MessageBoxIcon type)
    {
      MessageBox.Show(text, "DeepNest#", MessageBoxButtons.OK, type);
    }

    public DialogResult ShowQuestion(string text)
    {
      return MessageBox.Show(text, "DeepNest#", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }
  }
}