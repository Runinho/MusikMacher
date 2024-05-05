using System.Collections.Frozen;
using System.Windows;
using GalaSoft.MvvmLight;

namespace MusikMacher.components;

public partial class DragWindow : Window
{
  public DragWindow(FrozenSet<Track> tracks)
  {
    InitializeComponent();
    DataContext = new DragWindowViewModel(tracks);
  }
}

public class DragWindowViewModel: ViewModelBase
{
  public DragWindowViewModel(FrozenSet<Track> tracks)
  {
    _tracks = tracks;
  }

  private FrozenSet<Track> _tracks;

  public FrozenSet<Track> Tracks
  {
    get => _tracks;
    set
    {
      if (value != _tracks)
      {
        _tracks = value;
        RaisePropertyChanged(nameof(Tracks));
      }
    }
  }

}