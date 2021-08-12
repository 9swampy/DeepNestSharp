namespace DeepNestLib
{
  using System;
  using System.Threading.Tasks;

  public interface IDispatcherService
  {
    bool InvokeRequired { get; }

    void Invoke(Action callback);

    Task InvokeAsync(Action callback);
  }
}