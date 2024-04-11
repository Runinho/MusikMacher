using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
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
        double skipTo = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * 0.24;
        mediaPlayer.Position = TimeSpan.FromSeconds(skipTo);
        Console.WriteLine("media opend");
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(Length));
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

    public double Position
    {
      get { return mediaPlayer.Position.TotalSeconds; }
      set
      {
        // seek forward
        Console.Write("need to seek to " + value);
        mediaPlayer.Position = TimeSpan.FromSeconds(value);
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
