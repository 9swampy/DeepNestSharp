namespace DeepNestConsole
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib;

  public class SampleProgram
  {
    internal NestingContext Context { get; } = new NestingContext(new ConsoleMessageService(), null, null, SvgNest.Config);

    private bool IsFinished()
    {
      // Place code here to define when your Nest can be considered complete.
      return this.Context?.State.TopNestResults.Count() >= 3;

      // The example above considers the nest completed when the
      // first response has been iteratively improved upon twice.
    }

    public void Run()
    {
      SvgNest.Config.UseParallel = true;
      SvgNest.Config.PlacementType = PlacementTypeEnum.Gravity;
      Console.WriteLine("Settings updated..");

      Console.WriteLine("Start nesting..");
      Console.WriteLine("Parts: " + Context.Polygons.Count());
      Console.WriteLine("Sheets: " + Context.Sheets.Count());

      Context.StartNest();
      do
      {
        var sw = Stopwatch.StartNew();
        Context.NestIterate(SvgNest.Config);
        sw.Stop();
        Console.WriteLine("Iteration: " + Context.State.Iterations + "; fitness: " + Context.Current.Fitness + "; nesting time: " + sw.ElapsedMilliseconds + "ms");
      }
      while (!IsFinished());

      ConvertResults();
    }

    private void ConvertResults()
    {
      string path = "output.dxf";

      if (path.ToLower().EndsWith("svg"))
        new SvgParser(SvgNest.Config).Export(path, Context.Polygons, Context.Sheets);
      else if (path.ToLower().EndsWith("dxf"))
        new DxfParser().Export(path, Context.Polygons, Context.Sheets);
      else
        throw new NotImplementedException($"unknown format: {path}");

      Console.WriteLine($"Results exported in: {path}");
    }
  }
}
