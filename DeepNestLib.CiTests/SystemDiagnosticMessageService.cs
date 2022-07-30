namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib;

  internal class SystemDiagnosticMessageService : IMessageService
  {
    public void DisplayMessage(string message)
    {
      throw new NotImplementedException();
    }

    public void DisplayMessage(Exception ex)
    {
      System.Diagnostics.Debug.Print("DisplayMessage");
      System.Diagnostics.Debug.Print(ex.ToString());
      System.Diagnostics.Debug.Print(ex.StackTrace?.ToString());
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      System.Diagnostics.Debug.Print("DisplayMessageBox");
      System.Diagnostics.Debug.Print(text);
      System.Diagnostics.Debug.Print(caption);
    }

    public MessageBoxResult DisplayOkCancel(string text, string caption, MessageBoxIcon icon)
    {
      System.Diagnostics.Debug.Print("DisplayOkCancel");
      System.Diagnostics.Debug.Print(text);
      System.Diagnostics.Debug.Print(caption);
      throw new NotImplementedException();
    }

    public MessageBoxResult DisplayYesNoCancel(string text, string caption, MessageBoxIcon icon)
    {
      System.Diagnostics.Debug.Print("DisplayYesNoCancel");
      System.Diagnostics.Debug.Print(text);
      System.Diagnostics.Debug.Print(caption);
      throw new NotImplementedException();
    }
  }
}