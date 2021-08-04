namespace DeepNestLib
{
  using System;

  public interface IDispatcherService
  {
    bool InvokeRequired { get; }

    void Invoke(Action callback);
  }
}