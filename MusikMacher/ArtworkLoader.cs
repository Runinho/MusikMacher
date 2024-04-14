using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusikMacher
{
  internal class ArtworkLoader : LoaderWorker<string, byte[]?>
  {
    private static ArtworkLoader Instance = null;

    public static ArtworkLoader getInstance()
    {
      if (Instance == null)
      {
        Instance = new ArtworkLoader();
      }
      return Instance;
    }

    public ArtworkLoader(): base(true, false)
    {

    }

    internal override byte[]? Handle(string path)
    {
      return Track.LoadArtworkData(path);
    }
  }
}
