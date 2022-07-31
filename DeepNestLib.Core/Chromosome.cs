namespace DeepNestLib
{
  using System.Text.Json.Serialization;

  public class Chromosome
  {
    private readonly INfp part;

    public Chromosome(INfp part)
      : this(part, part.Rotation)
    {
    }

    [JsonConstructor]
    public Chromosome(INfp part, double rotation)
    {
      this.part = part;
      Rotation = rotation;
    }

    [JsonInclude]
    public INfp Part => part;

    [JsonInclude]
    public double Rotation { get; internal set; }

    internal void SetIndex(int idx)
    {
      Part.Id = idx;
    }
  }
}