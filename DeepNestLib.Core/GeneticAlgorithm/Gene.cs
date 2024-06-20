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

    public int Count => this.chromosomes.Count;

    public int Length => this.chromosomes.Count;

    public Chromosome this[int index] { get => this.chromosomes[index]; }

    public int IndexOf(Chromosome item) => this.chromosomes.IndexOf(item);

    public IEnumerator<Chromosome> GetEnumerator() => this.chromosomes.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.chromosomes.GetEnumerator();

    internal int IndexOf(int partId) => this.chromosomes.IndexOf(this.chromosomes.Single(o => o.Part.Id == partId));
  }
}