using LorusMusikMacher.database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

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


    public BitmapImage? LoadArtwork()
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
      if (picture == null) picture = tag.Pictures.First();

      // Get the Image
      BitmapImage artwork = null;
      try
      {
        using (MemoryStream memory = new MemoryStream(picture.Data.ToArray()))
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
        System.Diagnostics.Debug.WriteLine($"Failed to load artwork: {path}");
      }

      return artwork;
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
