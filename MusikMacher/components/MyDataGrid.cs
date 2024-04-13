using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusikMacher.components
{
  // needed to patch drag behaviour
  // from https://stackoverflow.com/a/55485136
  public class MyDataGrid : DataGrid
  {
    private static readonly FieldInfo s_isDraggingSelectionField =
        typeof(DataGrid).GetField("_isDraggingSelection", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo s_endDraggingMethod =
        typeof(DataGrid).GetMethod("EndDragging", BindingFlags.Instance | BindingFlags.NonPublic);

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
  }
}
