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
    public Chromosome(double rotation)
    {
      Rotation = rotation;
    }

    public Chromosome(INfp part, double rotation)
    {
      this.Part = part.Clone();
      Rotation = rotation;
    }

    [JsonInclude]
    public INfp Part { get; private set; }

    [JsonInclude]
    public double Rotation { get; internal set; }

    internal void SetIndex(int idx)
    {
      Part.Id = idx;
    }
  }
}