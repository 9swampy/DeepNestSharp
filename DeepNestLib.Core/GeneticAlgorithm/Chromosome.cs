namespace DeepNestLib.GeneticAlgorithm
{
  using System.Text.Json.Serialization;

  public class Chromosome : IDeepNestChromosome
  {
    private double rotation;

    public Chromosome(INfp part)
      : this(part, part.Rotation)
    {
    }

    [JsonConstructor]
    public Chromosome(double rotation)
    {
      this.rotation = rotation;
    }

    public Chromosome(INfp part, double rotation)
      :this(rotation)
    {
      Part = part.Clone();
    }

    [JsonInclude]
    public INfp Part { get; private set; }

    [JsonInclude]
    public double Rotation => this.rotation;

    public void SetRotation(double rotation)
    {
      this.rotation = rotation;
    }

    public override string ToString()
    {
      return $"Id:{Part.Id}:{Part.Name}:{Part.Rotation} I:{Rotation}";
    }

    internal void SetIndex(int idx)
    {
      Part.Id = idx;
    }
  }
}