namespace DeepNestSharp.Ui.Services
{
  using DeepNestSharp.Domain;
  using DeepNestSharp.Domain.Services;

  public class MouseCursorService : IMouseCursorService
  {
    public Cursors? OverrideCursor
    {
      set
      {
        if (value == null)
        {
          System.Windows.Input.Mouse.OverrideCursor = null;
        }
        else
        {
          switch (value)
          {
            case Cursors.Wait:
              System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
              break;
            case Cursors.AppStarting:
              System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.AppStarting;
              break;
            case Cursors.Arrow:
              System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
              break;
            default:
              System.Windows.Input.Mouse.OverrideCursor = null;
              break;
          }
        }
      }
    }
  }
}