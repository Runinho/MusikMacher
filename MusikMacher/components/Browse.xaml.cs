using LorusMusikMacher.database;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MusikMacher.components
{
  /// <summary>
  /// Interaction logic for browse.xaml
  /// </summary>
  public partial class Browse : UserControl
  {
    public BrowseViewModel model;
    public PlayerModel Player { get; private set; }
    public Browse()
    {
      InitializeComponent();

      this.PreviewKeyDown += Browse_PreviewKeyDown;

      // try to load last sorting direction
      //dataGrid.LoadedSortingDirection = settings.TracksSortingDescriptions;
    }

    private void Browse_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
      {
        // focus search box
        SoundSearchTextBox.Focus();
        SoundSearchTextBox.SelectAll();
      }

      if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
      {
        // focus search box
        TagSearchTextBox.Focus();
        TagSearchTextBox.SelectAll();
      }

      if (SoundSearchTextBox.IsFocused)
      {
        if (e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Up)
        {
          System.Diagnostics.Debug.WriteLine("focus data Grid because Arrow down");
          int index = dataGrid.SelectedIndex;
          if (index == -1)
          {
            index = 0;
          }
          dataGrid.ScrollIntoView(dataGrid.Items[index]);
          dataGrid.UpdateLayout();
          var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
          var direction = (e.Key == Key.Up ? FocusNavigationDirection.Up : FocusNavigationDirection.Down);
          if (row != null)
          {
            row.MoveFocus(new TraversalRequest(direction));
            e.Handled = true;
          }
        }
      }
    }

    private void DataGrid_OnClick(object sender, MouseButtonEventArgs e)
    {
      model.OnClick(e);
      base.OnMouseDown(e);
    }

    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      // can this also be done in the binding???
      //Logical focus
      var focusedControl = Keyboard.FocusedElement;
      model.TrackPreviewMouseLeftButtonDown(e, focusedControl);
    }
    private void DataGrid_TrackPreviewMouseLeftButtonDownAlwaysDrag(object sender, MouseButtonEventArgs e)
    {
      model.TrackPreviewMouseLeftButtonDownAlwaysDrag(e);
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
      model.HideTracks();
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

    private void listBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      model.ToggleCurrentTag();
    }

    private void DataGrid_OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
      model.TrackGiveFeedback();
    }
  }
}
