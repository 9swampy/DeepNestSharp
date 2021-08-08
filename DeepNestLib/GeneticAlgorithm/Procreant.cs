namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.NestProject;

  public class Procreant
  {
    private readonly Random r = new Random();

    private readonly ISvgNestConfig Config;
    public PopulationItem[] Population;

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

    public Procreant(INfp[] adam, ISvgNestConfig config)
    {
      Config = config;

      var angles = new List<double>();
      for (var i = 0; i < adam.Length; i++)
      {
        if (config.StrictAngles == AnglesEnum.Vertical)
        {
          angles.Add(strictVerticalAngles[i % strictVerticalAngles.Length]);
        }
        else if (config.StrictAngles == AnglesEnum.Horizontal)
        {
          angles.Add(strictHorizontalAngles[i % strictHorizontalAngles.Length]);
        }
        else
        {
          var angle = Math.Floor(r.NextDouble() * Config.Rotations) * (360f / Config.Rotations);
          angles.Add(angle);
        }
      }

      Population = new PopulationItem[config.PopulationSize];
      Population[0] = new PopulationItem(adam.ToList(), angles.ToArray());
      for (int i = 1; i < config.PopulationSize; i++)
      {
        var mutant = this.Mutate(Population[0]);
        Population[i] = mutant;
      }
    }

    public Procreant(NestItem<INfp>[] parts, ISvgNestConfig config)
      : this(CreateAdam(parts), config)
    {
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

      adam = adam.OrderByDescending(z => Math.Abs(GeometryUtil.polygonArea(z))).ToList();
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
        if (rand < 0.01 * Config.MutationRate)
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
        if (rand < 0.01 * Config.MutationRate)
        {
          if (clone.Parts[i].StrictAngle == AnglesEnum.Vertical || (clone.Parts[i].StrictAngle == AnglesEnum.None && Config.StrictAngles == AnglesEnum.Vertical))
          {
            clone.Rotation[i] = strictVerticalAngles[random.Next() % strictVerticalAngles.Length];
          }
          else if (clone.Parts[i].StrictAngle == AnglesEnum.Horizontal || (clone.Parts[i].StrictAngle == AnglesEnum.None && Config.StrictAngles == AnglesEnum.Horizontal))
          {
            clone.Rotation[i] = strictHorizontalAngles[random.Next() % strictHorizontalAngles.Length];
          }
          else
          {
            clone.Rotation[i] = Math.Floor(r.NextDouble() * Config.Rotations) * (360f / Config.Rotations);
          }
        }
      }

      return clone;
    }

    private double[] shuffleArray(double[] array)
    {
      for (var i = array.Length - 1; i > 0; i--)
      {
        var j = (int)Math.Floor(r.NextDouble() * (i + 1));
        var temp = array[i];
        array[i] = array[j];
        array[j] = temp;
      }

      return array;
    }

    // returns a random individual from the population, weighted to the front of the list (lower fitness value is more likely to be selected)
    private PopulationItem randomWeightedIndividual(PopulationItem exclude = null)
    {
      //var pop = this.population.slice(0);
      var pop = this.Population.ToArray();

      if (exclude != null && Array.IndexOf(pop, exclude) >= 0)
      {
        pop.Splice(Array.IndexOf(pop, exclude), 1);
      }

      var rand = r.NextDouble();

      double lower = 0;
      var weight = 1 / (double)pop.Length;
      var upper = weight;

      for (var i = 0; i < pop.Length; i++)
      {
        // if the random number falls between lower and upper bounds, select this individual
        if (rand > lower && rand < upper)
        {
          return pop[i];
        }

        lower = upper;
        upper += 2 * weight * ((pop.Length - i) / (double)pop.Length);
      }

      return pop[0];
    }

    // single point crossover
    private PopulationItem[] mate(PopulationItem male, PopulationItem female)
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
      var newpopulation = new List<PopulationItem>();
      var fittestSurvivors = Config.PopulationSize / 10;
      newpopulation.AddRange(this.Population.Take(this.Population.Count() < fittestSurvivors ? this.Population.Count() : fittestSurvivors));
      while (newpopulation.Count() < this.Population.Length)
      {
        var male = randomWeightedIndividual();
        var female = randomWeightedIndividual(male);

        // each mating produces two children
        var children = mate(male, female);

        // slightly mutate children
        newpopulation.Add(this.Mutate(children[0]));

        if (newpopulation.Count < this.Population.Length)
        {
          newpopulation.Add(this.Mutate(children[1]));
        }
      }

      this.Population = newpopulation.ToArray();
    }
  }

}
