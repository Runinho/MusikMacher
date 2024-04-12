using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusikMacher
{
  class MainWindowModel : ViewModelBase
  {

    public static MainWindowModel Instance = new MainWindowModel();

    public MainWindowModel()
    {
      LoadDataCommand = new RelayCommand(LoadData);
      SpaceKeyPressedCommand = new RelayCommand(SpaceKeyPressed);
      AddTagCommand = new RelayCommand(AddTag);
      Player = new PlayerModel();

      dataLocation = "C:/some/folder";


      db = new TrackContext();
      db.Database.OpenConnection();
      db.Database.EnsureCreated();

      Tracks = new ObservableCollection<Track>(db.Tracks.Include(t => t.Tags));
      ReloadTags();

      // Set up collection view and apply sorting
      _itemSourceList = new CollectionViewSource() { Source = Tracks };
      //TracksView.SortDescriptions.Add(new SortDescription("creationTime", ListSortDirection.Descending));
      //_itemSourceList.Filter += new FilterEventHandler(FilterTracks);
      TracksView  = _itemSourceList.View;
      TracksView.Filter = FilterTracks;
      //LoadData();
    }

    private bool FilterTracks(object obj)
    {
      Track track = obj as Track;
      if (track != null)
      {
        // filter at search
        if (Search.Length > 0)
        {
          if (!track.name.ToLower().Contains(Search.ToLower()))
          {
            return false;
          }
        }
        // check with tags

        int numTagChecked = 0;
        int matchedTags = 0;
        foreach (Tag tag in Tags)
        {
          if (tag.IsChecked)
          {
            numTagChecked++;
            if (track.Tags.Contains(tag))
            {
              // have a tag that is included
              matchedTags++;
            }
          }
        }
        if (AndTags)
        {
          // AND behaviour
          // only include if all are deselected
          return numTagChecked == matchedTags;
        }
        else
        {
          // OR behaviour (default)
          if (numTagChecked > 0)
          {
            return matchedTags > 0;
          }
          else
          {
            // no tags selected display all
            return true;
          }
        }
      }
      return false;
    }

    private string _search = "";
    public string Search
    {
      get { return _search; }
      set
      {
        if (value != Search)
        {
          _search = value;
          RaisePropertyChanged(nameof(Search));
          RefreshTracksView();
          //TracksView = _itemSourceList.View;
        }
      }
    }

    private ObservableCollection<Track> _tracks;
    public ObservableCollection<Track> Tracks
    {
      get { return _tracks; }
      set { _tracks = value; RaisePropertyChanged(nameof(Tracks)); }
    }

    public ObservableCollection<Tag> _tags;
    public ObservableCollection<Tag> Tags
    {
      get { return _tags; }
      set { _tags = value; RaisePropertyChanged(nameof(Tags)); }
    }

    private CollectionViewSource _itemSourceList;


    private Tag _selectedTag;
    public Tag SelectedTag
    {
      get { return _selectedTag; }
      set
      {
        if (value != _selectedTag)
        {
          _selectedTag = value;
          RaisePropertyChanged(nameof(SelectedTag));
        }
      }
    }

    private Tag _selectedTags;
    public Tag SelectedTags
    {
      get { return _selectedTags; }
      set
      {
        if (value != _selectedTags)
        {
          _selectedTags = value;
          RaisePropertyChanged(nameof(SelectedTags));
        }
      }
    }

    private ICollectionView _tracksView;
    public ICollectionView TracksView
    {
      get { return _tracksView; }
      set { _tracksView = value; RaisePropertyChanged(nameof(TracksView)); }
    }

    public int TrackCount
    {
      get { return TracksView.Cast<object>().Count(); }
    }

    public ICommand LoadDataCommand { get; private set; }
    public ICommand SpaceKeyPressedCommand { get; private set; }
    public ICommand AddTagCommand { get; private set; }
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
          RaisePropertyChanged(nameof(dataLocation));
        }
      }
    }

    private bool _importSubfolders = false;
    public bool ImportSubfolders
    {
      get { return _importSubfolders; }
      set
      {
        if (value != _importSubfolders)
        {
          _importSubfolders = value;
          RaisePropertyChanged(nameof(ImportSubfolders));
        }
      }
    }

    private bool _andTags = false;
    public bool AndTags
    {
      get { return _andTags; }
      set
      {
        if (value != _andTags)
        {
          _andTags = value;
          RaisePropertyChanged(nameof(AndTags));
          RefreshTracksView();
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
          RaisePropertyChanged(nameof(loadingLog));
        }
      }
    }
    private DispatcherTimer timer;
    private Nullable<Point> startPoint;
    public TrackContext db;
    private FrozenSet<Track> selected;

    private void logLoading(string s)
    {
      loadingLog += s + "\n";
    }

    private void LoadData()
    {
      loadingLog = "Trying to load data from " + dataLocation + "\n";
      LoadData([], dataLocation);
      Tracks.Clear();
      foreach (Track track in db.Tracks)
      {
        Tracks.Add(track);
      }
      ReloadTags();
    }

    private void LoadData(List<string> parents, string location)
    {
      loadingLog += "loading from sublocation " + location + "\n";
      string loadFrom = location;

      int existing = 0;
      int created = 0;

      Tag? tag = null;
      if (parents.Count > 0)
      {
        tag = db.CreateOrFindTag(parents[parents.Count - 1]);
      }

      // Handle loading data here
      // Check if the directory exists
      if (Directory.Exists(loadFrom))
      {

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
            Track? songExists = db.Tracks.Find(fileInfo.Name);
            if (songExists != null)
            {
              // we do nothing
              // TODO: maybe check if the original location is still valid?
              // TODO: do some hashing of the content???
              if(tag != null)
              {
                songExists.AddTag(tag);
              }
              existing += 1;
            }
            else
            {
              created += 1;
              var track = new Track(fileInfo.Name, filePath, fileInfo.LastWriteTime);
              if(tag != null)
              {
                track.AddTag(tag);
              }
              db.Tracks.Add(track);
            }
          }
        }

        if (ImportSubfolders)
        {
          // iter over directories
          string[] directories = Directory.GetDirectories(loadFrom);
          foreach (string dirPath in directories)
          {
            string dirName = Path.GetFileName(dirPath);
            if (parents.Count < 10) // max 10 dirs deep
            {
              List<string> newParents = parents.GetRange(0, parents.Count);
              newParents.Add(dirName);
              LoadData(newParents, dirPath);
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
    }

    public void TrackPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
      {
        if (e.OriginalSource is Decorator decorator)
        {
          if (decorator.Child is ContentPresenter presenter)
          {
            if (presenter.Content is Track selected)
            {
              if (Player.SelectedTracks.Contains(selected))
              {
                e.Handled = true;
              }
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
        }
        else
        {
          if (e.OriginalSource is System.Windows.Controls.TextBlock textBlock)
          {
            if (textBlock.DataContext is Track selected)
            {
              if (Player.SelectedTracks.Contains(selected))
              {
                e.Handled = true;
              }
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
          var dataGrid = sender as DataGrid;
          // if (listBoxItem != null)
          //{
          // Initialize drag and drop operation
          Console.WriteLine($"Drag: {Player.currentTrack.path}");
          var toDrag = Player.currentTrack;
          DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { toDrag.path });
          // put all the selected strings into it.
          var selected = Player.SelectedTracks.ToFrozenSet();
          var strings = new string[selected.Count];
          int i = 0;
          foreach ( var item in selected )
          {
            strings[i] = item.name;
            i++;
          }
          dragData.SetData("MusikMakerTrack", strings); // use the full selection.
          dragData.SetData(DataFormats.Text, toDrag.name);
          DragDropEffects effect = DragDrop.DoDragDrop(dataGrid, dragData, DragDropEffects.Copy);
          // pause track 
          if (effect == DragDropEffects.Link)
          {
            System.Diagnostics.Debug.WriteLine("Got linked so not stopping lol");
          }
          else  if (effect == DragDropEffects.Copy)
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

    private void AddTag()
    {
      var tagName = openTagDialog("tag name:");
      if (tagName != null)
      {
        System.Diagnostics.Debug.WriteLine($"creating new tag: '{tagName}'");
        Tag tag = new Tag();
        tag.Name = tagName;
        db.Tags.Add(tag);
        db.SaveChanges();
        ReloadTags();
      }
    }

    public string? openTagDialog(string question){
      // TODO remove need for dialog and just add an empty that can be renamed.
      // create dialog box
      var viewModel = new NewTagDialogViewModel(question);

      var dialog = new NewTagDialog
      {
        DataContext = viewModel
      };
      string? result = null;
      viewModel.createTag = (tagName) =>
      {
        result = tagName;
        dialog.Close();
      };
      dialog.ShowDialog();
      return result;
    }


    internal void RenameTag()
    {
      var toRename = SelectedTag;
      var newName = openTagDialog($"rename '{toRename.Name}' to:");
      if(newName != null)
      {
        toRename.Name = newName;
        db.SaveChanges();
        
      }
    }

    private void ReloadTags()
    {
      // just overwrite LOL
      Tags = new ObservableCollection<Tag>(db.Tags);
      foreach(var tag in Tags)
      {
        tag.PropertyChanged += Tag_PropertyChanged;
      }
    }

    private void Tag_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine(e.PropertyName);
      switch (e.PropertyName)
      {
        case "IsChecked":
          System.Diagnostics.Debug.WriteLine("Update triggered");
          RefreshTracksView();
          break;
      }
    }

    internal void AddTrackToTag(string trackName, Tag tag)
    {
      // find the track
      Track? track = db.Tracks.Find(trackName);
      if(track != null)
      {
        if (!track.Tags.Contains(tag))
        {
          // need to add it
          track.AddTag(tag);
          db.SaveChanges();
          System.Diagnostics.Debug.WriteLine($"added tag '{tag.Name}' to track {trackName}");
        }
      } else
      {
        Console.WriteLine($"Could not find track with name '{trackName}' to add tag '{tag.Name}' to it!");
      }
    }

    internal void DeleteTracks()
    {
      // delete current selection
      // TODO: maybee add a question dialog LOL
      selected = Player.SelectedTracks.ToFrozenSet();
      foreach(Track track in selected)
      {
        // delete in db
        db.Tracks.Remove(track);
        // delete in tracks
        Tracks.Remove(track);
      }
      RefreshTracksView();
    }

    private void RefreshTracksView()
    {
      TracksView.Refresh();
      RaisePropertyChanged(nameof(TrackCount));
    }
  }
}
