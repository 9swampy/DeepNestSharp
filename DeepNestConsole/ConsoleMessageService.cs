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
  }
}
