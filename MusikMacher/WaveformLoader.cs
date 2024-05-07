using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusikMacher
{
  internal class WaveformLoader : LoaderWorker<string, Point[][]>
  {
    private static WaveformLoader Instance = null;

    public static WaveformLoader getInstance()
    {
      if (Instance == null)
      {
        Instance = new WaveformLoader();
      }
      return Instance;
    }

    public WaveformLoader() : this(1)
    {
    }

    public WaveformLoader(int workers) : base(false, true, workers) {
    }

    public WaveformLoader(bool empty, int workers) : base(false, empty, workers)
    {
    }

    internal override Point[][] Handle(string item)
    {
      return Track.LoadWaveformGeometry(item);
    }
  }
}
