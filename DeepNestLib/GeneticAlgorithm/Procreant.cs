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
    private int terminations = 0;

    private double[] strictVerticalAngles = new double[]
    {
      0,
      180,
    };

    private double[] strictHorizontalAngles = new double[]
    {
      90,
      270,
    };

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
        if (adam[i].StrictAngle == AnglesEnum.Vertical || (adam[i].StrictAngle == AnglesEnum.None && this.config.StrictAngles == AnglesEnum.Vertical))
        {
          angles.Add(this.strictVerticalAngles[this.random.Next() % this.strictVerticalAngles.Length]);
        }
        else if (adam[i].StrictAngle == AnglesEnum.Horizontal || (adam[i].StrictAngle == AnglesEnum.None && this.config.StrictAngles == AnglesEnum.Horizontal))
        {
          angles.Add(this.strictHorizontalAngles[this.random.Next() % this.strictHorizontalAngles.Length]);
        }
        else
        {
          var angle = Math.Floor(this.r.NextDouble() * this.config.Rotations) * (360f / this.config.Rotations);
          angles.Add(angle);
        }
      }

      var population = new PopulationItem[config.PopulationSize];
      population[0] = new PopulationItem(adam.ToList(), angles.ToArray());
      for (int i = 1; i < config.PopulationSize; i++)
      {
        var mutant = this.Mutate(population[0]);
        population[i] = mutant;
      }

      Population = TerminateClones(population).ToArray();
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
      var chromosome = $"{string.Join(",", citizen.Parts.Select(p => p.Id))},{string.Join(",", citizen.Rotation.Select(r => r))}";
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

    public PopulationItem[] Population { get; private set; }

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
      var clone = new PopulationItem(p.Parts.ToList(), p.Rotation.Clone() as double[]);
      for (var i = 0; i < clone.Parts.Count(); i++)
      {
        var rand = r.NextDouble();
        if (rand < 0.01 * config.MutationRate)
        {
          var j = i + 1;
          if (j < clone.Parts.Count)
          {
            var temp = clone.Parts[i];
            clone.Parts[i] = clone.Parts[j];
            clone.Parts[j] = temp;
          }
        }

        rand = r.NextDouble();
        if (rand < 0.01 * config.MutationRate)
        {
          if (clone.Parts[i].StrictAngle == AnglesEnum.Vertical || (clone.Parts[i].StrictAngle == AnglesEnum.None && config.StrictAngles == AnglesEnum.Vertical))
          {
            clone.Rotation[i] = strictVerticalAngles[random.Next() % strictVerticalAngles.Length];
          }
          else if (clone.Parts[i].StrictAngle == AnglesEnum.Horizontal || (clone.Parts[i].StrictAngle == AnglesEnum.None && config.StrictAngles == AnglesEnum.Horizontal))
          {
            clone.Rotation[i] = strictHorizontalAngles[random.Next() % strictHorizontalAngles.Length];
          }
          else
          {
            clone.Rotation[i] = Math.Floor(r.NextDouble() * config.Rotations) * (360f / config.Rotations);
          }
        }
      }

      return clone;
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
      var cutpoint = (int)Math.Round(Math.Min(Math.Max(r.NextDouble(), 0.1), 0.9) * (male.Parts.Count - 1));

      var gene1 = new List<INfp>(male.Parts.Take(cutpoint).ToArray());
      var rot1 = new List<double>(male.Rotation.Take(cutpoint).ToArray());

      var gene2 = new List<INfp>(female.Parts.Take(cutpoint).ToArray());
      var rot2 = new List<double>(female.Rotation.Take(cutpoint).ToArray());

      var i = 0;

      for (i = 0; i < female.Parts.Count; i++)
      {
        if (!gene1.Any(z => z.Id == female.Parts[i].Id))
        {
          gene1.Add(female.Parts[i]);
          rot1.Add(female.Rotation[i]);
        }
      }

      for (i = 0; i < male.Parts.Count; i++)
      {
        if (!gene2.Any(z => z.Id == male.Parts[i].Id))
        {
          gene2.Add(male.Parts[i]);
          rot2.Add(male.Rotation[i]);
        }
      }

      return new[]
      {
        new PopulationItem(gene1, rot1.ToArray()),
        new PopulationItem(gene2, rot2.ToArray()),
      };
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
      this.progressDisplayer.SetIsVisibleSecondaryProgressBar(false);
      this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Primary, "Procreate. . .", config.PopulationSize);
      while (newPopulation.Count() < config.PopulationSize && chaperone.ElapsedMilliseconds <= config.ProcreationTimeout)
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
