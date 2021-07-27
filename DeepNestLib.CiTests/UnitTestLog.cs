namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

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
