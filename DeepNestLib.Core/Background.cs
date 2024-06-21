namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.PairMap;
  using DeepNestLib.Placement;

  public class Background
  {
    private readonly IProgressDisplayer progressDisplayer;
    private readonly SvgNest nest;
    private readonly IMinkowskiSumService minkowskiSumService;
    private readonly INestStateBackground state;
    private readonly bool useDllImport;
    private readonly NfpHelper nfpHelper;

    // run the placement synchronously
    private IWindowUnk window = new WindowUnk();

    /// <summary>
    /// Initializes a new instance of the <see cref="Background"/> class.
    /// Needs to be totally self contained so it can calculate multiple nests in parallel.
    /// </summary>
    /// <param name="progressDisplayer">Callback access to the executing UI.</param>
    /// <param name="nest">Passed in because have had issues with nest.ResponseProcessor accepting responses after a new nest has already been started.</param>
    /// <param name="minkowskiSumService">MinkowskiSumService used to inject algorithms to calculate the No-Fit-Polygons critical to DeepNest.</param>
    public Background(IProgressDisplayer progressDisplayer, SvgNest nest, IMinkowskiSumService minkowskiSumService, INestStateBackground state, bool useDllImport)
    {
      this.window = new WindowUnk();
      this.progressDisplayer = progressDisplayer;
      this.nest = nest;
      this.minkowskiSumService = minkowskiSumService;
      this.state = state;
      this.useDllImport = useDllImport;
      this.nfpHelper = new NfpHelper(minkowskiSumService, this.window);
    }

    internal void BackgroundStart(PopulationItem individual, ISheet[] sheets, ISheet[] originalSheets, ISvgNestConfig config)
    {
      try
      {
        Stopwatch backgroundStopwatch = new Stopwatch();
        backgroundStopwatch.Start();
        DeepNestGene gene = individual.Gene;
        for (int i = 0; i < sheets.Length; i++)
        {
          ISheet sheet = sheets[i];
        }

        // preprocess
        List<NfpPair> pairs = new NfpPairsFactory(this.window).Generate(config.UseParallel, gene);

        // console.log('pairs: ', pairs.length);
        // console.time('Total');
        if (pairs.Count > 0)
        {
          PmapWorker pmapWorker = new PmapWorker(pairs, this.progressDisplayer, config.UseParallel, this.minkowskiSumService, this.state);
          NfpPair[] pmapResult = pmapWorker.PmapDeepNest();
          this.ThenDeepNest(pmapResult, gene, sheets, originalSheets, config, individual.Index, backgroundStopwatch);
        }
        else
        {
          this.SyncPlaceParts(gene, sheets, originalSheets, config, individual.Index, backgroundStopwatch);
        }
      }
      catch (ArgumentNullException)
      {
        throw;
      }
      catch (DllNotFoundException)
      {
        throw;
      }
      catch (BadImageFormatException)
      {
        throw;
      }
      catch (SEHException)
      {
        throw;
      }
    }

    private void SyncPlaceParts(DeepNestGene gene, ISheet[] sheets, ISheet[] originalSheets, ISvgNestConfig config, int index, Stopwatch backgroundStopwatch)
    {
      try
      {
        NestResult nestResult = new PlacementWorker(this.nfpHelper, sheets, originalSheets, gene, config, backgroundStopwatch, this.state).PlaceParts();
        if (nestResult != null)
        {
          nestResult.Index = index;
          this.nest.ResponseProcessor(nestResult);
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    private void ThenIterate(NfpPair processed, DeepNestGene gene, double clipperScale)
    {
      // returned data only contains outer nfp, we have to account for any holes separately in the synchronous portion
      // this is because the c++ addon which can process interior nfps cannot run in the worker thread
      Chromosome holeProvider = gene.FirstOrDefault(p => p.Part.Source == processed.Asource);
      Chromosome partToFit = gene.FirstOrDefault(p => p.Part.Source == processed.Bsource);

      List<INfp> holes = new List<INfp>();

      if (holeProvider.Part.Children != null && holeProvider.Part.Children.Count > 0)
      {
        for (int j = 0; j < holeProvider.Part.Children.Count; j++)
        {
          holes.Add(holeProvider.Part.Children[j].Rotate(processed.ARotation));
        }

        INfp partRotated = partToFit.Part.Rotate(processed.BRotation);
        PolygonBounds partBounds = GeometryUtil.GetPolygonBounds(partRotated);
        List<INfp> cnfp = new List<INfp>();

        for (int j = 0; j < holes.Count; j++)
        {
          PolygonBounds holeBounds = GeometryUtil.GetPolygonBounds(holes[j]);
          if (holeBounds.Width > partBounds.Width && holeBounds.Height > partBounds.Height)
          {
            INfp[] n = this.nfpHelper.GetInnerNfp(holes[j], partRotated, MinkowskiCache.NoCache, clipperScale, this.useDllImport, o => { });
            if (n != null && n.Count() > 0)
            {
              cnfp.AddRange(n);
            }
          }
        }

        processed.Nfp.Children = cnfp;
      }

      DbCacheKey keyItem = new DbCacheKey(processed.Asource, processed.Bsource, processed.ARotation, processed.BRotation, new[] { processed.Nfp });

      /*var doc = {
              A: processed[i].Asource,
              B: processed[i].Bsource,
              Arotation: processed[i].Arotation,
              Brotation: processed[i].Brotation,
              nfp: processed[i].nfp

          };*/
      this.window.Insert(keyItem);
    }

    private void ThenDeepNest(NfpPair[] nfpPairs, DeepNestGene gene, ISheet[] sheets, ISheet[] originalSheets, ISvgNestConfig config, int index, Stopwatch backgroundStopwatch)
    {
      bool hideSecondaryProgress = false;
      if (this.state.NestCount == 0 || this.state.AverageNestTime > 2000)
      {
        hideSecondaryProgress = true;
        this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Placement...", nfpPairs.Length);
      }

      if (config.UseParallel)
      {
        Parallel.For(0, nfpPairs.Count(), (i) =>
        {
          this.ThenIterate(nfpPairs[i], gene, config.ClipperScale);
        });
      }
      else
      {
        for (var i = 0; i < nfpPairs.Count(); i++)
        {
          this.ThenIterate(nfpPairs[i], gene, config.ClipperScale);
        }
      }

      // console.timeEnd('Total');
      // console.log('before sync');
      this.SyncPlaceParts(gene, sheets, originalSheets, config, index, backgroundStopwatch);
      if (hideSecondaryProgress)
      {
        this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      }
    }
  }
}
