namespace DeepNestPort
{
  using System.Windows.Forms;
  using DeepNestLib;

  internal class MessageBoxService : IMessageService
  {
    public void DisplayMessage(string message)
    {
      MessageBox.Show(message);
    }
  }
}