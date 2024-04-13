﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusikMacher
{
  // adapted from https://learn.microsoft.com/en-us/archive/msdn-technet-forums/5fa7cbc2-c99f-4b71-b46c-f156bdf0a75a
  public class TimeSlider : Slider
  {
    private Thumb _thumb = null;

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if (_thumb != null)
      {
        _thumb.MouseEnter -= thumb_MouseEnter;
      }

      _thumb = (GetTemplateChild("PART_Track") as System.Windows.Controls.Primitives.Track).Thumb;
      if (_thumb != null)
      {
        _thumb.MouseEnter += thumb_MouseEnter;
      }
    }

    private void thumb_MouseEnter(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        // the left button is pressed on mouse enter
        // so the thumb must have been moved under the mouse
        // in response to a click on the track.
        // Generate a MouseLeftButtonDown event.
        MouseButtonEventArgs args = new MouseButtonEventArgs(
            e.MouseDevice, e.Timestamp, MouseButton.Left);
        args.RoutedEvent = MouseLeftButtonDownEvent;
        (sender as Thumb).RaiseEvent(args);
      }
    }
  }
}
