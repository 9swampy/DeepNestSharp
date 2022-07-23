namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  internal static class DxfFileExtensions
  {
    public static void AddRange(this IList<DxfEntity> list, IEnumerable<DxfLine> lines)
    {
      foreach (var line in lines)
      {
        list.Add(line);
      }
    }
  }
}