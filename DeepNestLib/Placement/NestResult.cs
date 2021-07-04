namespace DeepNestLib.Placement
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public class NestResult
  {
    private double fitnessAlt = -1;

    public double? fitness { get; }

    public NestResult(int nestIndex, double area, SheetPlacementCollection allplacements, double? fitness, double mergedLength)
    {
      UsedSheets = allplacements;
      this.fitness = fitness;
      this.NestIndex = nestIndex;
      this.area = area;
      this.mergedLength = mergedLength;
    }

    public double FitnessAlt
    {
      get
      {
        if (this.fitness == null || this.fitnessAlt == -1)
        {
          this.fitnessAlt = new OriginalFitness().Evaluate(this);
        }

        return this.fitnessAlt;
      }
    }

    public float[] Rotation { get; set; }

    public SheetPlacementCollection UsedSheets { get; private set; }

    public int NestIndex { get; }

    /// <summary>
    /// Area of the placement sheet.
    /// </summary>
    public double area { get; }

    public double mergedLength { get; }

    internal int index { get; set; }
  }
}
