namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;
  using System.Linq;

  public class DeepNestGene : IEnumerable<Chromosome>
  {
    private readonly IList<Chromosome> chromosomes;

    public DeepNestGene(IList<Chromosome> chromosomes)
    {
      this.chromosomes = chromosomes;
    }

    public int Count => chromosomes.Count;

    public int Length => chromosomes.Count;

    public Chromosome this[int index] { get => chromosomes[index]; }

    public int IndexOf(Chromosome item) => chromosomes.IndexOf(item);

    public IEnumerator<Chromosome> GetEnumerator() => chromosomes.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => chromosomes.GetEnumerator();

    internal int IndexOf(int partId) => chromosomes.IndexOf(chromosomes.Single(o => o.Part.Id == partId));
  }
}