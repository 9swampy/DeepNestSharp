namespace DeepNestLib.GeneticAlgorithm
{
  using GeneticSharp.Domain.Chromosomes;
  using GeneticSharp.Domain.Fitnesses;
  using GeneticSharp.Domain.Randomizations;

  public class NestChromosome : ChromosomeBase
  {
    public NestChromosome(int numberOfParts)
      : base(numberOfParts)
    => CreateGenes();

    public override IChromosome CreateNew()
    {
      return new NestChromosome(this.Length);
    }

    public override Gene GenerateGene(int geneIndex)
    {
      return new Gene(RandomizationProvider.Current.GetInt(0, this.Length * 50));
    }
  }

  public class NestFitness : IFitness
  {
    public double Evaluate(IChromosome chromosome)
    {
      throw new System.NotImplementedException();
    }
  }
}
