namespace DeepNestLib
{
  public interface ITestNfpHelper
  {
    IMinkowskiSumService MinkowskiSumService { get; set; }

    bool UseDllImport { get; set; }

    INfp[] ExecuteInterchangeableMinkowski(bool useDllImport, INfp path, INfp pattern);
  }
}