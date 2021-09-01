namespace DeepNestSharp.Ui.Docking
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using AvalonDock;
  using AvalonDock.Layout;
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
      var l = GatherLayoutContent(dockManager.Layout).ToArray();
      // Remove the views by force
      foreach (var x in l)
      {
        dockManager.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.Name.Equals("RemoveViewFromLogicalChild"))
            .First()
            .Invoke(dockManager, new object[] { x });
      }

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

    private static IEnumerable<LayoutContent> GatherLayoutContent(ILayoutElement le)
    {
      if (le is LayoutContent)
      {
        yield return (LayoutContent)le;
      }

      IEnumerable<ILayoutElement> children = new ILayoutElement[0];
      if (le is LayoutRoot)
      {
        children = ((LayoutRoot)le).Children;
      }
      else if (le is LayoutPanel)
      {
        children = ((LayoutPanel)le).Children;
      }
      else if (le is LayoutDocumentPaneGroup)
      {
        children = ((LayoutDocumentPaneGroup)le).Children;
      }
      else if (le is LayoutAnchorablePane)
      {
        children = ((LayoutAnchorablePane)le).Children;
      }
      else if (le is LayoutDocumentPane)
      {
        children = ((LayoutDocumentPane)le).Children;
      }

      foreach (var child in children)
      {
        foreach (var x in GatherLayoutContent(child))
        {
          yield return x;
        }
      }
    }
  }
}
