namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.GeneticAlgorithm;
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
    private readonly IEnumerable<ISheet> originalSheets;
    private readonly DeepNestGene gene;
    private readonly IPlacementConfig config;
    private readonly Stopwatch backgroundStopwatch;
    private readonly INestState state;
    private Stopwatch sw;
    private Stack<ISheet> unusedSheets;
    private Stack<ISheet> unusedOriginalSheets;
    private List<INfp> unplacedParts;
    private PartPlacementWorker lastPartPlacementWorker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementWorker"/> class.
    /// </summary>
    /// <param name="nfpHelper">NfpHelper provides access to the Nfp cache generated in the PMap stage and this will add to it, potentially.</param>
    /// <param name="sheets">The list of sheets upon which to place parts.</param>
    /// <param name="originalSheets">The list of un-offset sheets corresponding with sheets.</param>
    /// <param name="gene">The list of parts to be placed.</param>
    /// <param name="config">Config for the Nest.</param>
    /// <param name="backgroundStopwatch">Stopwatch started at Background.Start (included the PMap stage prior to the PlacementWorker).</param>
    public PlacementWorker(NfpHelper nfpHelper, IEnumerable<ISheet> sheets, IEnumerable<ISheet> originalSheets, DeepNestGene gene, IPlacementConfig config, Stopwatch backgroundStopwatch, INestState state)
    {
      this.nfpHelper = nfpHelper;
      this.sheets = sheets;
      this.originalSheets = originalSheets;
      this.gene = gene;
      gene.Select(o => o.Part.Id).Distinct().Count().MustBe(gene.Length, message: "Parts must have unique Ids.");
      this.config = config;
      this.backgroundStopwatch = backgroundStopwatch;
      this.state = state;
    }

    PartPlacementWorker ITestPlacementWorker.LastPartPlacementWorker => this.lastPartPlacementWorker;

    /// <summary>
    /// Gets a value indicating whether the current loop started as a PriorityPLacement.
    /// Note as parts get placed this could change; hence we memoise at the start of each placement.
    /// </summary>
    private bool StartedAsPriorityPlacement => this.config.UsePriority && this.unplacedParts.Any(o => o.IsPriority);

    internal NestResult PlaceParts()
    {
      this.VerboseLog("PlaceParts");
      if (this.sheets == null || this.sheets.Count() == 0)
      {
        return null;
      }

      this.Initialise();
      ISheet sheet;
      ISheet originalSheet;
      Queue<ISheet> requeue;
      List<IPartPlacement> placements;
      while (this.unplacedParts.Count > 0 && this.TryGetSheet(out sheet, out originalSheet, out placements, out requeue))
      {
        var isPriorityPlacement = this.config.UsePriority && this.StartedAsPriorityPlacement;
        if (isPriorityPlacement)
        {
          this.VerboseLog("Priority Placement.");
        }

        this.lastPartPlacementWorker = new PartPlacementWorker(this, this.config, this.gene, placements, sheet, this.nfpHelper, this.state);
        INfp[] processingParts = (isPriorityPlacement ? this.unplacedParts.Where(o => o.IsPriority).Union(this.unplacedParts.Where(o => !o.IsPriority)) : this.unplacedParts).ToArray();
        for (int processingPartIndex = 0; processingPartIndex < processingParts.Length; processingPartIndex++)
        {
          INfp processingPart = processingParts[processingPartIndex];
          var partIndex = this.gene.IndexOf(this.gene.Single(o => o.Part.Id == processingPart.Id));
          InnerFlowResult processPartResult = this.lastPartPlacementWorker.ProcessPart(processingParts[processingPartIndex], partIndex);
          if (processPartResult == InnerFlowResult.Break)
          {
            break;
          }
          else if (processPartResult == InnerFlowResult.Continue)
          {
            continue;
          }
        }

        this.RequeueSheets(sheet, requeue, isPriorityPlacement);
        if (this.lastPartPlacementWorker.Placements != null && this.lastPartPlacementWorker.Placements.Count > 0)
        {
          this.VerboseLog($"Add {this.config.PlacementType} placement {sheet.ToShortString()}.");
          this.allPlacements.Add(new SheetPlacement(this.config.PlacementType, sheet, originalSheet, this.lastPartPlacementWorker.Placements, this.lastPartPlacementWorker.MergedLength, this.config.ClipperScale));
        }
        else
        {
          this.VerboseLog($"Something's gone wrong; break out of nest.");
          break; // something went wrong
        }
      }

      this.VerboseLog($"Nest complete in {this.sw.ElapsedMilliseconds}");
      NestResult result = new NestResult(this.gene.Length, this.allPlacements, this.unplacedParts, this.config.PlacementType, this.sw.ElapsedMilliseconds, this.backgroundStopwatch.ElapsedMilliseconds);
#if NCRUNCH || DEBUG
      if (!result.IsValid)
      {
        throw new InvalidOperationException("Invalid nest generated.");
      }
#endif
      return result;
    }

    /// <summary>
    /// Requeues sheets so the nest can attempt to use them again; for example if this pass was for Priority, requeue for non-priority placement.
    /// </summary>
    /// <param name="sheet">Current sheet being populated.</param>
    /// <param name="requeue">Incumbent requeue pending.</param>
    /// <param name="isPriorityPlacement">A flag that indicates that the current nest was priority.</param>
    private void RequeueSheets(ISheet sheet, Queue<ISheet> requeue, bool isPriorityPlacement)
    {
      this.VerboseLog("All parts processed for current sheet.");
      if (isPriorityPlacement && this.unplacedParts.Count > 0)
      {
        this.VerboseLog($"Requeue {sheet.ToShortString()} for reuse.");
        this.unusedSheets.Push(sheet);
      }
      else
      {
        this.VerboseLog($"No need to requeue {sheet.ToShortString()}.");
      }

      while (requeue.Count > 0)
      {
        this.VerboseLog($"Reinstate {sheet.ToShortString()} for reuse.");
        this.unusedSheets.Push(requeue.Dequeue());
      }
    }

    SheetPlacement IPlacementWorker.AddPlacement(INfp inputPart, List<IPartPlacement> placements, INfp processedPart, PartPlacement position, PlacementTypeEnum placementType, ISheet sheet, ISheet originalSheet, double mergedLength)
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
        (this.allPlacements.TotalPartsPlaced + placements.Count).MustBeLessThanOrEqualTo(this.gene.Length);
