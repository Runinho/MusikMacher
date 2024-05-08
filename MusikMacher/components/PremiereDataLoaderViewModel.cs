using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MusikMacher.converter;
using Wpf.Ui.Controls;


namespace MusikMacher.components
{
  public class PremiereDataLoaderViewModel : ViewModelBase
  {
    public PremiereDataLoaderViewModel()
    {
      LoadPremiereDataCommand = new RelayCommand(LoadPremiereData);
      GenerateCommand = new RelayCommand(Generate);


      Settings settings = Settings.getSettings();
      PremiereFilePath = settings.PremiereFilePath;

      Clips = new ObservableCollection<Clip>();
      _clipsSourceList = new CollectionViewSource() { Source = Clips };
      ClipsView = _clipsSourceList.View;
      ClipsView.Filter = FilterClips;
    }

    private void Generate()
    {
      // generate the windows tmestamps
      Log("======== TIMESTAMPS ========");
      foreach(Clip clip in ClipsView)
      {
        if (clip.Include)
        {
          SecondsToTimeStringConverter converter = new SecondsToTimeStringConverter();
          // TODO: configure output
          

          Log($"[{converter.Convert(clip.InTime, null, null, null)} - {converter.Convert(clip.OutTime, null, null, null)}] {clip.NameOnly()}");
        }
      }
    }

    private bool FilterClips(object obj)
    {
      if (FilterTrack1)
      {
        // only track index 1 included
        if(obj is Clip clip)
        {
          // always include the selected ones
          if (clip.Include)
          {
            return true; 
          }

          // only include if track index is 1
          return clip.TrackIndex == 1;
        }
      }
      return true;
    }

    private void Log(string message)
    {
      Console.WriteLine($"Premiere log: {message}");
      PremiereLog += message + "\n";
    }
    
    private void LoadPremiereData()
    {
      PremiereLog = $"starting data load from {PremiereFilePath}\n";
      Task.Run(()=>{
        var clips = PremiereFileReader.ReadFile(Log, PremiereFilePath);
        _ = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
          Clips.Clear();
          // add all
          foreach (var clip in clips)
          {
            Clips.Add(clip);
            clip.PropertyChanged += Clip_PropertyChanged;
          }
        }));
      });
      Settings.getSettings().PremiereFilePath = PremiereFilePath;
      Settings.saveSettings();
    }

    private void Clip_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "Include":
          System.Diagnostics.Debug.WriteLine("Update triggered");
          ClipsView.Refresh();
          RaisePropertyChanged(nameof(Clips));
          RaisePropertyChanged(nameof(ClipsView));
          break;
      }
    }

    public ICommand LoadPremiereDataCommand { get; set; }
    public RelayCommand GenerateCommand
    {
      get;
      private set;
    }

    private string _premiereLog = "";
    public string PremiereLog
    {
      get => _premiereLog;
      set
      {
        if (value != _premiereLog)
        {
          _premiereLog = value;
          RaisePropertyChanged(nameof(PremiereLog));
        }
      }
    }

    private string _premiereFilePath;
    public string PremiereFilePath
    {
      get => _premiereFilePath; 
      set
      {
        if (value != _premiereFilePath)
        {
          _premiereFilePath = value;
          RaisePropertyChanged(nameof(PremiereFilePath));
        }
      }
    }

    private ObservableCollection<Clip> _clips = new();
    public ObservableCollection<Clip> Clips
    {
      get => _clips;
      set
      {
        if (value != _clips)
        {
          _clips = value;
          RaisePropertyChanged(nameof(Clips));
        }
      }
    }

    private CollectionViewSource _clipsSourceList;
    private ICollectionView _clipsView;
    public ICollectionView ClipsView
    {
      get => _clipsView;
      set
      {
        _clipsView = value;
        RaisePropertyChanged(nameof(ClipsView));
      }
    }



    private bool _filterTrack1 = false;
    public bool FilterTrack1
    {
      get => _filterTrack1;
      set
      {
        if (value != _filterTrack1)
        {
          _filterTrack1 = value;
          RaisePropertyChanged(nameof(FilterTrack1));
          // force filter to be recalculated
          if(ClipsView !=  null)
          {
            ClipsView.Refresh();
            RaisePropertyChanged(nameof(ClipsView));
            RaisePropertyChanged(nameof(Clips));
          }
        }
      }
    }
  }
}