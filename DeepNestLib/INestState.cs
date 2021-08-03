namespace DeepNestLib
{
  public interface INestState
  {
    long AverageNestTime { get; }

    int Generations { get; }

    bool IsErrored { get; }

    int Iterations { get; }

    long LastPlacementTime { get; }

    int NestCount { get; }

    int Population { get; }

    int Threads { get; }
  }
}