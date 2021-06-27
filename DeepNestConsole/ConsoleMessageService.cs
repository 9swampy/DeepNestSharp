using DeepNestLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DeepNestConsole
{
  public class ConsoleMessageService : IMessageService
  {
    public void DisplayMessage(string message)
    {
      Console.WriteLine(message);
    }
  }
}
