using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using MusikMacher;

// data classes for the premiere data
namespace MusikMacher{
  public class Project
  {
    public string Filename;
    public List<Sequence> Sequences;

    public Project(string filename)
    {
      Filename = filename;
      Sequences = new List<Sequence>();
    }
  }
  
  public class Sequence
  {
    public string Name;
    public List<Clip> Clips;

    public Sequence(string name)
    {
      Name = name;
      Clips = new List<Clip>();
    }
  }  

  // usage of a track in a sequce
  public class Clip : INotifyPropertyChanged
  {
    public string Filename { get; set; } // file path
    public string Name { get; set; } // only the name of the file
    public string Color{ get; set; } // color in adobe
    public double Start { get; set; } // start in seconds in the sequence
    public double Stop { get; set; }
    public TimeSpan InTime { get; set; } // part of the track.
    public TimeSpan OutTime { get; set; }
    private bool _include;
    public bool Include
    {
      get => _include;
      set
      {
        if (value != _include)
        {
          _include = value;
          OnPropertyChanged(nameof(Include));
        }
      }
    }
    public double Time { get; set; }
    public int? TrackIndex { get; set; }


    public Clip(string filename, string name, /*double start, double stop, */TimeSpan inTime, TimeSpan outTime, int? trackIndex)
    {
      Filename = filename;
      Name = name;
      //Start = start;
      //Stop = stop;
      InTime = inTime;
      OutTime = outTime;
      Time = outTime.TotalSeconds - inTime.TotalSeconds;
      TrackIndex = trackIndex;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal object NameOnly()
    {
      // remove file ending
      var lastDotIndex = Name.LastIndexOf('.');
      if(lastDotIndex != -1)
      {
        return Name.Substring(0, lastDotIndex);
      }
      return Name;
    }
  }
  class PremiereFileReader
  {
    public static List<Clip> ReadFile(Action<string> log, string filename)
    {
      // strip " if the are there
      if(filename.StartsWith("\"") && filename.EndsWith("\""))
      {
        filename = filename.Substring(1, filename.Length - 2);
      }

      // Load the file
      var project = new Project(filename);

      var clips = new List<Clip>();

      try
      {
        byte[] data = File.ReadAllBytes(filename);
        byte[] unziped;
        using (MemoryStream ms = new MemoryStream(data))
        {
          using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress))
          {
            using (MemoryStream unzipMs = new MemoryStream())
            {
              zipStream.CopyTo(unzipMs);
              unziped = unzipMs.ToArray();
            }
          }
        }
        string xmlString = System.Text.Encoding.UTF8.GetString(unziped);

        XElement tree = XElement.Parse(xmlString);

        // build up key values for the objectIDs and objectUIDs. for O(n) (linear) runtime.
        Dictionary<String, XElement> byObjectID = new Dictionary<String, XElement>();
        Dictionary<String, XElement> byObjectUID = new Dictionary<String, XElement>();

