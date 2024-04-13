﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MusikMacher
{
  public class PlayerModel: INotifyPropertyChanged
  {
    private MediaPlayer mediaPlayer = new MediaPlayer();

    public ICommand PlayCommand { get; private set; }
    public ICommand PauseCommand { get; private set; }

    private DispatcherTimer timer;

    public PlayerModel()
    {
      PlayCommand = new RelayCommand(Play);
      PauseCommand = new RelayCommand(Pause);

      // start timer for the playbar
      timer = new DispatcherTimer();
      timer.Interval = TimeSpan.FromSeconds(0.1);
      timer.Tick += Timer_Tick;

      mediaPlayer.MediaOpened += MediaOpend;
    }

    private void MediaOpend(object sender, EventArgs e)
    {
      // skip forward
      if (mediaPlayer.NaturalDuration.HasTimeSpan)
      {
        double full_length = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        double skipTo = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * 0.24;
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
    private bool isPlaying;

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
          }
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

    public void Play()
    {
      mediaPlayer.Play();
      isPlaying = true;
    }
    public void Pause()
    {
      mediaPlayer.Pause();
      isPlaying = false;
    }

    internal void PlayPause()
    {
      Console.WriteLine("play pause triggerd");
      if (isPlaying)
      {
        Pause();
      }
      else
      {
        Play();
      }
    }
  }
}
