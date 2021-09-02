﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading.Tasks;
  using DeepNestLib.Geometry;

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
      this.nfpHelper = new NfpHelper(minkowskiSumService, window);
    }

    public INfp GetPart(int source, INfp[] parts)
    {
      for (var k = 0; k < parts.Length; k++)
      {
        if (parts[k].Source == source)
        {
          return parts[k];
        }
      }

      return null;
    }

    internal void BackgroundStart(IDataInfo data, ISvgNestConfig config)
    {
      try
      {
        var backgroundStopwatch = new Stopwatch();
        backgroundStopwatch.Start();
        var individual = data.Individual;

        var parts = individual.Parts.ToArray();
        var rotations = individual.Rotation;
        var ids = data.Ids;
        var sources = data.Sources;
        var children = data.Children;

        for (var i = 0; i < parts.Length; i++)
        {
          parts[i].Rotation = rotations[i];
          parts[i].Id = ids[i];
          parts[i].Source = sources[i];
          if (!config.Simplify)
          {
            parts[i].Children = children[i];
          }
        }

        var sheets = data.Sheets;
        for (int i = 0; i < sheets.Length; i++)
        {
          var sheet = sheets[i];
          sheet.Id = data.SheetIds[i];
          sheet.Source = data.SheetSources[i];
          sheet.Children = data.SheetChildren[i];
        }

        // preprocess
        List<NfpPair> pairs = new NfpPairsFactory(window).Generate(config.UseParallel, parts);

        // console.log('pairs: ', pairs.length);
        // console.time('Total');
        if (pairs.Count > 0)
        {
          var pmapWorker = new PmapWorker(pairs, progressDisplayer, config.UseParallel, minkowskiSumService, state);
          var pmapResult = pmapWorker.PmapDeepNest();
          this.ThenDeepNest(pmapResult, parts, data.Sheets, config, data.Index, backgroundStopwatch);
        }
        else
        {
          this.SyncPlaceParts(parts, data.Sheets, config, data.Index, backgroundStopwatch);
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

    private void SyncPlaceParts(INfp[] parts, ISheet[] sheets, ISvgNestConfig config, int index, Stopwatch backgroundStopwatch)
    {
      try
      {
        var nestResult = new PlacementWorker(this.nfpHelper, sheets, parts, config, backgroundStopwatch, state).PlaceParts();
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

    private void ThenIterate(NfpPair processed, INfp[] parts, double clipperScale)
    {
      // returned data only contains outer nfp, we have to account for any holes separately in the synchronous portion
      // this is because the c++ addon which can process interior nfps cannot run in the worker thread
      var a = this.GetPart(processed.Asource, parts);
      var b = this.GetPart(processed.Bsource, parts);

      var aChildren = new List<INfp>();

      if (a.Children != null)
      {
        for (int j = 0; j < a.Children.Count; j++)
        {
          aChildren.Add(a.Children[j].Rotate(processed.ARotation));
        }
      }

      if (aChildren.Count > 0)
      {
        var bRotated = b.Rotate(processed.BRotation);
        var bBounds = GeometryUtil.GetPolygonBounds(bRotated);
        var cnfp = new List<INfp>();

        for (int j = 0; j < aChildren.Count; j++)
        {
          var cbounds = GeometryUtil.GetPolygonBounds(aChildren[j]);
          if (cbounds.Width > bBounds.Width && cbounds.Height > bBounds.Height)
          {
            var n = nfpHelper.GetInnerNfp(aChildren[j], bRotated, MinkowskiCache.NoCache, clipperScale, this.useDllImport);
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
      window.Insert(keyItem);
    }

    private void ThenDeepNest(NfpPair[] nfpPairs, INfp[] parts, ISheet[] sheets, ISvgNestConfig config, int index, Stopwatch backgroundStopwatch)
    {
      progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, "Placement. . .", nfpPairs.Length);
      if (config.UseParallel)
      {
        Parallel.For(0, nfpPairs.Count(), (i) =>
        {
          this.ThenIterate(nfpPairs[i], parts, config.ClipperScale);
        });
      }
      else
      {
        for (var i = 0; i < nfpPairs.Count(); i++)
        {
          this.ThenIterate(nfpPairs[i], parts, config.ClipperScale);
        }
      }

      // console.timeEnd('Total');
      // console.log('before sync');
      this.SyncPlaceParts(parts, sheets, config, index, backgroundStopwatch);
      progressDisplayer.SetIsVisibleSecondaryProgressBar(false);
    }
  }
}
