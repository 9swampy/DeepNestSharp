namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;

  /// <summary>
  ///     Directs tracing or debugging output to the specified <see cref="Action{T}"/>
  ///     handlers where assertions may be made about the messages being traced.
  /// </summary>
  public sealed class UnitTestingTraceListener : TraceListener
  {
    /// <summary>
    ///     Simple <see cref="Action{T}"/> that does nothing;
    ///     used to prevent <see cref="NullReferenceException"/>s.
    /// </summary>
    static readonly Action<StackFrame, string> doNothing = (s, x) => { /* Do nothing */ };

    /// <summary>
    ///     Gets or sets the <see cref="Action{T}"/> to invoke
    ///     whenever <see cref="Trace.WriteLine(object)"/> is invoked.
    /// </summary>
    public Action<StackFrame, string> OnWrite { get; set; } = doNothing;

    /// <summary>
    ///     Gets or sets the <see cref="Action{T}"/> to invoke
    ///     whenever <see cref="Trace.Write(object)"/> is invoked.
    /// </summary>
    public Action<StackFrame, string> OnWriteLine { get; set; } = doNothing;

    /// <summary>Gets or sets a name for this <see cref="TraceListener" />.</summary>
    /// <returns>A name for this <see cref="TraceListener" />.</returns>
    /// <filterpriority>2</filterpriority>
    public override string Name { get; set; } = "Unit Testing Trace Listener";

    /// <summary>
    ///     When overridden in a derived class, writes the specified
    ///     message to the listener you create in the derived class.
    /// </summary>
    /// <param name="message">A message to write.</param>
    /// <filterpriority>2</filterpriority>
    public override void Write(string message) => OnWrite(GetStackFrame(), message);

    /// <summary>
    ///     When overridden in a derived class, writes a message to the listener
    ///     you create in the derived class, followed by a line terminator.
    /// </summary>
    /// <param name="message">A message to write.</param>
    /// <filterpriority>2</filterpriority>
    public override void WriteLine(string message)
    {
      OnWriteLine(GetStackFrame(), message);
    }

    private static StackFrame GetStackFrame()
    {
      return new StackFrame(5, true);
    }
  }
}
