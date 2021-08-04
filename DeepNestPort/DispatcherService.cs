namespace DeepNestPort
{
  using DeepNestLib;
  using System;
  using System.Reflection;
  using System.Windows.Forms;
  using System.Windows.Threading;

  public class DispatcherService : IDispatcherService
  {
    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

    public bool InvokeRequired => !dispatcher.CheckAccess();

    public void Invoke(Action callback)
    {
      dispatcher.Invoke(callback);
    }
  }
}
