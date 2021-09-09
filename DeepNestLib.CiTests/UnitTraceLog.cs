namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using System.Text;

  public class UnitTraceLog : List<UnitTraceLogEntry>
  {
    public override string ToString()
    {
      var resultBuilder = new StringBuilder();
      foreach (var entry in this)
      {
        resultBuilder.AppendLine($"{entry.StackFrame.GetMethod().DeclaringType.Name}.{entry.StackFrame.GetMethod().Name}:{entry.StackFrame.GetFileLineNumber()}=>{entry.Message}");
      }

      return resultBuilder.ToString();
    }
  }
}