#endif
        this.VerboseLog($"Placed part {processedPart}");
        placements.Add(position);
        SheetPlacement sp = new SheetPlacement(placementType, sheet, originalSheet, placements, mergedLength, this.config.ClipperScale);
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

    /// <summary>
    /// Gets the next sheet to populate.
    /// </summary>
    /// <param name="sheet">The next sheet to populate with parts.</param>
    /// <param name="originalSheet">The un-offset sheet corresponding with sheet.</param>
    /// <param name="partPlacements">Any partPlacements already on the sheet (e.g. priority).</param>
    /// <param name="requeue">Already used sheets that already cannot accept more parts at this time (e.g. priority).</param>
    /// <returns>.t if a sheet was available.</returns>
    private bool TryGetSheet(out ISheet sheet, out ISheet originalSheet, out List<IPartPlacement> partPlacements, out Queue<ISheet> requeue)
    {
      ISheet localSheet = null;
      ISheet localOriginalSheet = null;
      partPlacements = null;
      requeue = new Queue<ISheet>();
      while (this.unusedSheets.Count > 0 && localSheet == null)
      {
        localSheet = this.unusedSheets.Pop();
        localOriginalSheet = this.unusedOriginalSheets.Pop();
        if (this.allPlacements.Any(o => o.Sheet == localSheet))
        {
          ISheetPlacement sheetPlacement = this.allPlacements.Single(o => o.Sheet == localSheet);
          partPlacements = sheetPlacement.PartPlacements.ToList();
          if (this.config.UsePriority && this.unplacedParts.Any(o => o.IsPriority))
          {
            // Sheet's already used so by definition it's already full of priority parts, no point trying to add more
            requeue.Enqueue(localSheet);
            localSheet = null;
            localOriginalSheet = null;
          }
          else
          {
            this.VerboseLog($"Using sheet {localSheet.Id}:{localSheet.Source} because although it's already used for {partPlacements.Count()} priority parts there's no priority parts left so try fill spaces with non-priority:");
            this.allPlacements.Remove(sheetPlacement);
            sheet = localSheet;
            originalSheet = localOriginalSheet;
            return true;
          }
        }
        else
        {
          this.VerboseLog($"Using sheet {localSheet.ToShortString()} because it's a new sheet so just go ahead and use it for whatever's left:");
          partPlacements = new List<IPartPlacement>();
          sheet = localSheet;
          originalSheet = localOriginalSheet;
          return true;
        }
      }

      partPlacements = null;
      sheet = null;
      originalSheet = null;
      return false;
    }

    private void Initialise()
    {
      this.StartTimer();
      this.PrepUsedSheets();
      this.PrepUnplacedParts();
    }

    private void StartTimer()
    {
      this.sw = new Stopwatch();
      this.sw.Start();
    }

    private void PrepUsedSheets()
    {
      this.unusedSheets = new Stack<ISheet>(this.sheets.Reverse());
      this.unusedOriginalSheets = new Stack<ISheet>(this.originalSheets.Reverse());
    }

    private void PrepUnplacedParts()
    {
      // rotate paths by given rotation
      this.unplacedParts = new List<INfp>();
      for (int i = 0; i < this.gene.Length; i++)
      {
        this.unplacedParts.Add(this.gene[i].Part.Rotate(this.gene[i].Rotation));
      }
    }

    void IPlacementWorker.VerboseLog(string message)
    {
      this.VerboseLog(message);
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