namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;

  public interface IPlacementWorker
  {
    void AddPlacement(INfp processingPart, List<IPartPlacement> placements, INfp part, PartPlacement position, PlacementTypeEnum placementType, ISheet sheet, double mergedLength);

    void VerboseLog(string message);
  }
}