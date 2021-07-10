namespace DeepNestLib.GeneticAlgorithm
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using System.Text;

  public class Procreant
  {
    private readonly Random r = new Random();

    private readonly ISvgNestConfig Config;
    public List<PopulationItem> Population;

    private float[] defaultAngles = new float[]
    {
      0,
      180,
    };

    //private float[] defaultAngles = new float[]
    //{
    //  90,
    //  270,
    //};

    //private float[] defaultAngles = new float[]
    //{
    //  0,
    //  180,
    //  //0,
    //  //0,
    //  //270,
    //  //180,
    //  //180,
    //  //180,
    //  //90
    //};

    public Procreant(NFP[] adam, ISvgNestConfig config)
    {
      Config = config;

      var angles = new List<float>();
      for (var i = 0; i < adam.Length; i++)
      {
        if (config.StrictAngles)
        {
          angles.Add(defaultAngles[i % defaultAngles.Length]);
        }
        else
        {
          var angle = (float)Math.Floor(r.NextDouble() * Config.Rotations) * (360f / Config.Rotations);
          angles.Add(angle);
        }
      }

      Population = new List<PopulationItem>();
      Population.Add(new PopulationItem(adam.ToList(), angles.ToArray()));
      while (Population.Count() < config.PopulationSize)
      {
        var mutant = this.mutate(Population[0]);
        Population.Add(mutant);
      }
    }

    public Procreant(NestItem[] parts, ISvgNestConfig config)
      : this(CreateAdam(parts), config)
    {
    }

    private static NFP[] CreateAdam(NestItem[] parts)
    {
      List<NFP> adam = new List<NFP>();
      var id = 0;
      for (int i = 0; i < parts.Count(); i++)
      {
        if (!parts[i].IsSheet)
        {
          for (int j = 0; j < parts[i].Quantity; j++)
          {
            var poly = parts[i].Polygon.CloneTree(); // deep copy
            poly.Id = id; // id is the unique id of all parts that will be nested, including cloned duplicates
            poly.Source = i; // source is the id of each unique part from the main part list

            adam.Add(poly);
            id++;
          }
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
        for (int i = 0; i < this.Population.Count; i++)
        {
          if (this.Population[i].fitness == null)
          {
            return false;
          }
        }

        return true;
      }
    }

    private PopulationItem mutate(PopulationItem p)
    {
      var clone = new PopulationItem(p.Placements.ToArray().ToList(), p.Rotation.Clone() as float[]);
      for (var i = 0; i < clone.Placements.Count(); i++)
      {
        var rand = r.NextDouble();
        if (rand < 0.01 * Config.MutationRate)
        {
          var j = i + 1;
          if (j < clone.Placements.Count)
          {
            var temp = clone.Placements[i];
            clone.Placements[i] = clone.Placements[j];
            clone.Placements[j] = temp;
          }
        }

        rand = r.NextDouble();
        if (rand < 0.01 * Config.MutationRate)
        {
          if (Config.StrictAngles)
          {
            clone.Rotation[i] = defaultAngles[random.Next() % defaultAngles.Length];
          }
          else
          {
            clone.Rotation[i] = (float)Math.Floor(r.NextDouble() * Config.Rotations) * (360f / Config.Rotations);
          }
        }
      }

      return clone;
    }

    private float[] shuffleArray(float[] array)
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

      float lower = 0;
      var weight = 1 / (float)pop.Length;
      var upper = weight;

      for (var i = 0; i < pop.Length; i++)
      {
        // if the random number falls between lower and upper bounds, select this individual
        if (rand > lower && rand < upper)
        {
          return pop[i];
        }

        lower = upper;
        upper += 2 * weight * ((pop.Length - i) / (float)pop.Length);
      }

      return pop[0];
    }

    // single point crossover
    private PopulationItem[] mate(PopulationItem male, PopulationItem female)
    {
      var cutpoint = (int)Math.Round(Math.Min(Math.Max(r.NextDouble(), 0.1), 0.9) * (male.Placements.Count - 1));

      var gene1 = new List<NFP>(male.Placements.Take(cutpoint).ToArray());
      var rot1 = new List<float>(male.Rotation.Take(cutpoint).ToArray());

      var gene2 = new List<NFP>(female.Placements.Take(cutpoint).ToArray());
      var rot2 = new List<float>(female.Rotation.Take(cutpoint).ToArray());

      var i = 0;

      for (i = 0; i < female.Placements.Count; i++)
      {
        if (!gene1.Any(z => z.Id == female.Placements[i].Id))
        {
          gene1.Add(female.Placements[i]);
          rot1.Add(female.Rotation[i]);
        }
      }

      for (i = 0; i < male.Placements.Count; i++)
      {
        if (!gene2.Any(z => z.Id == male.Placements[i].Id))
        {
          gene2.Add(male.Placements[i]);
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
      Population = Population.OrderBy(z => z.fitness).ToList();

      // fittest individuals are preserved in the new generation (elitism)
      var newpopulation = new List<PopulationItem>();
      var fittestSurvivors = Config.PopulationSize / 10;
      newpopulation.AddRange(this.Population.Take(this.Population.Count() < fittestSurvivors ? this.Population.Count() : fittestSurvivors));
      while (newpopulation.Count() < this.Population.Count)
      {
        var male = randomWeightedIndividual();
        var female = randomWeightedIndividual(male);

        // each mating produces two children
        var children = mate(male, female);

        // slightly mutate children
        newpopulation.Add(this.mutate(children[0]));

        if (newpopulation.Count < this.Population.Count)
        {
          newpopulation.Add(this.mutate(children[1]));
        }
      }

      this.Population = newpopulation;
    }
  }


}
