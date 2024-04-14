﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace MusikMacher
{
  public class PlayerModel: INotifyPropertyChanged
  {

    private static PathGeometry loadingGeometry = WaveFormPathRenderer.PointsToPathGeometry(
      [[new Point(0, 0), new Point(0, 10), new Point(0, 1), new Point(100,1), new Point(100, 0), new Point(0, 0)],
       [new Point(0, 0), new Point(0, -10), new Point(0, -1), new Point(100, -1), new Point(100, -0), new Point(0, 0)]]);

    private MediaPlayer mediaPlayer = new MediaPlayer();

    public ICommand PlayCommand { get; private set; }
    public ICommand PauseCommand { get; private set; }
    public ICommand PlayPauseCommand { get; private set; }
    public ICommand SkipForwardCommand { get; private set; }
    public ICommand SkipBackwardCommand { get; private set; }
    public ICommand SkipCommand { get; private set; }

    private DispatcherTimer timer;

    public PlayerModel()
    {
      PlayCommand = new RelayCommand(Play);
      PauseCommand = new RelayCommand(Pause);
      PlayPauseCommand = new RelayCommand(PlayPause);
      SkipForwardCommand = new RelayCommand(SkipForward);
      SkipBackwardCommand = new RelayCommand(SkipBackward);
      SkipCommand = new RelayCommand<double>(Skip);

      // start timer for the playbar
      timer = new DispatcherTimer();
      timer.Interval = TimeSpan.FromSeconds(0.03);//30fps
      timer.Tick += Timer_Tick;

      mediaPlayer.MediaOpened += MediaOpend;

      Volume = Settings.getSettings().Volume;
      SkipPosition = Settings.getSettings().SkipPosition;
    }

    private void MediaOpend(object sender, EventArgs e)
    {
      // skip forward
      if (mediaPlayer.NaturalDuration.HasTimeSpan)
      {
        double full_length = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        double skipTo = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * SkipPosition;
        mediaPlayer.Position = TimeSpan.FromSeconds(skipTo);
        Console.WriteLine("media opend");
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(Length));
        // check if we can save the duration
        if(currentTrack != null)
        {
          currentTrack.length = (int)full_length;
        }
        MainWindowModel.Instance.db.SaveChanges();
      }
      else
      {
        Console.WriteLine("WARNING: Failed to skip forward?");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    private void Timer_Tick(object sender, EventArgs e)
    {
      var tmp_seek_request = seek_request;
      if(tmp_seek_request != null)
      {
        seek_request = null; //TODO: maybe we need a semaphore to protect seek_request
        // we seek
        mediaPlayer.Position = TimeSpan.FromSeconds((double)tmp_seek_request);
      }

      //Console.WriteLine("got tick lol.");
      OnPropertyChanged(nameof(Position));
      OnPropertyChanged(nameof(Length));
    }

    private Track _currentTrack;
    private bool _isPlaying;

    public bool IsPlaying
    {
      get { return _isPlaying; }
      set
      {
        if (value != _isPlaying)
        {
          _isPlaying = value;
          OnPropertyChanged(nameof(IsPlaying));
        }
      }
    }

    public Track currentTrack
    {
      get { return _currentTrack; }
      set
      {
        if (value != _currentTrack)
        {
          _currentTrack = value;
          OnPropertyChanged(nameof(currentTrack));

          if(currentTrack != null)
          {
            // start playing that track lol
            mediaPlayer.Open(new Uri(_currentTrack.path));
            // skip to 1/3 of the song
            Play();
            timer.Start();

            // set wafeform to placeholder.
            if(WaveformGeometry != loadingGeometry)
            {
              WaveformGeometry = loadingGeometry;
            }

            // load in waveform in other thread
            SheduleLoad.getInstance().Shedule(new Tuple<string, Action<Point[][]>>(_currentTrack.path,
                  (Point[][] points) =>
                  {
                    System.Diagnostics.Debug.WriteLine($"loaded wafeform is same track??: {_currentTrack == value}");
                    // Code to run on the GUI thread.
                    if (_currentTrack == value)
                    {
                      var geometry = WaveFormPathRenderer.PointsToPathGeometry(points);
                      WaveformGeometry = geometry;
                    }
                  }));
          } else
          {
            Pause();
          }
        }
      }
    }

    public double Volume
    {
      get {
        if (mediaPlayer != null)
        {
          return mediaPlayer.Volume;
        }
        return 0.5;
       }
      set
      {
        if (mediaPlayer != null && mediaPlayer.Volume != value)
        {
          mediaPlayer.Volume = value;
          OnPropertyChanged(nameof(Volume));

          // save
          Settings.getSettings().Volume = value;
        }
      }
    }

    private double _skipPosition;
    public double SkipPosition
    {
      get { return _skipPosition; }
      set
      {
        if (value != _skipPosition)
        {
          _skipPosition = value;
          OnPropertyChanged(nameof(SkipPosition));

          // saving location in settings
          Settings.getSettings().SkipPosition = value;
          Settings.saveSettings();
        }
      }
    }

    private ObservableCollection<Track> _selectedTracks = new ObservableCollection<Track>();

    public ObservableCollection<Track> SelectedTracks
    {
      get { return _selectedTracks; }
      set
      {
        _selectedTracks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTracks)));
      }
    }

    private double? seek_request = null; //seek request is done by the timer. so we
    public double Position
    {
      get {
        var tmp_seek_request = seek_request; // copy to prevent race condition
        // return requested if it is not fullfiled already.
        if(tmp_seek_request != null)
        {
          return (double)tmp_seek_request;
        }
        return mediaPlayer.Position.TotalSeconds; 
      }
      set
      {
        // seek forward
        Console.Write("need to seek to " + value);
        seek_request = value;
        OnPropertyChanged(nameof(Position));
      }
    }

    public double Length
    {
      get {
        if (mediaPlayer.NaturalDuration.HasTimeSpan)
        {
          return mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }
        Console.WriteLine("no media open");
        return 1; 
      }
    }

    private BitmapImage? _wafeform;
    public BitmapImage? Waveform
    {
      get { return _wafeform; }
      set
      {
        if (value != _wafeform)
        {
          _wafeform = value;
          OnPropertyChanged(nameof(Waveform));
        }
      }
    }

    private PathGeometry? _waveformGeometry;
    public PathGeometry? WaveformGeometry
    {
      get { return _waveformGeometry; }
      set
      {
        if (value != _waveformGeometry)
        {
          _waveformGeometry = value;
          OnPropertyChanged(nameof(WaveformGeometry));
        }
      }
    }

    public void Play()
    {
      mediaPlayer.Play();
      IsPlaying = true;
    }
    public void Pause()
    {
      mediaPlayer.Pause();
      IsPlaying = false;
    }

    internal void PlayPause()
    {
      Console.WriteLine("play pause triggerd");
      if (IsPlaying)
      {
        Pause();
      }
      else
      {
        Play();
      }
    }

    internal void SkipForward()
    {
      double factor = 0.1;
      Position += Length * factor;
    }

    internal void SkipBackward()
    {
      double factor = 0.1;
      Position -= Length * factor;
    }

    internal void Skip(double relative)
    {
      Position = Length * relative;
    }
  }
}
