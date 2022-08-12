namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.Geometry;
  using DeepNestLib.NestProject;

  public class Procreant
  {
    private static readonly Random Random = new Random();

    private readonly HashSet<string> ancestors = new HashSet<string>();
    private readonly ISvgNestConfig config;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly Stopwatch chaperone = new Stopwatch();

    private readonly double[] strictAsPreviewedAngles = new double[]
    {
      0,
      180,
    };

    private readonly double[] strictRotate90Angles = new double[]
    {
      90,
      270,
    };

    private PopulationItem[] population;
    private int terminations = 0;

    public Procreant(NestItem<INfp>[] parts, ISvgNestConfig config, IProgressDisplayer progressDisplayer)
      : this(CreateAdam(parts), config, progressDisplayer)
    {
    }

    public Procreant(INfp[] adam, ISvgNestConfig config, IProgressDisplayer progressDisplayer)
    {
      this.config = config;
      this.progressDisplayer = progressDisplayer;
      var angles = new List<double>();
      for (var i = 0; i < adam.Length; i++)
      {
        angles.Add(GetRandomRotation(adam[i]));
      }

      var population = new PopulationItem[config.PopulationSize];
      population[0] = new PopulationItem(BuildAdamGene(adam.ToList(), angles.ToArray()));
      for (int i = 1; i < config.PopulationSize; i++)
      {
        var mutant = this.Mutate(population[0]);
        population[i] = mutant;
      }

      Population = TerminateClones(population).ToArray();
    }

    private static DeepNestGene BuildAdamGene(List<INfp> parts, double[] rotations)
    {
      var resultSource = new List<Chromosome>();
      for (int i = 0; i < parts.Count; i++)
      {
        var chromosome = new Chromosome(parts[i], rotations[i]);
        chromosome.SetIndex(i);
        resultSource.Add(chromosome);
      }

      return new DeepNestGene(resultSource);
    }

    private IEnumerable<PopulationItem> TerminateClones(IEnumerable<PopulationItem> source)
    {
      var population = new List<PopulationItem>();
      foreach (var citizen in source)
      {
        if (IsUnique(citizen))
        {
          population.Add(citizen);
        }
      }

      return population;
    }

    private bool IsUnique(PopulationItem citizen)
    {
      var chromosome = $"{string.Join(",", citizen.Gene.Select(p => p.Part.Id))},{string.Join(",", citizen.Gene.Select(r => r.Rotation))}";
      if (ancestors.Add(chromosome))
      {
        this.progressDisplayer.IncrementLoopProgress(ProgressBar.Primary);
        return true;
      }
      else
      {
        terminations++;
      }

      return false;
    }

    public PopulationItem[] Population
    {
      get => population;

      private set
      {
        population = value;
        for (int idx = 0; idx < population.Length; idx++)
        {
          population[idx].Index = idx;
        }
      }
    }

    private static INfp[] CreateAdam(NestItem<INfp>[] parts)
    {
      List<INfp> adam = new List<INfp>();
      var id = 0;
      for (int i = 0; i < parts.Count(); i++)
      {
        var part = parts[i];
        for (int j = 0; j < part.Quantity; j++)
        {
          var poly = part.Polygon.CloneTree(); // deep copy
          poly.Id = id; // id is the unique id of all parts that will be nested, including cloned duplicates
          poly.Source = i; // source is the id of each unique part from the main part list

          adam.Add(poly);
          id++;
        }
      }

      adam = adam.OrderByDescending(z => Math.Abs(GeometryUtil.PolygonArea(z))).ToList();
      /*List<NFP> shuffle = new List<NFP>();
      Random r = new Random(DateTime.Now.Millisecond);
      while (adam.Any())
      {
          var rr = r.Next(adam.Count);
          shuffle.Add(adam[rr]);
          adam.RemoveAt(rr);
      }
      adam = shuffle;*/

      /*#region special case
      var temp = adam[1];
      adam.RemoveAt(1);
      adam.Insert(9, temp);

      #endregion*/
      return adam.ToArray();
    }

    public bool IsCurrentGenerationFinished
    {
      get
      {
        for (int i = 0; i < this.Population.Length; i++)
        {
          if (this.Population[i].Fitness == -1)
          {
            return false;
          }
        }

        return true;
      }
    }

    /// <summary>
    /// Slightly mutate children.
    /// </summary>
    private PopulationItem Mutate(PopulationItem p)
    {
      var mutationRate = 0.01 * config.MutationRate;
      var clonedChromosomes = p.Gene.ToList();
      for (var i = 0; i < clonedChromosomes.Count; i++)
      {
        var rand = Random.NextDouble();
        if (rand < mutationRate)
        {
          var j = i + 1;
          if (j < clonedChromosomes.Count)
          {
            var temp = clonedChromosomes[i];
            clonedChromosomes[i] = clonedChromosomes[j];
            clonedChromosomes[j] = temp;
          }
        }

        rand = Random.NextDouble();
        if (rand < mutationRate)
        {
          clonedChromosomes[i] = new Chromosome(clonedChromosomes[i].Part, GetRandomRotation(clonedChromosomes[i].Part));
        }
      }

      return new PopulationItem(new DeepNestGene(clonedChromosomes));
    }

    private double GetRandomRotation(INfp part)
    {
      if (IsPartRotationRestricted(part, AnglesEnum.AsPreviewed))
      {
        return strictAsPreviewedAngles[Random.Next() % strictAsPreviewedAngles.Length];
      }
      else if (IsPartRotationRestricted(part, AnglesEnum.Rotate90))
      {
        return strictRotate90Angles[Random.Next() % strictRotate90Angles.Length];
      }
      else
      {
        return Math.Floor(Random.NextDouble() * config.Rotations) * (360f / config.Rotations);
      }
    }

    private bool IsPartRotationRestricted(INfp part, AnglesEnum restriction)
    {
      return part.StrictAngle == restriction || (part.StrictAngle == AnglesEnum.None && this.config.StrictAngles == restriction);
    }

    /// <summary>
    /// Returns a random individual from the population, weighted to the front of the list (lower fitness value is more likely to be selected).
    /// </summary>
    private PopulationItem RandomWeightedIndividual(IEnumerable<PopulationItem> population, PopulationItem exclude = null)
    {
      var pop = population.ToList();

      if (exclude != null)
      {
        pop.Remove(exclude);
      }

      var rand = Random.NextDouble();

      double lower = 0;
      var weight = 1 / (double)pop.Count;
      var upper = weight;

      for (var i = 0; i < pop.Count; i++)
      {
        // if the random number falls between lower and upper bounds, select this individual
        if (rand > lower && rand < upper)
        {
          return pop[i];
        }

        lower = upper;
        upper += 2 * weight * ((pop.Count - i) / (double)pop.Count);
      }

      return pop[0];
    }

    /// <summary>
    /// Single point crossover, each mating produces two children.
    /// </summary>
    private static PopulationItem[] Mate(PopulationItem male, PopulationItem female)
    {
      var cutpoint = (int)Math.Round(Math.Min(Math.Max(Random.NextDouble(), 0.1), 0.9) * (male.Gene.Length - 1));

      var son = CompleteGene(male.Gene.Take(cutpoint), female.Gene);
      var daughter = CompleteGene(female.Gene.Take(cutpoint), male.Gene);

      return new[]
      {
        new PopulationItem(new DeepNestGene(son)),
        new PopulationItem(new DeepNestGene(daughter)),
      };
    }

    /// <summary>
    /// Given partial gene add any missing chromosomes from the population gene.
    /// </summary>
    /// <param name="initiantPartialGene">Partial gene from initiant parent.</param>
    /// <param name="supplicantParentGene">Full gene from supplicant parent.</param>
    /// <returns>Completed child gene.</returns>
    private static IDeepNestChromosome[] CompleteGene(IEnumerable<IDeepNestChromosome> initiantPartialGene, DeepNestGene supplicantParentGene)
    {
      var result = initiantPartialGene.ToArray();
      var idx = result.Length;
      Array.Resize(ref result, supplicantParentGene.Length);
      var i = 0;
      for (i = 0; i < supplicantParentGene.Length; i++)
      {
        if (!initiantPartialGene.Any(z => z.Part.Id == supplicantParentGene[i].Part.Id))
        {
          result[idx] = supplicantParentGene[i];
          idx++;
        }
      }

      return result;
    }

    internal void Generate(TopNestResultsCollection topNestResults)
    {
      Generate(topNestResults.Select(o => (o.Gene, o.FitnessTotal)).ToList(), topNestResults.EliteSurvivors);
    }

    internal void Generate(IList<(DeepNestGene Gene, double FitnessTotal)> progenitors, int eliteSurvivors)
    {
      this.progressDisplayer.DisplayTransientMessage("Procreating. . .");
      // Individuals with higher fitness are more likely to be selected for mating
      var ancestors = progenitors.Take(eliteSurvivors).Select(o => new PopulationItem(o.Gene) { Fitness = o.FitnessTotal })
                                         .Union(Population)
                                         .OrderBy(o => o.Fitness)
                                         .ToArray();

      // fittest individuals are preserved in the new generation (elitism)
      var newPopulation = new List<PopulationItem>();
      var survivors = (int)Math.Min(eliteSurvivors * 2, ancestors.Count());
      ancestors = ancestors.Take(survivors).ToArray();
      chaperone.Restart();
      //this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      //this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Primary, "Procreate. . .", config.PopulationSize);

      bool first = true;
      while (first ||
             (newPopulation.Count() < config.PopulationSize &&
              chaperone.ElapsedMilliseconds <= config.ProcreationTimeout))
      {
        first = false;
        var parents = ancestors.Union(newPopulation);
        var male = RandomWeightedIndividual(parents);
        var female = RandomWeightedIndividual(parents, male);

        var foetuses = Mate(male, female);
        foreach (var foetus in foetuses)
        {
          if (newPopulation.Count < config.PopulationSize)
          {
            var child = this.Mutate(foetus);
            if (IsUnique(child))
            {
              chaperone.Restart();
              newPopulation.Add(child);
              this.progressDisplayer.IncrementLoopProgress(ProgressBar.Primary);
            }
            else if (SvgNest.IsVerboseLogging)
            {
              this.progressDisplayer.DisplayTransientMessage($"Cumulative Terminations={terminations} ({chaperone.ElapsedMilliseconds / 1000:N0}s)");
            }
          }
        }

        if (newPopulation.Count(o => o.IsPending == false) > 0)
        {
          System.Diagnostics.Debugger.Break();
        }
      }

      this.Population = newPopulation.ToArray();
    }
  }
}
