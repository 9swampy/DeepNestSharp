namespace DeepNestLib.IO
{
  public interface ISaveable
  {
    /// <summary>
    /// Gets a value indicating whether the state has changed since last SaveState.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Update the memoised save state which is used to determine <see cref="IsDirty"/>.
    /// </summary>
    void SaveState();
  }
}