namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using System.Text;

  public class UnitTraceLog : List<UnitTraceLogEntry>
  {
    public override string ToString()
    {
      StringBuilder resultBuilder = new StringBuilder();
      foreach (UnitTraceLogEntry entry in this)
      {
        resultBuilder.AppendLine($"{entry.StackFrame.GetMethod().DeclaringType.Name}.{entry.StackFrame.GetMethod().Name}:{entry.StackFrame.GetFileLineNumber()}=>{entry.Message}");
      }

      return resultBuilder.ToString();
    }
  }
}
