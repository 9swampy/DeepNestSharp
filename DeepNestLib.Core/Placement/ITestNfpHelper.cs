namespace DeepNestLib
{
  public interface ITestNfpHelper
  {
    IMinkowskiSumService MinkowskiSumService { get; set; }

    INfp[] ExecuteInterchangeableMinkowski(bool useDllImport, INfp path, INfp pattern);
  }
}