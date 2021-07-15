namespace DeepNestSharp
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib;
  using DeepNestLib.Ui;

  public class DeepNestState : IDeepNestState
  {
    public DeepNestState(List<DetailLoadInfo> partInfos, List<ISheetLoadInfo> sheetInfos, NestingContext context)
    {
      PartInfos = partInfos;
      SheetInfos = sheetInfos;
      Context = context;
    }

    public NestingContext Context { get; }

    public TopNestResultsCollection NestResults => Context.Nest.TopNestResults;

    public List<DetailLoadInfo> PartInfos { get; }
    
    public ICollection<INfp> Polygons => Context.Polygons;

    public List<ISheetLoadInfo> SheetInfos { get; }

    public List<INfp> Sheets => Context.Sheets;

    public string ToJson()
    {
      throw new NotImplementedException();
    }
  }
}