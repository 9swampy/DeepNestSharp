namespace DeepNestSharp.Ui.Docking
{
  //using System.Windows.Media;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

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
      get => title;
      set
      {
        if (title != value)
        {
          title = value;
          OnPropertyChanged(nameof(Title));
        }
      }
    }

    //public ImageSource IconSource { get; protected set; }

    public string ContentId
    {
      get => contentId;
      set
      {
        if (contentId != value)
        {
          contentId = value;
          OnPropertyChanged(nameof(ContentId));
        }
      }
    }

    public bool IsSelected
    {
      get => isSelected;
      set
      {
        if (isSelected != value)
        {
          isSelected = value;
          OnPropertyChanged(nameof(IsSelected));
        }
      }
    }
  }
}
