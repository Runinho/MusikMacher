using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using NAudio.Gui;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Point = System.Windows.Point;
using Tag = LorusMusikMacher.database.Tag;
using Vector = System.Windows.Vector;

namespace MusikMacher.components
{
  public class BrowseViewModel : ViewModelBase
  {
    public BrowseSettings settings;
    public TrackContext db;
    private Nullable<Point> startPoint;
    private FrozenSet<Track> selected;

    public PlayerModel Player { get; private set; }
    public ICommand SpaceKeyPressedCommand { get; private set; }
    public ICommand AddTagCommand { get; private set; }
    public ICommand ClearTagsCommand { get; private set; }
    public ICommand ClearSearchCommand { get; private set; }
    public ICommand HideTracksCommand { get; private set; }
    public ICommand FavoriteTagCommand { get; private set; }
    public ICommand CopyTracksNameCommand { get; private set; }
    public ICommand CopyTracksPathCommand { get; private set; }
    public ICommand CopyTracksFileCommand { get; private set; }
    public ICommand HideTagCommand { get; private set; }
    public ICommand RenameTagCommand { get; private set; }
    public ICommand RandomSongCommand  {  get; private set; }


    public BrowseViewModel(string v, BrowseSettings settings, bool checkPlayFromStart) {
      this.settings = settings;
      this.checkPlayFromStart = checkPlayFromStart;
      this.db = TrackContext.GetTrackContext(v);

      SpaceKeyPressedCommand = new RelayCommand(SpaceKeyPressed);
      AddTagCommand = new RelayCommand(AddTag);
      ClearTagsCommand = new RelayCommand(ClearTags);
      HideTagCommand = new RelayCommand(HideTag);
      RenameTagCommand = new RelayCommand(RenameTag);
      FavoriteTagCommand = new RelayCommand(FavoriteTag);
      ClearSearchCommand = new RelayCommand(ClearSearch);
      HideTracksCommand = new RelayCommand(HideTracks);
      CopyTracksNameCommand = new RelayCommand(CopyTracksName);
      CopyTracksPathCommand = new RelayCommand(CopyTracksPath);
      CopyTracksFileCommand = new RelayCommand(CopyTracksFile);
      RandomSongCommand = new RelayCommand(RandomSong);

      Player = new PlayerModel(this);

      Tracks = new ObservableCollection<Track>(db.Tracks.Include(t => t.Tags));
      Tags = new ObservableCollection<Tag>();
      ReloadTags();

      // Set up collection view and apply sorting
      _itemSourceList = new CollectionViewSource() { Source = Tracks };
      //TracksView.SortDescriptions.Add(new SortDescription("creationTime", ListSortDirection.Descending));
      //_itemSourceList.Filter += new FilterEventHandler(FilterTracks);
      TracksView = _itemSourceList.View;
      TracksView.Filter = FilterTracks;
      //LoadData();

      var _tagSourceList = new CollectionViewSource() { Source = Tags };
      _tagSourceList.SortDescriptions.Add(new SortDescription("IsChecked", ListSortDirection.Descending));
      _tagSourceList.SortDescriptions.Add(new SortDescription("IsFavorite", ListSortDirection.Descending));
      TagsView = _tagSourceList.View;
      TagsView.Filter = FilterTags;

      // load Settings
      TracksSortingDescriptions = settings.TracksSortingDescriptions;
      Search = settings.Search;
    }

    private void ClearTags()
    {
      foreach (var tag in Tags)
      {
        tag.IsChecked = false;
      }

      SearchTag = ""; // clear search
    }

    private void ClearSearch()
    {
      Search = "";
    }

    private bool FilterTags(object obj)
    {
      Tag tag = obj as Tag;
      if (tag != null)
      {
        // don't show hidden tags
        if (tag.IsHidden && !ShowHiddenTags)
        {
          return false;
        }
        
        if (tag.IsChecked)
        {
          // always show tagged
          return true;
        }
        if (SearchTag.Length > 0)
        {
          return tag.Name.ToLower().Contains(SearchTag.ToLower());
        }
      }
      return true;
    }

