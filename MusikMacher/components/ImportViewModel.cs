using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using MusikMacher.components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using MusikMacher.dialog;

namespace MusikMacher
{
  internal class BrowseCollection: INotifyPropertyChanged
  {
    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        if(_name != value)
        {
          _name = value;
          OnPropertyChanged(nameof(Name));
        }
      }
    }
    public BrowseViewModel ViewModel;

    public BrowseCollection(string name, BrowseViewModel viewModel)
    {
      this.Name = name;
      this.ViewModel = viewModel;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  internal class ImportViewModel : ViewModelBase
  {
    public ICommand LoadDataCommand { get; private set; }
    public ICommand LoadWaveformsCommand { get; private set; }

    MainWindowModel model;
    private BrowseViewModel browseTracksViewModel;

    public ObservableCollection<BrowseCollection> ImportIntoItems { get; }

    private BrowseCollection _importInto;
    public BrowseCollection ImportInto
    {
      get { return _importInto; }
      set { 
        if(_importInto != value)
        {
          _importInto = value;
          RaisePropertyChanged(nameof(ImportInto));
        }
      }
    }

    public ImportViewModel(MainWindowModel model, BrowseViewModel browseTracksViewModel, BrowseViewModel browseEffectsViewModel)
    {
      LoadDataCommand = new RelayCommand(LoadData);
      LoadWaveformsCommand = new RelayCommand(LoadWaveforms);

      this.model = model;
      this.browseTracksViewModel = browseTracksViewModel;
      ImportIntoItems = new ObservableCollection<BrowseCollection>([new BrowseCollection("Songs", browseTracksViewModel),
                              new BrowseCollection("Effects", browseEffectsViewModel)]);
      ImportInto = ImportIntoItems[0];

      // load Settings
      Settings settings = Settings.getSettings();
      dataLocation = settings.ImportPath;
    }

    private void LoadWaveforms()
    {
      // open loading dialog
      if (Dialog is null || !Dialog.IsVisible)
      {
        Dialog = new PreloadWaveformsWindow();
        Dialog.Show();
      }
      else
      {
        Dialog.Focus();
      }
    }

    public PreloadWaveformsWindow? Dialog { get; set; }

    private string _dataLocation = "C:/some/folder";
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

    private string _loadingLog = "";
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

    private void logLoading(string s)
    {
      loadingLog += s + "\n";
    }

    private void LoadData()
    {
      // saving location in settings
      var settings = Settings.getSettings();
      settings.ImportPath = dataLocation;
      settings.ImportSubfolders = ImportSubfolders;
      Settings.saveSettings();

      // get the correct db
      var currentbrowseViewModel = ImportInto.ViewModel;
      var db = currentbrowseViewModel.db;

      loadingLog = "Trying to load data from " + dataLocation + "\n";
      // we set the folder name as the tag
      var parent = Path.GetFileName(dataLocation);
      LoadData([parent], dataLocation, db);

      // reload data in main view model
      // TODO: reload the correct view model. where wo got the db from too.
      currentbrowseViewModel.ReloadData();
    }

    private void LoadData(List<string> parents, string location, TrackContext db)
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

          if (fileInfo.Extension == ".mp3" || fileInfo.Extension == ".m4a" || fileInfo.Extension == ".mp4")
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
              if (tag != null)
              {
                songExists.AddTag(tag);
              }
              existing += 1;
            }
            else
            {
              created += 1;
              var track = new Track(fileInfo.Name, filePath, fileInfo.LastWriteTime);
              if (tag != null)
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
              LoadData(newParents, dirPath, db);
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
  }
}
