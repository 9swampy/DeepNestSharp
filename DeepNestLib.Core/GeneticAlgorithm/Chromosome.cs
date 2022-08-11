namespace DeepNestLib.GeneticAlgorithm
{
  using System.Text.Json.Serialization;

  public class Chromosome
  {
    public Chromosome(INfp part)
      : this(part, part.Rotation)
    {
    }

    [JsonConstructor]
    public Chromosome(double rotation)
    {
      Rotation = rotation;
    }

    public Chromosome(INfp part, double rotation)
    {
      Part = part.Clone();
      Rotation = rotation;
    }

    [JsonInclude]
    public INfp Part { get; private set; }

    [JsonInclude]
    public double Rotation { get; internal set; }

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