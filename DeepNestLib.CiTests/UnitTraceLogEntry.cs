namespace DeepNestLib.CiTests
{
  using System.Diagnostics;

  public class UnitTraceLogEntry
  {
    public UnitTraceLogEntry(StackFrame stackFrame, string message)
    {
      StackFrame = stackFrame;
      Message = message;
    }

    public StackFrame StackFrame { get; }

    public string Message { get; }
  }
}