        foreach (var child in tree.Elements())
        {
          // save the by OBjectRef
          var objectID = child.Attribute("ObjectID");
          if (objectID != null)
          {
            byObjectID.Add(objectID.Value, child);
          }

          // save the by OBjectURef
          var objectUID = child.Attribute("ObjectUID");
          if (objectUID != null)
          {
            byObjectUID.Add(objectUID.Value, child);
          }
        }
        // Find the sequences
        var sequences = tree.Elements("Sequence");
        foreach (var sequenceXElement in sequences)
        {
          string sequenceName = sequenceXElement.Element("Name").Value;
          var sequence = new Sequence(sequenceName);
          project.Sequences.Add(sequence);

          log($"=== SEQUENCE: {sequenceName} ===");
          var trackGroups = sequenceXElement.Element("TrackGroups").Elements("TrackGroup");
          int trackGroupId = 0; // TrackGroupId enumeration

          foreach (var trackGroup in trackGroups)
          {
            log($"TrackGroup {trackGroupId}");
            var audioTrackGroup = trackGroup.Element("Second");
            string audioTrackGroupId = audioTrackGroup.Attribute("ObjectRef").Value;
            XElement audioTrackGroupElement;//tree.XPathSelectElement($".//AudioTrackGroup[@ObjectID='{audioTrackGroupId}']");
            if (byObjectID.TryGetValue(audioTrackGroupId, out audioTrackGroupElement) && audioTrackGroupElement.Name == "AudioTrackGroup")
            {
              string trackFrameRateString = audioTrackGroupElement.XPathSelectElement(".//FrameRate").Value;

              // adapted from https://github.com/sergeiventurinov/PRPROJ-READER/blob/a51a2a5b1095b8bdead114c812e85132a5eb26e0/PRPROJ-READER.py#L745
              var seq_aud_frame_rate = long.Parse(trackFrameRateString);
              seq_aud_frame_rate = ((long)5292000 * (long)48000) / (long)seq_aud_frame_rate;//((long)5292000 * (long)48000) / (long)seq_aud_frame_rate;
              log($"Audio FrameRate:{seq_aud_frame_rate}kHz");

              // We have some audio!
              var tracks = audioTrackGroupElement.XPathSelectElements(".//Track");
              foreach (var track in tracks)
              {
                string trackObjectUID = track.Attribute("ObjectURef").Value;
                XElement audioClipTrack;  //tree.XPathSelectElement($".//AudioClipTrack[@ObjectUID='{trackObjectUID}']");
                byObjectUID.TryGetValue(trackObjectUID, out audioClipTrack);

                int? trackIndex = null;
                XElement index;
                index = audioClipTrack.XPathSelectElement("ClipTrack/ClipItems/Index");
                if(index != null)
                {
                  trackIndex = int.Parse(index.Value);
                } else
                {
                  Console.WriteLine($"Failed to load track index for AudioClipTrack with ObjectUID {trackObjectUID}");
                }
                log("trackIndex: " + trackIndex);

                var trackItems = audioClipTrack.XPathSelectElements(".//TrackItem");
                foreach (var trackItem in trackItems)
                {
                  string trackItemID = trackItem.Attribute("ObjectRef").Value;
                  XElement audioClipTrackItem; // = tree.XPathSelectElement($".//AudioClipTrackItem[@ObjectID='{trackItemUID}']");
                  byObjectID.TryGetValue(trackItemID, out audioClipTrackItem);
                  var clipTrackItem = audioClipTrackItem.Element("ClipTrackItem");
                  var trackItemElement = clipTrackItem.Element("TrackItem");

                  // Position in the sequence.
                  string clipStartString = trackItemElement.Element("Start").Value;
                  string clipEndString = trackItemElement.Element("End").Value;
                  // Log($"from {clip_start} to {clip_end}");

                  var subClipRef = clipTrackItem.Element("SubClip");
                  string subClipObjectID = subClipRef.Attribute("ObjectRef").Value;
                  XElement subClip; // = tree.XPathSelectElement($".//SubClip[@ObjectID='{subClipObjectID}']");
                  byObjectID.TryGetValue(subClipObjectID, out subClip);
                  var clipRef = subClip.Element("Clip");
                  string clipObjectID = clipRef.Attribute("ObjectRef").Value;
                  XElement audioClip; // = tree.XPathSelectElement($".//AudioClip[@ObjectID='{clipObjectID}']");
                  byObjectID.TryGetValue(clipObjectID, out audioClip);
                  var clipXElement = audioClip.Element("Clip");

                  // Part of the song
                  string songInPoint = clipXElement.Element("InPoint").Value;
                  string songOutPoint = clipXElement.Element("OutPoint").Value;
                  string songColor = "unkown";
                  var colorTagElement = clipXElement.XPathSelectElement(".//asl.clip.label.name");
                  if (colorTagElement != null)
                  {
                    songColor = clipXElement.XPathSelectElement(".//asl.clip.label.name").Value;
                  }
                  // Log($"position {song_in_point} to {song_out_point}");

                  // Get the filename
                  var source = clipXElement.XPathSelectElement(".//Source");
                  string sourceId = source.Attribute("ObjectRef").Value;
                  XElement audioMediaSource;// = tree.XPathSelectElement($".//AudioMediaSource[@ObjectID='{sourceId}']");\
                  byObjectID.TryGetValue(sourceId, out audioMediaSource);
                  string originalDuration = audioMediaSource.Element("OriginalDuration").Value;

                  var mediaSource = audioMediaSource.Element("MediaSource");

                  var mediaRef = mediaSource.Element("Media");
                  string mediaObjectUID = mediaRef.Attribute("ObjectURef").Value;
                  XElement media;// = tree.XPathSelectElement($".//Media[@ObjectUID='{mediaObjectUID}']");
                  byObjectUID.TryGetValue(mediaObjectUID, out media);
                  string title = media.Element("Title").Value;
                  string filePath = media.Element("FilePath").Value;
                  string audioRate = media.Element("ConformedAudioRate").Value;

                  long audioRateLong = long.Parse(trackFrameRateString) * 48000 ; // long.Parse(audioRate);

                  long trackFrameRate = long.Parse(trackFrameRateString); // 44khz 60fps
                  double songStart = long.Parse(clipStartString);

                  log($"at {clipStartString}-{clipEndString} [{(long.Parse(songInPoint) / (double)audioRateLong):F2}-{(long.Parse(songOutPoint) / (double)audioRateLong):F2}] {title}");

                  if (!title.EndsWith(".mp4")) {
                    clips.Add(new Clip(filePath,
                      title,
                      TimeSpan.FromSeconds(long.Parse(clipStartString) / (double)audioRateLong),
                      TimeSpan.FromSeconds(long.Parse(clipEndString) / (double)audioRateLong),
                      trackIndex));
                  }
                  // TODO: reformat timestaps into mp3s.
                  // Using the logic of this script
                  //var clip = new Clip(filename, title, )
                }
              }
            }
            trackGroupId++;
          }
        }
      } catch (Exception ex)
      {
        log($"failed with exception {ex.Message} {ex.StackTrace}");
      }
      
