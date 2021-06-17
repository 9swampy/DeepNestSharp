namespace DeepNestConsole
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using DeepNestLib;

    public class SampleProgram
    {
        public NestingContext Context = new NestingContext();

        public bool IsFinished()
        {
            // insert you code here
            return this.Context.Iterations >= 3;
        }

        public void Run()
        {
            Background.UseParallel = true;
            SvgNest.Config.PlacementType = PlacementTypeEnum.Gravity;
            Console.WriteLine("Settings updated..");

            Console.WriteLine("Start nesting..");
            Console.WriteLine("Parts: " + this.Context.Polygons.Count());
            Console.WriteLine("Sheets: " + this.Context.Sheets.Count());

            this.Context.StartNest();
            do
            {
                var sw = Stopwatch.StartNew();
                this.Context.NestIterate();
                sw.Stop();
                Console.WriteLine("Iteration: " + this.Context.Iterations + "; fitness: " + this.Context.Current.fitness + "; nesting time: " + sw.ElapsedMilliseconds + "ms");
            }
            while (!this.IsFinished());

            this.Context.ExportSvg("temp.svg");
            Console.WriteLine("Results exported in: temp.svg");
        }
    }
}
