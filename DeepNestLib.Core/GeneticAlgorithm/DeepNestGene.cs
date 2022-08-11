namespace DeepNestLib.GeneticAlgorithm
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Text.Json;
  using DeepNestLib.Placement;

  public class DeepNestGene : IEnumerable<Chromosome>
  {
    private IList<Chromosome> chromosomes;

    public DeepNestGene(IList<Chromosome> chromosomes)
    {
      this.chromosomes = chromosomes;
    }

    public int Length => this.chromosomes.Count;

    public Chromosome this[int index] { get => this.chromosomes[index]; }

    public int IndexOf(Chromosome item) => this.chromosomes.IndexOf(item);

    public IEnumerator<Chromosome> GetEnumerator() => this.chromosomes.ToList().GetEnumerator();

    public override string ToString()
    {
      var list = this.chromosomes.Select(o =>
      {
        StringBuilder builder = new StringBuilder($"{o.Part.Id}:");
        if (o.Part.Rotation != 0)
        {
          builder.Append($"{o.Rotation}:");
        }

        builder.Append($"{o.Part.Rotation}:");
        return builder.ToString();
      });

      return string.Join(",", list.Select(x => x.ToString()).ToArray());
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.chromosomes.GetEnumerator();

    internal static DeepNestGene FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new InterfaceConverterFactory(typeof(NoFitPolygon), typeof(INfp)));
      options.Converters.Add(new IListInterfaceConverterFactory(typeof(INfp)));
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new DeepNestGeneJsonConverter());
      return JsonSerializer.Deserialize<DeepNestGene>(json, options);
    }

    internal int IndexOf(int partId) => this.chromosomes.IndexOf(this.chromosomes.Single(o => o.Part.Id == partId));

    internal string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}