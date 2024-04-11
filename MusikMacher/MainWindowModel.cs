using GalaSoft.MvvmLight.Command;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusikMacher
{
  class MainWindowModel : INotifyPropertyChanged
  {

    public MainWindowModel()
    {
      LoadDataCommand = new RelayCommand(LoadData);
      SpaceKeyPressedCommand = new RelayCommand(SpaceKeyPressed);
      Player = new PlayerModel();

      dataLocation = "C:/some/folder";


      db = new TrackContext();
      db.Database.OpenConnection();
      db.Database.EnsureCreated();

      Tracks = new ObservableCollection<Track>(db.Tracks);

      // Set up collection view and apply sorting
      TracksView = CollectionViewSource.GetDefaultView(Tracks);
      TracksView.SortDescriptions.Add(new SortDescription("creationTime", ListSortDirection.Descending));

      //LoadData();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ObservableCollection<Track> _tracks;
    public ObservableCollection<Track> Tracks
    {
      get { return _tracks; }
      set { _tracks = value; OnPropertyChanged(nameof(Tracks)); }
    }

    private ICollectionView _tracksView;
    public ICollectionView TracksView
    {
      get { return _tracksView; }
      set { _tracksView = value; OnPropertyChanged(nameof(TracksView)); }
    }
    public ICommand LoadDataCommand { get; private set; }
    public ICommand SpaceKeyPressedCommand { get; private set; }
    public PlayerModel Player { get; private set; }

    private string _dataLocation;
    public string dataLocation
    {
      get { return _dataLocation; }
      set
      {
        if (value != _dataLocation)
        {
          _dataLocation = value;
          OnPropertyChanged(nameof(dataLocation));
        }
      }
    }

    private string _loadingLog;
    public string loadingLog
    {
      get { return _loadingLog; }
      set
      {
        if (value != loadingLog)
        {
          _loadingLog = value;
          OnPropertyChanged(nameof(loadingLog));
        }
      }
    }
    private DispatcherTimer timer;
    private Nullable<Point> startPoint;
    private TrackContext db;

    private void logLoading(string s)
    {
      loadingLog += s + "\n";
    }

    private void LoadData()
    {
      loadingLog = "Trying to load data from " + dataLocation + "\n";
      string loadFrom = dataLocation;

      int existing = 0;
      int created = 0;

      // Handle loading data here
      // Check if the directory exists
      if (Directory.Exists(loadFrom))
      {
        // Clear tracks
        //Tracks.Clear();

        // Get all files in the directory
        string[] files = Directory.GetFiles(loadFrom);

        // Iterate over each file
        foreach (string filePath in files)
        {
          // Create a FileInfo object to get file information
          FileInfo fileInfo = new FileInfo(filePath);

          if (fileInfo.Extension == ".mp3")
          {
            // Found mp3 file.
            // Output file details
            Console.WriteLine($"File Name: {fileInfo.Name}");
            Console.WriteLine($"File Size: {fileInfo.Length} bytes");
            Console.WriteLine($"Creation Time: {fileInfo.CreationTime}");
            Console.WriteLine($"Last Access Time: {fileInfo.LastAccessTime}");
            Console.WriteLine($"Last Write Time: {fileInfo.LastWriteTime}");
            Console.WriteLine($"File attributes: {fileInfo.Attributes}");


            // check if the track exists
            bool songExists = db.Tracks.Any(track => track.name == fileInfo.Name);
            if (songExists)
            {
              // we do nothing
              // TODO: maybe check if the original location is still valid?
              // TODO: do some hashing of the content???
              existing += 1;
            }
            else
            {
              created += 1;
              db.Tracks.Add(new Track(fileInfo.Name, filePath, fileInfo.CreationTime, ""));
            }
          }
        }
        db.SaveChanges();
        logLoading($"Found {existing} known songs and created {created} new ones in database.");
      }
      else
      {
        logLoading("Failed to load from '" + loadFrom + "': Directory does not exists.");
      }
      logLoading("Done.");
      Tracks.Clear();
      foreach(Track track in db.Tracks)
      {
        Tracks.Add(track);
      }
    }

    public void TrackPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      if (e.OriginalSource is Decorator decorator)
      {
        if(decorator.Child is ContentPresenter presenter)
        {
          if(presenter.Content is Track)
          {
            startPoint = e.GetPosition(null);
            return;
          }
          else
          {
            //Console.WriteLine("Not Track.");
          }
        }
        else
        {
          //Console.WriteLine("Not presenter.");
        }
      } else
      {
        if(e.OriginalSource is System.Windows.Controls.TextBlock textBlock)
        {
          if (textBlock.DataContext is Track)
          {
            startPoint = e.GetPosition(null);
            Console.WriteLine("Drag from text block");
            return;
          }
        }
        else
        {
          Console.WriteLine("Not decorator.");
        }
      }
      startPoint = null;
    }

    public void TrackPreviewMouseMove(object sender, MouseEventArgs e)
    {

      if (e.LeftButton == MouseButtonState.Pressed && startPoint is Point valueOfStart)
      {
        Point mousePos = e.GetPosition(null);
        Vector diff = valueOfStart - mousePos;
        Console.WriteLine(diff);

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
          // Get the dragged ListBoxItem
          var listBox = sender as ListBox;
          // if (listBoxItem != null)
          //{
          // Initialize drag and drop operation
          Console.WriteLine($"Drag: {Player.currentTrack.path}");
          var toDrag = Player.currentTrack;
          DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { toDrag.path });
          DragDropEffects effect = DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Copy | DragDropEffects.Move);
          // pause track 
          if (effect == DragDropEffects.Copy || effect == DragDropEffects.Move)
          {
            Player.Pause();
            Console.WriteLine($"sucessfully dragged {toDrag.name}");
          }
          else
          {
            Console.WriteLine($"drag failed for {toDrag.name}");
          }
        }
      }
    }

    private void SpaceKeyPressed()
    {
      Player.PlayPause();
    }
  }
}
