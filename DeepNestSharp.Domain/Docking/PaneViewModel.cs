namespace DeepNestSharp.Domain.Docking
{
  //using System.Windows.Media;
  using CommunityToolkit.Mvvm.ComponentModel;

  public class PaneViewModel : ObservableRecipient
  {
    private string title;
    private string contentId;
    private bool isSelected;

    public PaneViewModel()
    {
    }

    public string Title
    {
      get => this.title;
      set
      {
        if (this.title != value)
        {
          this.title = value;
          this.OnPropertyChanged(nameof(this.Title));
        }
      }
    }

    //public ImageSource IconSource { get; protected set; }

    public string ContentId
    {
      get => this.contentId;
      set
      {
        if (this.contentId != value)
        {
          this.contentId = value;
          this.OnPropertyChanged(nameof(this.ContentId));
        }
      }
    }

    public bool IsSelected
    {
      get => this.isSelected;
      set
      {
        if (this.isSelected != value)
        {
          this.isSelected = value;
          this.OnPropertyChanged(nameof(this.IsSelected));
        }
      }
    }
  }
}
