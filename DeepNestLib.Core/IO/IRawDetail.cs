namespace DeepNestLib.IO
{
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;

  public interface IRawDetail
  {
    string Name { get; }

    IReadOnlyCollection<ILocalContour> Outers { get; }

    bool TryConvertToNfp(int src, out INfp loadedNfp);

    bool TryConvertToNfp(int src, out Chromosome chromosome);

    bool TryConvertToSheet(int v, out ISheet firstSheet);

    INfp ToNfp();

    ISheet ToSheet();

    bool TryConvertToNfp(int firstPartIdSrc, int v, out IDeepNestChromosome firstPart);
  }
}