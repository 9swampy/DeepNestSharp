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
    private readonly HashSet<string> ancestors = new HashSet<string>();
    private readonly Random r = new Random();
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

    private readonly Random random = new Random();

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

    private PopulationItem Mutate(PopulationItem p)
    {
      var clone = p.Gene.ToArray();
      for (var i = 0; i < clone.Length; i++)
      {
        var rand = r.NextDouble();
        if (rand < 0.01 * config.MutationRate)
        {
          var j = i + 1;
          if (j < clone.Length)
          {
            var temp = clone[i];
            clone[i] = clone[j];
            clone[j] = temp;
          }
        }

        rand = r.NextDouble();
        if (rand < 0.01 * config.MutationRate)
        {
          clone[i].Rotation = GetRandomRotation(clone[i].Part);
        }
      }

      return new PopulationItem(new DeepNestGene(clone));
    }

    private double GetRandomRotation(INfp part)
    {
      if (IsPartRotationRestricted(part, AnglesEnum.AsPreviewed))
      {
        return strictAsPreviewedAngles[random.Next() % strictAsPreviewedAngles.Length];
      }
      else if (IsPartRotationRestricted(part, AnglesEnum.Rotate90))
      {
        return strictRotate90Angles[random.Next() % strictRotate90Angles.Length];
      }
      else
      {
        return Math.Floor(r.NextDouble() * config.Rotations) * (360f / config.Rotations);
      }
    }

    private bool IsPartRotationRestricted(INfp part, AnglesEnum restriction)
    {
      return part.StrictAngle == restriction || (part.StrictAngle == AnglesEnum.None && this.config.StrictAngles == restriction);
    }

    // returns a random individual from the population, weighted to the front of the list (lower fitness value is more likely to be selected)
    private PopulationItem RandomWeightedIndividual(IEnumerable<PopulationItem> population, PopulationItem exclude = null)
    {
      var pop = population.ToList();

      if (exclude != null)
      {
        pop.Remove(exclude);
      }

      var rand = r.NextDouble();

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

    // single point crossover
    private PopulationItem[] Mate(PopulationItem male, PopulationItem female)
    {
      var cutpoint = (int)Math.Round(Math.Min(Math.Max(r.NextDouble(), 0.1), 0.9) * (male.Gene.Length - 1));

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
    private static Chromosome[] CompleteGene(IEnumerable<Chromosome> initiantPartialGene, DeepNestGene supplicantParentGene)
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

    public void Generate()
    {
      // Individuals with higher fitness are more likely to be selected for mating
      Array.Sort(Population, (x, y) => x.Fitness.CompareTo(y.Fitness)); // = Population.OrderBy(z => z.Fitness).ToList();

      // fittest individuals are preserved in the new generation (elitism)
      var newPopulation = new List<PopulationItem>();
      var fittestSurvivors = config.PopulationSize / 10;
      newPopulation.AddRange(this.Population.Take(this.Population.Count() < fittestSurvivors ? this.Population.Count() : fittestSurvivors));
      chaperone.Restart();
      //this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      //this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Primary, "Procreate. . .", config.PopulationSize);
      while (newPopulation.Count() > 1 && newPopulation.Count() < config.PopulationSize && chaperone.ElapsedMilliseconds <= config.ProcreationTimeout)
      {
        var male = RandomWeightedIndividual(newPopulation);
        var female = RandomWeightedIndividual(newPopulation, male);

        // each mating produces two children
        var children = Mate(male, female);

        // slightly mutate children
        var child = this.Mutate(children[0]);
        if (IsUnique(child))
        {
          chaperone.Restart();
          newPopulation.Add(child);
          if (newPopulation.Count < config.PopulationSize)
          {
            child = this.Mutate(children[1]);
            if (IsUnique(child))
            {
              newPopulation.Add(child);
            }
          }
        }
        else
        {
          this.progressDisplayer.DisplayTransientMessage($"Cumulative Terminations={terminations} ({chaperone.ElapsedMilliseconds / 1000:N0}s)");
        }
      }

      if (newPopulation.Count <= fittestSurvivors)
      {
        newPopulation = TerminateClones(newPopulation).ToList();
      }

      this.Population = newPopulation.ToArray();
    }
  }
}
