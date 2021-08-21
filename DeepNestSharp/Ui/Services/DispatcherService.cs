namespace DeepNestSharp.Ui.Services
{
  using System;
  using System.Threading.Tasks;
  using System.Windows.Threading;
  using DeepNestLib;
  using Microsoft.VisualStudio.Threading;

  public class DispatcherService : IDispatcherService
  {
    private static JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
    private Dispatcher dispatcher = System.Windows.Application.Current.Dispatcher;

    public DispatcherService(Dispatcher dispatcher)
    {
      this.dispatcher = dispatcher;
    }

    public bool InvokeRequired => !dispatcher.CheckAccess();

    public void Invoke(Action callback) => joinableTaskContext.Factory.Run(async () => await InvokeAsync(callback).ConfigureAwait(false));

    public async Task InvokeAsync(Action callback)
    {
      await joinableTaskContext.Factory.SwitchToMainThreadAsync();
      callback();
    }

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