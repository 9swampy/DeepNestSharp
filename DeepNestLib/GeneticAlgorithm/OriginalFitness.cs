namespace DeepNestLib.GeneticAlgorithm
{
  using DeepNestLib.Placement;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public class OriginalFitness
  {
    public double Evaluate(NestResult sheetPlacement)
    {
      var result = 0D;
      result += sheetPlacement.UsedSheets.Count * sheetPlacement.area;
      // sheetPlacement.placements[0].Sum(o=>o.sheetplacements.)

      // 100000000 * (Math.Abs(GeometryUtil.polygonArea(parts[noPlaceIdx])) / totalsheetarea);

      return result;
    }

    // https://www.researchgate.net/publication/276909495_An_optimizing_model_to_solve_the_nesting_problem_of_rectangle_pieces_based_on_genetic_algorithm

  }
}
