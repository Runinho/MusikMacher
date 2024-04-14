using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MusikMacher.components
{
  // needed to patch drag behaviour
  // from https://stackoverflow.com/a/55485136
  // also use it to safe sorting order between restarts
  // https://stackoverflow.com/a/34947185
  public class MyDataGrid : DataGrid
  {
    private static readonly FieldInfo s_isDraggingSelectionField =
        typeof(DataGrid).GetField("_isDraggingSelection", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo s_endDraggingMethod =
        typeof(DataGrid).GetMethod("EndDragging", BindingFlags.Instance | BindingFlags.NonPublic);
    
    internal List<SortDescription>? LoadedSortingDirection = null;

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

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
      base.OnItemsSourceChanged(oldValue, newValue);

      if (LoadedSortingDirection != null)
      {
        SetSortDescription(LoadedSortingDirection);
        // set to null because we only do this on first load.
        LoadedSortingDirection = null;
      }
    }

      public void SetSortDescription(List<SortDescription> descriptions)
    {
      ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
      view.SortDescriptions.Clear();

      foreach (SortDescription sortDescription in descriptions)
      {
        view.SortDescriptions.Add(sortDescription);

        // I need to tell the column its SortDirection,
        // otherwise it doesn't draw the triangle adornment
        DataGridColumn column = Columns.FirstOrDefault(c => c.SortMemberPath == sortDescription.PropertyName);
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
