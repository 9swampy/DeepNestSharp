namespace DeepNestLib
{
#if NCRUNCH
  using System.Diagnostics;
#endif
  internal enum Found
  {
    Outside = 0x0,
    Inside = 0x1,
    OnPolygon = -0x1,
  }
}