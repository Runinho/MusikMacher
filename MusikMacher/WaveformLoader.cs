using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusikMacher
{
  internal class SheduleLoad : LoaderWorker<string, Point[][]>
  {
    private static SheduleLoad Instance = null;

    public static SheduleLoad getInstance()
    {
      if (Instance == null)
      {
        Instance = new SheduleLoad();
      }
      return Instance;
    }

    public SheduleLoad() : base(false, true) {
    }

    internal override Point[][] Handle(string item)
    {
      return Track.LoadWaveformGeometry(item);
    }
  }
}
