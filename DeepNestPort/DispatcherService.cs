namespace DeepNestPort
{
  using System;
  using System.Threading.Tasks;
  using System.Windows.Threading;
  using DeepNestLib;

  public class DispatcherService : IDispatcherService
  {
    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

    public bool InvokeRequired => !dispatcher.CheckAccess();

    public void Invoke(Action callback)
    {
      dispatcher.Invoke(callback);
    }

    public async Task InvokeAsync(Action callback)
    {
      await dispatcher.InvokeAsync(callback).Task.ConfigureAwait(false);
    }
  }
}
