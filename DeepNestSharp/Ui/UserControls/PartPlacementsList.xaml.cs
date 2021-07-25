namespace DeepNestSharp.Ui.UserControls
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Windows;
  using System.Windows.Controls;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Models;

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

    public PartPlacementsList() => InitializeComponent();

    public int SelectedIndex
    {
      get => (int)GetValue(SelectedIndexProperty);
      set
      {
        SetValue(SelectedIndexProperty, value);
        System.Diagnostics.Debug.Print($"Set SelectedIndex to {value}");
      }
    }

    public IPartPlacement SelectedItem
    {
      get => (IPartPlacement)GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
    }

    public IReadOnlyList<IPartPlacement> ItemsSource
    {
      get => (ObservableCollection<ObservablePartPlacement>)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }
  }
}
