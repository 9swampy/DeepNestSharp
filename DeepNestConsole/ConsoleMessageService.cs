namespace DeepNestConsole
{
  using System;
  using DeepNestLib;

  public class ConsoleMessageService : IMessageService
  {
    public void DisplayMessage(string message)
    {
      Console.WriteLine(message);
    }

    public void DisplayMessage(Exception ex)
    {
      Console.WriteLine($"{ex.Message}/n{ex.StackTrace}");
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      Console.WriteLine($"--------------------------------------------------------------/n" +
                        $"{caption}/n" +
                        $"{text}/n" +
                        $"--------------------------------------------------------------");
    }

    public MessageBoxResult DisplayOkCancel(string text, string caption, MessageBoxIcon icon)
    {
      throw new NotImplementedException();
    }

    public MessageBoxResult DisplayYesNoCancel(string text, string caption, MessageBoxIcon icon)
    {
      throw new NotImplementedException();
    }
  }
}
