using LorusMusikMacher.database;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TagLib.Mpeg;

namespace MusikMacher
{
  // represents one track file
  public class Track : INotifyPropertyChanged
  {

    public Track(string name, string path, DateTime creationTime)
    {
      this.name = name;
      this.creationTime = creationTime;
      this.path = path;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void AddTag(Tag tag)
    {
      // only add if it does not have that tag.
      if (!Tags.Contains(tag))
      {
        Tags.Add(tag);
        OnPropertyChanged(nameof(Tags));
      }
    }

    public string path { get; set; }
    [Key]
    public string name { get; set; }
    public TagList Tags { get; } = [];

    private int? _length;
    public int? length // length of the song
    {
      get { return _length; }
      set
      {
        if (value != _length)
        {
          _length = value;
          OnPropertyChanged(nameof(length));
        }
      }
    }

    public DateTime creationTime { get; set; }


    [NotMapped]
    private bool _artworkLoaded = false;
    private BitmapImage? _artwork = null;
    [NotMapped]
    public BitmapImage? Artwork
    {
      get
      {
        if (_artworkLoaded)
        {
          return _artwork;
        } else
        {
          _artworkLoaded = true;

          // load in waveform in other thread
          ArtworkLoader.getInstance().Shedule(new Tuple<string, Action<byte[]?>>(path,
                (byte[]? data) =>
                {
                  if (data != null)
                  {
                    _artwork = DataToBitmapImage(data);
                    OnPropertyChanged(nameof(Artwork));
                  }
                }));
          return null;
        }
      }
    }

    public static byte[]? LoadArtworkData(string path)
    {
      // load meta data
      // Get the tag for the file
      TagLib.Tag tag = null;
      TagLib.File tagFile = null;
      try
      {
        tagFile = TagLib.File.Create(path);
        tag = tagFile.Tag;
      }
      catch (Exception)
      {
        System.Diagnostics.Debug.WriteLine("Could not parse tags for file " + path);
      }

      // If we have no pictures, bail out
      if (tagFile == null || tag == null || tag.Pictures.Length == 0) return null;

      // Find the frontcover
      var picture = tag.Pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover);
      //if (picture == null) picture = tag.Pictures.First();
      if (picture == null) return null;


      return picture.Data.ToArray();

    }
    
    public static BitmapImage? DataToBitmapImage(byte[] data)
    {
      // Get the Image
      BitmapImage artwork = null;
      try
      {
        using (MemoryStream memory = new MemoryStream(data))
        {
          artwork = new BitmapImage();
          artwork.BeginInit();
          artwork.StreamSource = memory;
          artwork.CacheOption = BitmapCacheOption.OnLoad;
          artwork.EndInit();
        }
      }
      catch (Exception)
      {
        System.Diagnostics.Debug.WriteLine($"Failed to convert artwork");
      }

      return artwork;
    }

    public static BitmapImage ConvertImageToBitmapImage(System.Drawing.Image image)
    {
      Bitmap bitmap = new Bitmap(image);

      using (MemoryStream memory = new MemoryStream())
      {
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        memory.Position = 0;

        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
      }
    }

    public BitmapImage LoadWaveform()
    {
      // move into track class?

      // Load the MP3 file
      using (var audioFile = new AudioFileReader(path))
      {
        var renderer = new WaveFormRenderer();
        var settings = new SoundCloudBlockWaveFormSettings(Color.FromArgb(66, 150, 96), Color.FromArgb(66, 150, 96), Color.FromArgb(124, 153, 134),
                Color.FromArgb(154, 184, 164));
        var soundCloudLightBlocks = new SoundCloudBlockWaveFormSettings(Color.FromArgb(102, 102, 102), Color.FromArgb(103, 103, 103), Color.FromArgb(179, 179, 179),
    Color.FromArgb(218, 218, 218))
        { Name = "SoundCloud Light Blocks" };
        var defaultSettings = new StandardWaveFormRendererSettings();
        defaultSettings.TopPeakPen = Pens.DarkGray;
        defaultSettings.TopSpacerPen = Pens.White;
        defaultSettings.BottomPeakPen = Pens.DarkGray;
        defaultSettings.BottomSpacerPen = Pens.White;
        defaultSettings.BackgroundColor = Color.White;
        var image = renderer.Render(audioFile, new AveragePeakProvider(4), soundCloudLightBlocks);
        return ConvertImageToBitmapImage(image);
      }
    }

    internal static System.Windows.Point[][] LoadWaveformGeometry(string path)
    {
      // TODO: move this into own 
      using (var audioFile = new AudioFileReader(path))
      {
        var defaultSettings = new StandardWaveFormRendererSettings();
        defaultSettings.Width = 1000;
        return WaveFormPathRenderer.LoadPoints(audioFile, new AveragePeakProvider(4), defaultSettings);
      }
    }
  }

  public class TagList : List<Tag>, IComparable
  {
    public int CompareTo(object? obj)
    {
      if (obj is TagList tagList)
      {
        if (this.Count == 1 && tagList.Count == 1)
        {
          return this[0].Name.CompareTo(tagList[0].Name);
        }
        else
        {
          return this.Count.CompareTo(tagList.Count);
        }
        for(int i =0; i < this.Count; i++)
        {
          if(tagList.Count > i)
          {
            if (this[i].Name != tagList[i].Name)
            {
              return this[i].Name.CompareTo(tagList[i].Name);
            }
          } else
          {
            // we are longer
            return -1;
          }
        }
        // the other is longer
        return 1;
      }
      throw new NotImplementedException();
    }
  }
}