    private bool FilterTracks(object obj)
    {
      Track track = obj as Track;
      if (track != null)
      {
        // don't show hidden files.
        if (track.IsHidden && !ShowHidden)
        {
          return false;
        } 
        
        // filter at search
        if (Search.Length > 0)
        {
          var words = Search.Split(" ");
          foreach(var word in words)
          {
            if (!track.name.ToLower().Contains(word.ToLower()) && !track.comment.ToLower().Contains(word.ToLower()))
            {
              return false;
            }
          }
        }
        // check with tags

        int numTagChecked = 0;
        int matchedTags = 0;
        foreach (Tag tag in Tags)
        {
          if (tag.IsChecked && (!tag.IsHidden || ShowHiddenTags))
          {
            numTagChecked++;
            if (track.Tags.Contains(tag))
            {
              // have a tag that is included
              matchedTags++;
            }
          }
        }
        if (MainWindowModel.Instance.AndTags)
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
          settings.Search = value;
        }
      }
    }

    private List<SortDescription>? _tracksSortingDescriptions;

    public List<SortDescription>? TracksSortingDescriptions
    {
      get => _tracksSortingDescriptions;
      set
      {
        if (value != _tracksSortingDescriptions)
        {
          //save in settings
          _tracksSortingDescriptions = value;
          RaisePropertyChanged(nameof(TracksSortingDescriptions));
          
          settings.TracksSortingDescriptions = value;
          Settings.saveSettings();
        }
      }
    }


