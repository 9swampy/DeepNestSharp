namespace DeepNestLib
{
  using System.Text.Json.Serialization;

  public class Chromosome
  {
    public Chromosome(INfp part)
      : this(part, part.Rotation)
    {
    }

    [JsonConstructor]
    public Chromosome(INfp part, double rotation)
    {
      Part = part;
      Rotation = rotation;
    }

    [JsonInclude]
    public INfp Part { get; private set; }

    [JsonInclude]
    public double Rotation { get; internal set; }
  }
}