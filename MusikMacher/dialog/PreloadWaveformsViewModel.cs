using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;

namespace MusikMacher.dialog
{
  class PreloadWaveformsViewModel : ViewModelBase
  {
    private Queue<DateTime> EndTimes = new Queue<DateTime>(30);

    public PreloadWaveformsViewModel()
    {
      StartLoading();
    }

    ~PreloadWaveformsViewModel()
    {
      Console.WriteLine("closing preloading");
      Cancle();
    }

    public void Cancle()
    {
      Loader.Cancle();
    }
    
    private void StartLoading()
    {
      // create a new loader and just trigger all
      StartTime = DateTime.Now;
      EndTimes = new Queue<DateTime>(100);

      int workers = Environment.ProcessorCount;
      workers = workers / 2;
      if (workers < 0)
      {
        workers = 1;
      }
      Console.WriteLine($"using {workers} workers to load the waveforms");

      Loader = new WaveformLoader(false, 10);

      List<Track> allTracks = new List<Track>();
      NumberTracks = 0;

      string[] dbNames = ["tracks", "effects"];
      
      foreach (string dbName in dbNames)
      {
        // open another db instance and load the data
        var db = TrackContext.GetTrackContext(dbName);

        NumberTracks += db.Tracks.Count();

        allTracks.AddRange(db.Tracks);
      }

      foreach (var track in allTracks)
      {
        Loader.Shedule(new Tuple<string, Action<Point[][]>>(track.path,
          (Point[][] points) =>
          {
            LoadedTracks += 1;
            // update estimated time.
            // only use the last 100 samples.
            if (EndTimes.Count >= 100)
            {
              EndTimes.Dequeue();
            }

            if (EndTimes.Count > 0)
            {
              // estimate over the last 100 samples
              var timeUsed = DateTime.Now - EndTimes.Peek();
              EstimatedTimeLeft = (timeUsed.TotalSeconds / EndTimes.Count) * (NumberTracks - LoadedTracks);
              // save the current time
            }

            // don't add for first 3 seconds (cached tracks load much faster)
            if ((DateTime.Now - StartTime).TotalSeconds > 3)
            {
              EndTimes.Enqueue(DateTime.Now);
            }
          }));
      }
    }

    public WaveformLoader Loader { get; set; }

    public DateTime StartTime { get; set; }

    private double _estimatedTimeLeft;

    public double EstimatedTimeLeft
    {
      get => _estimatedTimeLeft;
      set
      {
        if (value != _estimatedTimeLeft)
        {
          _estimatedTimeLeft = value;
          RaisePropertyChanged(nameof(EstimatedTimeLeft));
        }
      }
    }

    private int _numberTracks;

    public int NumberTracks
    {
      get => _numberTracks;
      set
      {
        if (value != _numberTracks)
        {
          _numberTracks = value;
          RaisePropertyChanged(nameof(NumberTracks));
        }
      }
    }

    private int _loadedTracks;

    public int LoadedTracks
    {
      get => _loadedTracks;
      set
      {
        if (value != _loadedTracks)
        {
          _loadedTracks = value;
          RaisePropertyChanged(nameof(LoadedTracks));
        }
      }
    }
  }
}