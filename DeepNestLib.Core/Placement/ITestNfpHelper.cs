namespace DeepNestLib
{
  public interface ITestNfpHelper
  {
    IMinkowskiSumService MinkowskiSumService { get; set; }

    bool UseCacheProcess { get; set; }

    INfp[] ExecuteInterchangeableMinkowski(bool useDllImport, INfp path, INfp pattern);
  }
}