namespace DeepNestSharp.Domain.Models
{
  using DeepNestLib;
  
  public class ObservableHole : ObservableNfp
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableHole"/> class.
    /// </summary>
    /// <param name="child">The hole to wrap.</param>
    public ObservableHole(INfp child)
      : base(child)
    {
    }
  }
}