namespace Minkowski
{
  using System.Runtime.InteropServices;

  public class MinkowskiWrapper
  {
#if x64
    [DllImport("minkowski_x64.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setData(long cntA, double[] pntsA, long holesCnt, long[] holesSizes, double[] holesPoints, long cntB, double[] pntsB);
#elif x86
        [DllImport("minkowski_x86.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setData(int cntA, double[] pntsA, int holesCnt, int[] holesSizes, double[] holesPoints, int cntB, double[] pntsB);
#else
    [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)] // Actually this is just a copy of the x86 dll
    public static extern void setData(int cntA, double[] pntsA, int holesCnt, int[] holesSizes, double[] holesPoints, int cntB, double[] pntsB);
#endif

#if x64
    [DllImport("minkowski_x64.dll", CallingConvention = CallingConvention.Cdecl)]
#elif x86
        [DllImport("minkowski_x86.dll", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
#endif
    public static extern void calculateNFP();

#if x64
    [DllImport("minkowski_x64.dll", CallingConvention = CallingConvention.Cdecl)]
#elif x86
        [DllImport("minkowski_x86.dll", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
#endif
    public static extern void getSizes1(int[] sizes);

#if x64
    [DllImport("minkowski_x64.dll", CallingConvention = CallingConvention.Cdecl)]
#elif x86
        [DllImport("minkowski_x86.dll", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
#endif
    public static extern void getSizes2(int[] sizes1, int[] sizes2);

#if x64
    [DllImport("minkowski_x64.dll", CallingConvention = CallingConvention.Cdecl)]
#elif x86
        [DllImport("minkowski_x86.dll", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
#endif
    public static extern void getResults(double[] data, double[] holesData);
  }
}