    private string _searchTag = "";
    public string SearchTag
    {
      get { return _searchTag; }
      set
      {
        if (value != SearchTag)
        {
          _searchTag = value;
          RaisePropertyChanged(nameof(SearchTag));
          TagsView.Refresh();
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

    public bool _showHidden = false;
    public bool ShowHidden
    {
      get { return _showHidden; }
      set
      {
        if (value != _showHidden)
        {
          _showHidden = value;
          RefreshTracksView();
          RaisePropertyChanged(nameof(ShowHidden));
        }
      }
    }

    public bool _showHiddenTags = false;
    public bool ShowHiddenTags
    {
      get
      {
        return _showHiddenTags;
      }
      set
      {
        if (value != _showHiddenTags)
        {
          _showHiddenTags = value;
          RefreshTagsView();
          RefreshTracksView();
          RaisePropertyChanged(nameof(ShowHiddenTags));
        }
      }
    }

    private ICollectionView _tracksView;
    public ICollectionView TracksView
    {
      get { return _tracksView; }
      set { _tracksView = value; RaisePropertyChanged(nameof(TracksView)); }
    }

    private ICollectionView _tagsView;
    public readonly bool checkPlayFromStart;
    private Window _dragdropWindow;
    internal Action<int> ScrollTrackIntoView;

    public ICollectionView TagsView
    {
      get { return _tagsView; }
      set { _tagsView = value; RaisePropertyChanged(nameof(TagsView)); }
    }

    public int TrackCount
    {
      get { return TracksView.Cast<object>().Count(); }
    }

    public Visibility ShowNoSongs
    {
      get
      {
        if (TrackCount == 0)
        {
          if (Search != "")
          {
            if (Tags.FirstOrDefault(t => t.IsChecked) != null)
            {
              // also check if we have a search
              return Visibility.Visible;
            }
          }
        }
        return Visibility.Collapsed;
      }
    }

    public void OnClick(MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine("On click!");
      // play if other one is playing
      if (PlayManager.Instance.currentPlayer != Player)
      {
        Player.Play();
        e.Handled = true;
      }
    }

    public void TrackPreviewMouseLeftButtonDown(MouseButtonEventArgs e, object focusedControl)
    {
      // we start this player if the other player is running
      if (PlayManager.Instance.currentPlayer != Player)
      {
        Player.Play();
      }

      // we check the focus. If the focused element has a Track as DataContext
      Track focusedTrack = null;
      if (focusedControl is FrameworkElement element)
      {
        if (element.DataContext is Track track)
        {
          focusedTrack = track;
        }
      }
      System.Diagnostics.Debug.WriteLine($"focused track: {(focusedTrack?.name)}");
      var isFocused = focusedTrack != null;

      if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
      {
        if (e.OriginalSource is Decorator decorator)
        {
          if (decorator.Child is ContentPresenter presenter)
          {
            // check data context
            if (presenter.DataContext is Track selected)
            {
              if (Player.SelectedTracks.Contains(selected) && isFocused)
              {
                e.Handled = true;
                System.Diagnostics.Debug.WriteLine("Handeled");
              }
              startPoint = e.GetPosition(null);
              System.Diagnostics.Debug.WriteLine("Content_presenter.");
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
              if (Player.SelectedTracks.Contains(selected) && isFocused)
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

    public void TrackPreviewMouseLeftButtonDownAlwaysDrag(MouseButtonEventArgs e)
    {
      startPoint = e.GetPosition(null);
    }

      public void TrackPreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && startPoint is Point valueOfStart)
      {
        Point mousePos = e.GetPosition(null);
        Vector diff = valueOfStart - mousePos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
          // Get the dragged ListBoxItem
          var dataGrid = sender as DependencyObject;
          // if (listBoxItem != null)
          //{
          // Initialize drag and drop operation
          // put all the selected strings into it.
          var selected = Player.SelectedTracks.ToFrozenSet();
          var strings = new string[selected.Count];
          int i = 0;
          foreach (var item in selected)
          {
            strings[i] = item.name;
            i++;
          }

          // single only (drag from name or cover)
          Console.WriteLine($"Drag: {Player.currentTrack.path}");
          var toDrag = Player.currentTrack;
          DataObject dragData = new DataObject(DataFormats.FileDrop, new string[] { toDrag.path });
          // all of them (if from data grid, add all of them)
          // Sadly the following is not working
          if (sender is Wpf.Ui.Controls.DataGrid dg)
          {
            List<string> stringPaths = new List<string>();
            foreach (var item in selected)
            {
              stringPaths.Add(item.path.ToString());
              Debug.WriteLine(item.path.ToString());
            }
            dragData = new DataObject(DataFormats.FileDrop, stringPaths.ToArray());
          }
          else
          {
            // only the currently played track
            selected = new List<Track>([toDrag]).ToFrozenSet();
          }
          dragData.SetData("MusikMakerTrack", strings); // use the full selection.
          dragData.SetData(DataFormats.Text, toDrag.name);
          // create own window as tooltip
          CreateDragDropWindow(selected);
          DragDropEffects effect = DragDrop.DoDragDrop(dataGrid, dragData, DragDropEffects.Copy);
          // pause track 
          if (effect == DragDropEffects.Link)
          {
            System.Diagnostics.Debug.WriteLine("Got linked so not stopping");
          }
          else if (effect == DragDropEffects.Copy)
          {
            Player.Pause();
            Console.WriteLine($"sucessfully dragged {toDrag.name}");
          }
          else
          {
            Console.WriteLine($"drag failed for {toDrag.name}");
          }
          // hide visual clue window
          if (this._dragdropWindow != null)
          {
            this._dragdropWindow.Close();
            this._dragdropWindow = null;
          }
        }
      }
    }

    private void CreateDragDropWindow(FrozenSet<Track> tracks)
    {
      this._dragdropWindow = new DragWindow(tracks);

      Win32Point w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);


      this._dragdropWindow.Left = w32Mouse.X;
      this._dragdropWindow.Top = w32Mouse.Y;
      this._dragdropWindow.Show();
    }
    
    // needed to position drag and drop tooltip window
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };
      
    // feedback for drag and drop shit
    public void TrackGiveFeedback()
    {
      // update the position of the visual feedback item
      Win32Point w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);
      if(this._dragdropWindow != null)
      {

        this._dragdropWindow.Left = w32Mouse.X;
        this._dragdropWindow.Top = w32Mouse.Y + 5;
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

    public string? openTagDialog(string question)
    {
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
      if (newName != null)
      {
        toRename.Name = newName;
        db.SaveChanges();

      }
    }
    
    internal void HideTag()
    {
      SelectedTag.IsHidden = !SelectedTag.IsHidden;
      if (SelectedTag.IsHidden == false)
      {
        SelectedTag.IsChecked = false;
      }
      db.SaveChanges();
      TagsView.Refresh();
    }

    internal void FavoriteTag()
    {
      SelectedTag.IsFavorite = !SelectedTag.IsFavorite;
      TagsView.Refresh();
    }



    private void ReloadTags()
    {
      // just overwrite LOL
      Tags.Clear();
      foreach (var tag in db.Tags)
      {
        tag.PropertyChanged += Tag_PropertyChanged;
        Tags.Add(tag);
      }
      if (TagsView != null)
      {
        TagsView.Refresh();
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
          TagsView.Refresh();
          RaisePropertyChanged(nameof(ShowNoSongs));
          db.SaveChanges();
          break;
      }
    }

    internal void AddTrackToTag(string trackName, Tag tag)
    {
      // find the track
      Track? track = db.Tracks.Find(trackName);
      if (track != null)
      {
        if (!track.Tags.Contains(tag))
        {
          // need to add it
          track.AddTag(tag);
          db.SaveChanges();
          System.Diagnostics.Debug.WriteLine($"added tag '{tag.Name}' to track {trackName}");
        }
      }
      else
      {
        Console.WriteLine($"Could not find track with name '{trackName}' to add tag '{tag.Name}' to it!");
      }
    }

    internal void HideTracks()
    {
      // delete current selection
      // TODO: maybee add a question dialog LOL
      selected = Player.SelectedTracks.ToFrozenSet();
      foreach (Track track in selected)
      {
        // toggle
        track.IsHidden = !track.IsHidden;
      }
      db.SaveChanges();
      RefreshTracksView();
    }

    private void CopyTracksName()
    {
      // copy the selected track names into the clipboard
      selected = Player.SelectedTracks.ToFrozenSet();
      if (selected.Count == 1)
      {
        Clipboard.SetText(selected.First().name);
      }
      else
      {
        Clipboard.SetText(string.Join(", ", selected.Select(t => t.name)));
      }
    }
    
    private void CopyTracksPath()
    {
      // copy the selected track names into the clipboard
      selected = Player.SelectedTracks.ToFrozenSet();
      if (selected.Count == 1)
      {
        Clipboard.SetText(selected.First().path);
      }
      else
      {
        Clipboard.SetText(string.Join("\n", selected.Select(t => t.path)));
      }
    }

    public void CopyTracksFile()
    {
      // copy the selected track names into the clipboard
      selected = Player.SelectedTracks.ToFrozenSet();
      StringCollection paths = new StringCollection();
      foreach (var elm in selected)
      {
        paths.Add(elm.path);
      }

      Clipboard.SetFileDropList(paths);
    }

    public void RefreshTracksView()
    {
      TracksView.Refresh();
      RaisePropertyChanged(nameof(TrackCount));
      RaisePropertyChanged(nameof(ShowNoSongs));
    }

    public void RefreshTagsView()
    {
      TagsView.Refresh();
    }

    internal void ReloadData()
    {
      Tracks.Clear();
      foreach (Track track in db.Tracks.Include(t => t.Tags))
      {
        Tracks.Add(track);
      }
      RaisePropertyChanged(nameof(TrackCount));
      RaisePropertyChanged(nameof(ShowNoSongs));
      ReloadTags();
    }

    internal void ToggleCurrentTag()
    {
      if(SelectedTag != null)
      {
        SelectedTag.IsChecked = !SelectedTag.IsChecked;
      }
    }

    private void RandomSong()
    {
      if (TracksView.IsEmpty)
      {
        return;
      }
      Random random = new Random();
      int randomIndex = random.Next(0, TrackCount-1); // without last
      

      Track selected =  TracksView.Cast<Track>().ElementAt(randomIndex);
      if(selected == Player.currentTrack)
      {
        // just use the last
        selected = TracksView.Cast<Track>().ElementAt(TrackCount-1);
      }
      Player.currentTrack = selected;
      // scroll to bottom so the view is at the top
      ScrollTrackIntoView(TrackCount - 1);
      ScrollTrackIntoView(randomIndex);
    }
  }
}