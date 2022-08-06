namespace DeepNestLib.IO
{
  using System.Collections.Generic;
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