using LorusMusikMacher.database;
using System.Collections.Frozen;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusikMacher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    MainWindowModel model = MainWindowModel.Instance;

    public MainWindow()
    {
      InitializeComponent();
      DataContext = model;
      this.Closed += Window_Closed;

      Settings settings = Settings.getSettings();

      Left = settings.MainWindowLeft;
      Top = settings.MainWindowTop;
      Width = settings.MainWindowWidth;
      Height = settings.MainWindowHeight;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      // Save the loaction of the window
      Settings settings = Settings.getSettings();

      settings.MainWindowLeft = Left;
      settings.MainWindowTop = Top;
      settings.MainWindowWidth = Width;
      settings.MainWindowHeight = Height;
      Settings.saveSettings(); // maybee this is enough and we can remove all the other saves?
    }

    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      // can this also be done in the binding???
      model.TrackPreviewMouseLeftButtonDown(e, dataGrid.IsFocused);
    }

    private void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      model.TrackPreviewMouseMove(sender, e);
    }

    private void MenuItem_Rename(object sender, RoutedEventArgs e)
    {
      model.RenameTag();
    }

    private void Tag_DragEnter(object sender, DragEventArgs e)
    {
      // Check if the data being dragged is of the expected type
      if (e.Data.GetDataPresent("MusikMakerTrack"))
      {
        e.Effects = DragDropEffects.Copy; // Change cursor to indicate dropping is allowed
      }
      else
      {
        e.Effects = DragDropEffects.None; // Data format not supported, prevent dropping
      }
      e.Handled = true; // Mark event as handled
    }

    private void Tag_DragOver(object sender, DragEventArgs e)
    {
      // Allow dropping by setting the effect to Copy or Move based on your requirement
      e.Effects = DragDropEffects.Copy;
      e.Handled = true; // Mark event as handled
    }

    private void Tag_Drop(object sender, DragEventArgs e)
    {
      // Retrieve the dropped data and handle it accordingly
      if (e.Data.GetDataPresent("MusikMakerTrack"))
      {
        var droppedData = (string[])e.Data.GetData("MusikMakerTrack");
        System.Diagnostics.Debug.WriteLine($"got dropped data: {droppedData}");
        // Process dropped data here
        if (sender is System.Windows.Controls.CheckBox checkBox)
        {
          if (checkBox.DataContext is Tag tag)
          {
            foreach (string name in droppedData)
            {
              model.AddTrackToTag(name, tag);
            }
          }
        }
      }
      e.Handled = true; // Mark event as handled
      e.Effects |= DragDropEffects.Link;
    }

    private void DataGrid_Delete(object sender, RoutedEventArgs e)
    {
      model.DeleteTracks();
    }
    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var bindedList = model.Player.SelectedTracks;

      var items = dataGrid.SelectedItems;
      foreach (var item in items)
      {
        if (item is Track track)
        {
          // add if not in selected
          if (!bindedList.Contains(track))
          {
            //System.Diagnostics.Debug.WriteLine($"adding track from selection {track.name}");
            bindedList.Add(track);
          }
        }
      }
      // check for deselected.
      foreach (var track in bindedList.ToFrozenSet())
      {
        if (!items.Contains(track))
        {
          //System.Diagnostics.Debug.WriteLine($"removing track from selection {track.name}");
          bindedList.Remove(track);
        }
      }
    }
  }
}