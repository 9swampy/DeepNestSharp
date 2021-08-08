namespace DeepNestSharp.Ui.Services
{
  using System;
  using System.Windows.Threading;
  using DeepNestLib;

  public class DispatcherService : IDispatcherService
  {
    private Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;

    public DispatcherService(Dispatcher dispatcher)
    {
      this.dispatcher = dispatcher;
    }

    public bool InvokeRequired => !dispatcher.CheckAccess();

    public void Invoke(Action callback) => dispatcher.Invoke(callback);

    //public void Invoke(Action callback)
    //{
    //  var uiContext = SynchronizationContext.Current;
    //  uiContext.Send(x => callback(), null);
    //}

    //public void Invoke(Action callback)
    //{
    //  JoinableTaskFactory.Run(async () =>
    //  {
    //    var factory =new JoinableTaskFactory();
    //    await JoinableTaskFactory.SwitchToMainThreadAsync();
    //    callback();
    //  });
    //}
  }
}