      //check if we can merge clips if they are after each other.
      List<Clip> mergedClips = new List<Clip>(clips.Count);
      Clip? lastElement = null;
      foreach (var clip in clips.OrderBy(c => c.InTime))
      {
        if (lastElement != null)
        {
          if (lastElement.Name == clip.Name)
          {
            // we merge
            lastElement.OutTime = clip.InTime;
            lastElement.Time += clip.Time; // add time
            continue;
          } 
        }

        // save into results
        mergedClips.Add(clip);
        lastElement = clip;
      }

      // preselect if trackindex is 1
      clips.Select(clip =>
      {
        if (clip.TrackIndex == 1)
        {
          clip.Include = true;
        }
        return false;
      }).ToList();

      log($"===== TRACK LIST =====");
      foreach (var clip in mergedClips)
      {
        // TODO: also handle with hours case.
        var inTime = clip.InTime.ToString(@"mm\:ss\.fff");
        var outTime = clip.OutTime.ToString(@"mm\:ss\.fff");

        var name = clip.Name;
        // remove file ending.
        var lastDotiIdex = name.LastIndexOf(".");
        if (lastDotiIdex != -1)
        {
          name = name.Substring(0, lastDotiIdex);
        }

        log($"[{inTime} - {outTime}] [{clip.Time:F1}s] {clip.TrackIndex} {name}");
      }

      log($"===== TRACK LIST =====");
      foreach (var clip in mergedClips)
      {
        // TODO: also handle with hours case.
        var inTime = clip.InTime.ToString(@"mm\:ss\.fff");
        var outTime = clip.OutTime.ToString(@"mm\:ss\.fff");

        var name = clip.Name;
        // remove file ending.
        var lastDotiIdex = name.LastIndexOf(".");
        if (lastDotiIdex != -1)
        {
          name = name.Substring(0, lastDotiIdex);
        }
      }
      return mergedClips;
    }
  }
}
