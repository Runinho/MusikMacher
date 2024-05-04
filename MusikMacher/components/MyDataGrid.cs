using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DataGrid = Wpf.Ui.Controls.DataGrid;

namespace MusikMacher.components
{
  // needed to patch drag behaviour
  // from https://stackoverflow.com/a/55485136
  // also use it to safe sorting order between restarts
  // https://stackoverflow.com/a/34947185
  public class MyDataGrid : Wpf.Ui.Controls.DataGrid
  {
    private static readonly FieldInfo s_isDraggingSelectionField =
        typeof(DataGrid).GetField("_isDraggingSelection", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo s_endDraggingMethod =
        typeof(DataGrid).GetMethod("EndDragging", BindingFlags.Instance | BindingFlags.NonPublic);
    
    public static readonly DependencyProperty TracksSortingDescriptionsProperty = DependencyProperty.Register(
      nameof(TracksSortingDescriptions), typeof(List<SortDescription>), typeof(MyDataGrid), new PropertyMetadata(default(List<SortDescription>)));

    public List<SortDescription>? TracksSortingDescriptions
    {
      get { return (List<SortDescription>?)GetValue(TracksSortingDescriptionsProperty); }
      set
      {
        Console.WriteLine("Sorting description changed");
        if (value != null)
        {
          Console.WriteLine("loaded sorting description");
          SetSortDescription(value); // update sorting to the loaded value
        }
        SetValue(TracksSortingDescriptionsProperty, value);
      }
    }
    
    // DataGrid.OnMouseMove() serves no other purpose than to execute click-drag-selection.
    // Bypass that, and stop 'is dragging selection' mode for DataGrid
    protected override void OnMouseMove(MouseEventArgs e)
    {
      // end dragging if cmd is pressed.
      if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
      {
        if ((bool)(s_isDraggingSelectionField?.GetValue(this) ?? false))
          s_endDraggingMethod.Invoke(this, new object[0]);
      } else
      {
        base.OnMouseMove(e);
      }
    }

    protected override void OnSorting(DataGridSortingEventArgs eventArgs)
    {
      base.OnSorting(eventArgs);
      // call set value directly, so we get no endless recursion;
      SetValue(TracksSortingDescriptionsProperty, GetSortDescriptions());
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
      base.OnItemsSourceChanged(oldValue, newValue);

      // only works if the sorting description is binded before the source!
      if (TracksSortingDescriptions != null)
      {
        SetSortDescription(TracksSortingDescriptions);
      }
    }

      public void SetSortDescription(List<SortDescription>? descriptions)
    {
      ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
      view.SortDescriptions.Clear();

      foreach (SortDescription sortDescription in descriptions)
      {
        view.SortDescriptions.Add(sortDescription);

        // I need to tell the column its SortDirection,
        // otherwise it doesn't draw the triangle adornment
        System.Windows.Controls.DataGridColumn column = Columns.FirstOrDefault(c => c.SortMemberPath == sortDescription.PropertyName);
        if (column != null)
          column.SortDirection = sortDescription.Direction;
      }
    }

    public List<SortDescription> GetSortDescriptions()
    {
      ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
      return new List<SortDescription>(view.SortDescriptions);
    }
  }
}
