namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.Placement;
  using Light.GuardClauses;

  public class PlacementWorker : IPlacementWorker, ITestPlacementWorker
  {
#if NCRUNCH
    internal const bool NCrunchTrace = false;
    private bool firstNCrunchTrace = false;
#endif

    private readonly SheetPlacementCollection allPlacements = new SheetPlacementCollection();
    private readonly NfpHelper nfpHelper;
    private readonly IEnumerable<ISheet> sheets;
    private readonly Gene gene;
    private readonly IPlacementConfig config;
    private readonly Stopwatch backgroundStopwatch;
    private readonly INestState state;
    private Stopwatch sw;
    private Stack<ISheet> unusedSheets;
    private List<INfp> unplacedParts;
    private PartPlacementWorker lastPartPlacementWorker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementWorker"/> class.
    /// </summary>
    /// <param name="nfpHelper">NfpHelper provides access to the Nfp cache generated in the PMap stage and this will add to it, potentially.</param>
    /// <param name="sheets">The list of sheets upon which to place parts.</param>
    /// <param name="gene">The list of parts to be placed.</param>
    /// <param name="config">Config for the Nest.</param>
    /// <param name="backgroundStopwatch">Stopwatch started at Background.Start (included the PMap stage prior to the PlacementWorker).</param>
    public PlacementWorker(NfpHelper nfpHelper, IEnumerable<ISheet> sheets, Gene gene, IPlacementConfig config, Stopwatch backgroundStopwatch, INestState state)
    {
      this.nfpHelper = nfpHelper;
      this.sheets = sheets;
      this.gene = gene;
      gene.Select(o => o.Part.Id).Distinct().Count().MustBe(gene.Length, message: "Parts must have unique Ids.");
      this.config = config;
      this.backgroundStopwatch = backgroundStopwatch;
      this.state = state;
    }

    PartPlacementWorker ITestPlacementWorker.LastPartPlacementWorker
    {
      get
      {
        return lastPartPlacementWorker;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the current loop started as a PriorityPLacement.
    /// Note as parts get placed this could change; hence we memoise at the start of each placement.
    /// </summary>
    private bool StartedAsPriorityPlacement => config.UsePriority && unplacedParts.Any(o => o.IsPriority);

    internal NestResult PlaceParts()
    {
      VerboseLog("PlaceParts");
      if (this.sheets == null || this.sheets.Count() == 0)
      {
        return null;
      }

      Initialise();
      while (unplacedParts.Count > 0 && unusedSheets.Count > 0)
      {
        // open a new sheet
        ISheet sheet;
        Queue<ISheet> requeue;
        List<IPartPlacement> placements;

        if (!TryGetSheet(out sheet, out placements, out requeue))
        {
          VerboseLog("No sheets left to place parts upon; break and end the nest.");
          break;
        }

        var isPriorityPlacement = config.UsePriority && StartedAsPriorityPlacement;
        if (isPriorityPlacement)
        {
          VerboseLog("Priority Placement.");
        }

        lastPartPlacementWorker = new PartPlacementWorker(this, config, gene, placements, sheet, nfpHelper, state);
        var processingParts = (isPriorityPlacement ? unplacedParts.Where(o => o.IsPriority).Union(unplacedParts.Where(o => !o.IsPriority)) : unplacedParts).ToArray();
        for (int processingPartIndex = 0; processingPartIndex < processingParts.Length; processingPartIndex++)
        {
          var processingPart = processingParts[processingPartIndex];
          var partIndex = gene.IndexOf(gene.Single(o => o.Part.Id == processingPart.Id));
          var processPartResult = lastPartPlacementWorker.ProcessPart(processingParts[processingPartIndex], partIndex);
          if (processPartResult == InnerFlowResult.Break)
          {
            break;
          }
          else if (processPartResult == InnerFlowResult.Continue)
          {
            continue;
          }
        }

        VerboseLog("All parts processed for current sheet.");
        if (isPriorityPlacement && unplacedParts.Count > 0)
        {
          VerboseLog($"Requeue {sheet.ToShortString()} for reuse.");
          unusedSheets.Push(sheet);
        }
        else
        {
          VerboseLog($"No need to requeue {sheet.ToShortString()}.");
        }

        while (requeue.Count > 0)
        {
          VerboseLog($"Reinstate {sheet.ToShortString()} for reuse.");
          unusedSheets.Push(requeue.Dequeue());
        }

        if (lastPartPlacementWorker.Placements != null && lastPartPlacementWorker.Placements.Count > 0)
        {
          VerboseLog($"Add {config.PlacementType} placement {sheet.ToShortString()}.");
          allPlacements.Add(new SheetPlacement(config.PlacementType, sheet, lastPartPlacementWorker.Placements, lastPartPlacementWorker.MergedLength, config.ClipperScale));
        }
        else
        {
          VerboseLog($"Something's gone wrong; break out of nest.");
          break; // something went wrong
        }
      }

      VerboseLog($"Nest complete in {sw.ElapsedMilliseconds}");
      var result = new NestResult(gene.Length, allPlacements, unplacedParts, config.PlacementType, sw.ElapsedMilliseconds, backgroundStopwatch.ElapsedMilliseconds);
#if NCRUNCH || DEBUG
      if (!result.IsValid)
      {
        throw new InvalidOperationException("Invalid nest generated.");
      }
#endif
      return result;
    }

    SheetPlacement IPlacementWorker.AddPlacement(INfp inputPart, List<IPartPlacement> placements, INfp processedPart, PartPlacement position, PlacementTypeEnum placementType, ISheet sheet, double mergedLength)
    {
      try
      {
        if (!this.unplacedParts.Remove(inputPart))
        {
#if NCRUNCH || DEBUG
          throw new InvalidOperationException("Failed to locate the part just placed in unplaced parts!");
#endif
        }
#if NCRUNCH || DEBUG
        position.Part.MustBe(processedPart);
        (this.allPlacements.TotalPartsPlaced + placements.Count).MustBeLessThanOrEqualTo(gene.Length);
#endif
        this.VerboseLog($"Placed part {processedPart}");
        placements.Add(position);
        var sp = new SheetPlacement(placementType, sheet, placements, mergedLength, config.ClipperScale);
        if (double.IsNaN(sp.Fitness.Evaluate()))
        {
          // Step back to calling method in PartPlacementWorker and you should find a PartPlacementWorker.ToJson() :)
          // Get that in to a Json file so it can be debugged.
          System.Diagnostics.Debugger.Break();
        }

        return sp;
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    private bool TryGetSheet(out ISheet sheet, out List<IPartPlacement> partPlacements, out Queue<ISheet> requeue)
    {
      ISheet localSheet = null;
      partPlacements = null;
      requeue = new Queue<ISheet>();
      while (unusedSheets.Count > 0 && localSheet == null)
      {
        localSheet = unusedSheets.Pop();
        if (allPlacements.Any(o => o.Sheet == localSheet))
        {
          var sheetPlacement = allPlacements.Single(o => o.Sheet == localSheet);
          partPlacements = sheetPlacement.PartPlacements.ToList();
          if (config.UsePriority && unplacedParts.Any(o => o.IsPriority))
          {
            // Sheet's already used so by definition it's already full of priority parts, no point trying to add more
            requeue.Enqueue(localSheet);
            localSheet = null;
          }
          else
          {
            VerboseLog($"Using sheet {localSheet.Id}:{localSheet.Source} because although it's already used for {partPlacements.Count()} priority parts there's no priority parts left so try fill spaces with non-priority:");
            allPlacements.Remove(sheetPlacement);
            sheet = localSheet;
            return true;
          }
        }
        else
        {
          VerboseLog($"Using sheet {localSheet.ToShortString()} because it's a new sheet so just go ahead and use it for whatever's left:");
          partPlacements = new List<IPartPlacement>();
          sheet = localSheet;
          return true;
        }
      }

      partPlacements = null;
      sheet = null;
      return false;
    }

    private void Initialise()
    {
      this.sw = new Stopwatch();
      sw.Start();

      this.unusedSheets = new Stack<ISheet>(sheets.Reverse());

      // rotate paths by given rotation
      unplacedParts = new List<INfp>();
      for (int i = 0; i < gene.Length; i++)
      {
        var r = gene[i].Part.Rotate(gene[i].Rotation);
        r.Rotation = gene[i].Rotation; //--> 8->33
        r.Source = gene[i].Part.Source;
        r.Id = gene[i].Part.Id;
        unplacedParts.Add(r);
      }
    }

    void IPlacementWorker.VerboseLog(string message)
    {
      VerboseLog(message);
    }

    private void VerboseLog(string message)
    {
#if NCRUNCH
      if (NCrunchTrace)
      {
        Trace.WriteLine(message);
      }
      else
      {
        if (firstNCrunchTrace)
        {
          firstNCrunchTrace = false;
          Trace.WriteLine("Background.NCrunchTrace disabled");
        }
      }
#endif
    }
  }
}