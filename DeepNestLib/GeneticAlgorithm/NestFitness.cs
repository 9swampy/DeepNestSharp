namespace DeepNestLib.GeneticAlgorithm
{
  // https://www.researchgate.net/publication/276909495_An_optimizing_model_to_solve_the_nesting_problem_of_rectangle_pieces_based_on_genetic_algorithm

  using GeneticSharp.Domain.Chromosomes;
  using GeneticSharp.Domain.Fitnesses;
  using GeneticSharp.Domain.Randomizations;

  public class NestFitness : IFitness
  {
    public double Evaluate(IChromosome chromosome)
    {
      throw new System.NotImplementedException();
    }
  }
}
