namespace DeepNestLib
{
  public interface ITopNestResultsConfig
  {
    /// <summary>
    /// Gets or sets the maximum total population per Genetic algorithm generation.
    /// </summary>
    int PopulationSize { get; set; }

    /// <summary>
    /// Gets or sets the percentage difference between an existing TopNest and a new candidate needed for insertion in to Top collection.
    /// Diversity of the Tops will help keep the Genetic Algorithm innovating at the expense of potentially excluding a novel Top performer.
    /// 1=100% which would kill the nest; anecdotally we've found the best is around 0.0001 but YMMV.
    /// </summary>
    double TopDiversity { get; set; }
  }
}