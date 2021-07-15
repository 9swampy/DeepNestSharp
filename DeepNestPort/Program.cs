namespace DeepNestSharp
{
  using System;
  using System.Windows.Forms;

  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      var mainForm = new Form1();
      Application.Run(mainForm);
    }
  }
}
