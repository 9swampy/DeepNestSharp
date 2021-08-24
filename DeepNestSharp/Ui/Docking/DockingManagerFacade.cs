namespace DeepNestSharp.Ui.Docking
{
  using System.IO;
  using AvalonDock;
  using AvalonDock.Layout.Serialization;
  using DeepNestSharp.Domain.Docking;

  public class DockingManagerFacade : IDockingManagerFacade
  {
    private readonly DockingManager dockManager;

    public DockingManagerFacade(DockingManager dockManager)
    {
      this.dockManager = dockManager;
    }

    public void LoadLayout()
    {
      // Walk down the layout and gather the LayoutContent elements.
      // AD bails out when it tries to invoke RemoveViewFromLogicalChild
      // on them.
      /*var l = GatherLayoutContent(DockManager.Layout).ToArray();
      // Remove the views by force
      foreach (var x in l)
      {
        DockManager.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.Name.Equals("RemoveViewFromLogicalChild"))
            .First()
            .Invoke(DockManager, new object[] { x });
      }*/

      var layoutSerializer = new XmlLayoutSerializer(this.dockManager);
      /*layoutSerializer.LayoutSerializationCallback += (s, e) =>
      //{
      //  object o = e.Content;
      };*/

      var configFile = new FileInfo(@".\AvalonDock.Layout.config");
      if (configFile.Exists)
      {
        layoutSerializer.Deserialize(configFile.FullName);
      }
    }

    public void SaveLayout()
    {
      var layoutSerializer = new XmlLayoutSerializer(this.dockManager);
      layoutSerializer.Serialize(@".\AvalonDock.Layout.config");
    }
  }
}
