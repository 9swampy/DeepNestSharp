namespace DeepNestLib.IO
{
  using System.Text.Json.Serialization;

  public abstract class Saveable : ISaveable
  {
    private string saveState;
    private bool forceIsDirty;

    [JsonIgnore]
    /// <inheritdoc />
    public bool IsDirty
    {
      get
      {
        return this.forceIsDirty ||
               this.ToJson(false) != saveState;
      }
    }

    public abstract string ToJson(bool writeIndented);

    /// <summary>
    /// Update the memoised save state which is used to determine <see cref="IsDirty"/>.
    /// </summary>
    public void SaveState()
    {
      saveState = this.ToJson(false);
    }

    /// <summary>
    /// Forces IsDirty to be true until next SaveState.
    /// </summary>
    public void SetIsDirty()
    {
      this.forceIsDirty = true;
    }
  }
}
