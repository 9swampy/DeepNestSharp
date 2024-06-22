namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;

  /// <summary>
  /// Interaction logic for PartPlacements.xaml.
  /// </summary>
  public partial class PartPlacementsList : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
                                                        "ItemsSource",
                                                        typeof(ObservableCollection<ObservablePartPlacement>),
                                                        typeof(PartPlacementsList),
                                                        new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.RegisterAttached(
                                                        "SelectedIndex",
                                                        typeof(int),
                                                        typeof(PartPlacementsList),
                                                        new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
                                                        "SelectedItem",
                                                        typeof(IPartPlacement),
                                                        typeof(PartPlacementsList),
                                                        new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });
    private HorizontalScrollHandler horizontalScrollHandler;

    public PartPlacementsList()
    {
      horizontalScrollHandler = new HorizontalScrollHandler();
      this.InitializeComponent();
    }

    public int SelectedIndex
    {
      get => (int)this.GetValue(SelectedIndexProperty);
      set
      {
        this.SetValue(SelectedIndexProperty, value);
        System.Diagnostics.Debug.Print($"Set SelectedIndex to {value}");
      }
    }

    public IPartPlacement SelectedItem
    {
      get => (IPartPlacement)this.GetValue(SelectedItemProperty);
      set => this.SetValue(SelectedItemProperty, value);
    }

    public IReadOnlyList<IPartPlacement> ItemsSource
    {
      get => (ObservableCollection<ObservablePartPlacement>)this.GetValue(ItemsSourceProperty);
      set => this.SetValue(ItemsSourceProperty, value);
    }

    private void HandleHorizontalScroll(object sender, MouseWheelEventArgs e)
    {
      horizontalScrollHandler.ListView_PreviewMouseWheel(sender, e);
    }
  }
}
