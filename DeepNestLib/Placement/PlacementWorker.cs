namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using ClipperLib;
  using DeepNestLib.Placement;
  using Light.GuardClauses;
  using static DeepNestLib.PlacementWorker;

  public class PlacementWorker : IPlacementWorker
  {
#if NCRUNCH
    internal const bool NCrunchTrace = false;
    private bool firstNCrunchTrace = false;
#endif

    private readonly SheetPlacementCollection allPlacements = new SheetPlacementCollection();
    private readonly NfpHelper nfpHelper;
    private readonly IEnumerable<ISheet> sheets;
    private readonly INfp[] parts;
    private readonly ISvgNestConfig config;
    private readonly Stopwatch backgroundStopwatch;
    private readonly INestState state;
    private Stopwatch sw;
    private Stack<ISheet> unusedSheets;
    private List<INfp> unplacedParts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementWorker"/> class.
    /// </summary>
    /// <param name="nfpHelper">NfpHelper provides access to the Nfp cache generated in the PMap stage and this will add to it, potentially.</param>
    /// <param name="sheets">The list of sheets upon which to place parts.</param>
    /// <param name="parts">The list of parts to be placed.</param>
    /// <param name="config">Config for the Nest.</param>
    /// <param name="backgroundStopwatch">Stopwatch started at Background.Start (included the PMap stage prior to the PlacementWorker).</param>
    public PlacementWorker(NfpHelper nfpHelper, IEnumerable<ISheet> sheets, INfp[] parts, ISvgNestConfig config, Stopwatch backgroundStopwatch, INestState state)
    {
      this.nfpHelper = nfpHelper;
      this.sheets = sheets;
      this.parts = parts;
      parts.Select(o => o.Id).Distinct().Count().MustBe(parts.Length, message: "Parts must have unique Ids.");
      this.config = config;
      this.backgroundStopwatch = backgroundStopwatch;
      this.state = state;
    }

    /// <summary>
    /// Gets a value indicating whether the current loop started as a PriorityPLacement. 
    /// Note as parts get placed this could change; hence we memoise at the start of each placement.
    /// </summary>
    private bool StartedAsPriorityPlacement => unplacedParts.Any(o => o.IsPriority);

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

        var isPriorityPlacement = StartedAsPriorityPlacement;
        if (isPriorityPlacement)
        {
          VerboseLog("Priority Placement.");
        }

        var partPlacementWorker = new PartPlacementWorker(this, config, parts, placements, sheet, nfpHelper, state);
        var processingParts = (isPriorityPlacement ? unplacedParts.Where(o => o.IsPriority) : unplacedParts).ToArray();
        for (int processingPartIndex = 0; processingPartIndex < processingParts.Length; processingPartIndex++)
        {
          var processingPart = processingParts[processingPartIndex];
          var partIndex = parts.IndexOf(parts.Single(o => o.Id == processingPart.Id));
          var processPartResult = partPlacementWorker.ProcessPart(processingParts[processingPartIndex], partIndex);
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

        if (partPlacementWorker.Placements != null && partPlacementWorker.Placements.Count > 0)
        {
          VerboseLog($"Add {config.PlacementType} placement {sheet.ToShortString()}.");
          allPlacements.Add(new SheetPlacement(config.PlacementType, sheet, partPlacementWorker.Placements, partPlacementWorker.MergedLength));
        }
        else
        {
          VerboseLog($"Something's gone wrong; break out of nest.");
          break; // something went wrong
        }
      }

      VerboseLog($"Nest complete in {sw.ElapsedMilliseconds}");
      var result = new NestResult(parts.Length, allPlacements, unplacedParts, config.PlacementType, sw.ElapsedMilliseconds, backgroundStopwatch.ElapsedMilliseconds);
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
        (this.allPlacements.TotalPartsPlaced + placements.Count).MustBeLessThanOrEqualTo(parts.Length);
#endif
        this.VerboseLog($"Placed part {processedPart}");
        placements.Add(position);
        var sp = new SheetPlacement(placementType, sheet, placements, mergedLength);
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
          if (unplacedParts.Any(o => o.IsPriority))
          {
            // Sheet's already used so by definition it's already full of priority parts, no point trying to add more
            requeue.Enqueue(localSheet);
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
      for (int i = 0; i < parts.Length; i++)
      {
        var r = parts[i].Rotate(parts[i].Rotation);
        r.Rotation = parts[i].Rotation;
        r.Source = parts[i].Source;
        r.Id = parts[i].Id;
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