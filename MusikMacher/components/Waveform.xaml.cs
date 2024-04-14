using NAudio.Wave;
using NAudio.WaveFormRenderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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

namespace MusikMacher.components
{
  /// <summary>
  /// Interaction logic for Waveform.xaml
  /// </summary>
  public partial class Waveform : UserControl, INotifyPropertyChanged
  {
    public static readonly DependencyProperty SkipCommandProperty =
    DependencyProperty.Register(
        "SkipCommand",
        typeof(ICommand),
        typeof(Waveform),
        new UIPropertyMetadata(null));
    public ICommand SkipCommand
    {
      get { return (ICommand)GetValue(SkipCommandProperty); }
      set { SetValue(SkipCommandProperty, value); }
    }

    public static readonly DependencyProperty PositionProperty =
DependencyProperty.Register(
    "Position",
    typeof(double),
    typeof(Waveform),
    new UIPropertyMetadata((double)0, OnPositionPropertyChanged));

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((Waveform)d).OnPropertyChanged(nameof(MouseState));
    }

    public double Position
    {
      get { return (double)GetValue(PositionProperty); }
      set { 
        SetValue(PositionProperty, value);
      }
    }

    public static readonly DependencyProperty LengthProperty =
    DependencyProperty.Register(
    "Length",
    typeof(double),
    typeof(Waveform),
    new UIPropertyMetadata(null));
    public double Length
    {
      get { return (double)GetValue(LengthProperty); }
      set { 
        SetValue(LengthProperty, value);
      }
    }

    private double _mousePoition;
    public double MousePosition
    {
      get { return _mousePoition; }
      set
      {
        if( _mousePoition != value)
        {
          _mousePoition = value;
          OnPropertyChanged(nameof(MousePosition));
          OnPropertyChanged(nameof(MouseState));
        }
      }
    }

    private bool _isOver;
    public bool IsOver
    {
      get { return _isOver; }
      set
      {
        if (_isOver != value)
        {
          _isOver = value;
          OnPropertyChanged(nameof(IsOver));
          OnPropertyChanged(nameof(MouseState));
        }
      }
    }

    private int _mouseState; // 0 -> outside; 1-> left 2-> right
    public int MouseState
    {
      get
      {
        if (!IsOver)
        {
          return 0;
        }
        return (MousePosition <= (Position / Length)) ? 1 : 2;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Waveform()
    {
      InitializeComponent();
      Path.DataContext = this;
    }



    private void SetRelativeMouse(double relative)
    {
      MousePosition = relative;
    }

    private void MouseMoveHandler(object sender, MouseEventArgs e)
    {
      /// Get the x and y coordinates of the mouse pointer.
      System.Windows.Point position = e.GetPosition(this);
      double pX = position.X;
      double pY = position.Y;

      /// Sets eclipse to the mouse coordinates.
      var relative = ((double)(pX)) / ((double)LayoutRoot.ActualWidth);
      SetRelativeMouse(relative);

      if (e.LeftButton == MouseButtonState.Pressed)
      {
        // update position
        SkipCommand?.Execute(relative);
      }

      IsOver = true;
    }

    private void MouseDownHandler(object sender, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition(this);
      double pX = position.X;
      var relative = ((double)(pX)) / ((double)LayoutRoot.ActualWidth);
      // relative skip
      SkipCommand?.Execute(relative);
    }

    private void MouseLeaveHandler(object sender, MouseEventArgs e)
    {
      IsOver = false;
    }
  }
}
