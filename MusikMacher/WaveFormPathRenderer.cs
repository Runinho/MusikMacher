using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using System.Drawing;
using System.Windows.Media;
using Point = System.Windows.Point;
using System.Windows.Ink;

namespace MusikMacher
{
  internal class WaveFormPathRenderer
  {
    public static Point[][] LoadPoints(WaveStream waveStream, IPeakProvider peakProvider, WaveFormRendererSettings settings)
    {
      Point[] points = new Point[settings.Width + 2];
      Point[] pointsBottom = new Point[settings.Width + 2];

      int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8);
      var samples = waveStream.Length / (bytesPerSample);
      var samplesPerPixel = (int)(samples / settings.Width);
      var stepSize = settings.PixelsPerPeak + settings.SpacerPixels;
      peakProvider.Init(waveStream.ToSampleProvider(), samplesPerPixel * stepSize);

      int x = 0;
      while (x < settings.Width)
      {
        var currentPeak = peakProvider.GetNextPeak();
        points[x] = new Point(x, currentPeak.Max * 20);
        pointsBottom[x] = new Point(x, currentPeak.Min * 20);
        x++;
      }
      points[x] = new Point(x, 0);
      points[x+1] = new Point(x, 0);

      pointsBottom[x] = new Point(x, 0);
      pointsBottom[x + 1] = new Point(x, 0);

      return [points, pointsBottom];
    }

    //similiar to NAudio.WaveFormRenderer but output a path instead of a bitmap
    public static System.Windows.Media.PathGeometry PointsToPathGeometry(Point[][] points)
    {

      // start
      PathFigure pathFigure = new PathFigure();
      pathFigure.IsClosed = true;
      pathFigure.StartPoint = new Point(0, 0);

      PathFigure pathFigureBottom = new PathFigure();
      pathFigureBottom.IsClosed = true;
      pathFigureBottom.StartPoint = new Point(0, 0);

      foreach(var point in points[0])
      {
        pathFigure.Segments.Add(new LineSegment(point, true));
      }

      foreach (var point in points[1])
      {
        pathFigureBottom.Segments.Add(new LineSegment(point, true));
      }

      PathGeometry geometry = new PathGeometry();
      geometry.Figures.Add(pathFigure);
      geometry.Figures.Add(pathFigureBottom);

      return geometry;
    }

  }
}
