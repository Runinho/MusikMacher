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

namespace MusikMacher
{
  internal class WaveFormPathRenderer
  {
    //similiar to NAudio.WaveFormRenderer but output a path instead of a bitmap
    public static System.Windows.Media.PathGeometry render(WaveStream waveStream, IPeakProvider peakProvider, WaveFormRendererSettings settings)
    {
      int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8);
      var samples = waveStream.Length / (bytesPerSample);
      var samplesPerPixel = (int)(samples / settings.Width);
      var stepSize = settings.PixelsPerPeak + settings.SpacerPixels;
      peakProvider.Init(waveStream.ToSampleProvider(), samplesPerPixel * stepSize);

      //if (settings.DecibelScale)
      //  peakProvider = new DecibelPeakProvider(peakProvider, 48);

      var midPoint = settings.TopHeight;

      // start
      PathFigure pathFigure = new PathFigure();
      pathFigure.IsClosed = true;
      pathFigure.StartPoint = new Point(0, 0);

      PathFigure pathFigureBottom = new PathFigure();
      pathFigureBottom.IsClosed = true;
      pathFigureBottom.StartPoint = new Point(0, 0);


      int x = 0;
      while (x < settings.Width)
      {
        var currentPeak = peakProvider.GetNextPeak();
        pathFigure.Segments.Add(new LineSegment(new Point(x, currentPeak.Max * 20), true));
        pathFigureBottom.Segments.Add(new LineSegment(new Point(x, currentPeak.Min * 20), true));
        x++;
      }
      // end thingy
      pathFigure.Segments.Add(new LineSegment(new Point(x, 0), true));
      pathFigure.Segments.Add(new LineSegment(new Point(0, 0), true));

      pathFigureBottom.Segments.Add(new LineSegment(new Point(x, 0), true));
      pathFigureBottom.Segments.Add(new LineSegment(new Point(0, 0), true));

      PathGeometry geometry = new PathGeometry();
      geometry.Figures.Add(pathFigure);
      geometry.Figures.Add(pathFigureBottom);

      return geometry;
    }

  }
}
