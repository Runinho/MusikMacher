using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;

namespace MusikMacher
{
  // cache the waveforms
  public class WaveformCache
  {
    public static string path = "WaveformCache";
    public static int version = 48;// start with 47 so its not 0 and less likly to be for random files.

    private static string GetCacheFilename(string filename)
    {
      string nameOnly = Path.GetFileName(filename);
      var cacheFile = Path.Join(path, nameOnly + ".wavcache");
      return cacheFile;
    }

    public static Point[][]? FromCache(string filename)
    {
      var cacheFile = GetCacheFilename(filename);
      if (File.Exists(cacheFile))
      {
        using (BinaryReader reader = new BinaryReader(File.Open(cacheFile, FileMode.Open)))
        {
          bool checkForAllZeros = false;
          bool allZeros = true;
          // Read the number of arrays
          int magicVersion = reader.ReadInt32();
          if (magicVersion < 47)
          {
            // unkown version
            return null;
          } else if(magicVersion == 47)
          {
            // we had a bug where some mp3 got not decoded fully.
            // we check these old files if they are all zeors
            checkForAllZeros = true;
          } else if(magicVersion > version)
          {
            // file from newer version of programm?
            return null;
          }

          // check if all are zero

          // Read the number of arrays
          int arrayCount = reader.ReadInt32();

          // Create an array to store the arrays of points
          Point[][] pointsArray = new Point[arrayCount][];

          // Iterate through each array of points
          for (int i = 0; i < arrayCount; i++)
          {
            // Read the length of the current array
            int length = reader.ReadInt32();

            // Create an array to store the points
            Point[] points = new Point[length];

            // Read each point's X and Y coordinates
            for (int j = 0; j < length; j++)
            {
              double x = reader.ReadDouble();
              double y = reader.ReadDouble();
              points[j] = new Point(x, y);
              if (double.IsNaN(y))
              {
                // invalid data, dont'use cache.
                return null;
              }
              if(y != 0)
              {
                allZeros = false;
              }
            }

            pointsArray[i] = points;
          }

          if(checkForAllZeros && allZeros)
          {
            // ignore that file.
            return null;
          }

          return pointsArray;
        }
      }
      // not in cache
      return null;
    }

    public static void SaveInCache(string filename, Point[][] data)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      var cacheFile = GetCacheFilename(filename);

      using (BinaryWriter writer = new BinaryWriter(File.Open(cacheFile, FileMode.Create)))
      {
        // Write magic
        writer.Write(version);

        // Write the number of arrays
        writer.Write(data.Length);

        // Iterate through each array of points
        foreach (Point[] points in data)
        {
          // Write the length of the current array
          writer.Write(points.Length);

          // Write each point's X and Y coordinates
          foreach (Point point in points)
          {
            writer.Write(point.X);
            writer.Write(point.Y);
          }
        }
      }
    }
  }
}
