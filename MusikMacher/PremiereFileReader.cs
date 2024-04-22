using System;
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
  public class Clip
  {
    public string Filename; // file path
    public string Name; // only the name of the file
    public double Start; // start in seconds in the sequence
    public double Stop;
    public double InTime; // part of the track.
    public double OutTime;

    public Clip(string filename, string name, double start, double stop, double inTime, double outTime)
    {
      Filename = filename;
      Name = name;
      Start = start;
      Stop = stop;
      InTime = inTime;
      OutTime = outTime;
    }
  }
}
class PremiereFileReader
{
  public static void ReadFile(Action<string> log, string filename)
  {
    // Load the file
    var project = new Project(filename);
    
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
        var audioTrackGroupElement = tree.XPathSelectElement($".//AudioTrackGroup[@ObjectID='{audioTrackGroupId}']");
        if (audioTrackGroupElement != null)
        {
          string trackFrameRateString = audioTrackGroupElement.XPathSelectElement(".//FrameRate").Value;
          // We have some audio!
          var tracks = audioTrackGroupElement.XPathSelectElements(".//Track");
          foreach (var track in tracks)
          {
            string trackObjectUID = track.Attribute("ObjectURef").Value;
            var audioClipTrack = tree.XPathSelectElement($".//AudioClipTrack[@ObjectUID='{trackObjectUID}']");
            var trackItems = audioClipTrack.XPathSelectElements(".//TrackItem");
            foreach (var trackItem in trackItems)
            {
              string trackItemUID = trackItem.Attribute("ObjectRef").Value;
              var audioClipTrackItem = tree.XPathSelectElement($".//AudioClipTrackItem[@ObjectID='{trackItemUID}']");
              var clipTrackItem = audioClipTrackItem.Element("ClipTrackItem");
              var trackItemElement = clipTrackItem.Element("TrackItem");

              // Position in the sequence.
              string clipStartString = trackItemElement.Element("Start").Value;
              string clipEndString = trackItemElement.Element("End").Value;
              // Log($"from {clip_start} to {clip_end}");

              var subClipRef = clipTrackItem.Element("SubClip");
              string subClipObjectID = subClipRef.Attribute("ObjectRef").Value;
              var subClip = tree.XPathSelectElement($".//SubClip[@ObjectID='{subClipObjectID}']");
              var clipRef = subClip.Element("Clip");
              string clipObjectID = clipRef.Attribute("ObjectRef").Value;
              var audioClip = tree.XPathSelectElement($".//AudioClip[@ObjectID='{clipObjectID}']");
              var clipXElement = audioClip.Element("Clip");

              // Part of the song
              string songInPoint = clipXElement.Element("InPoint").Value;
              string songOutPoint = clipXElement.Element("OutPoint").Value;
              string songColor = clipXElement.XPathSelectElement(".//asl.clip.label.name").Value;
              // Log($"position {song_in_point} to {song_out_point}");

              // Get the filename
              var source = clipXElement.XPathSelectElement(".//Source");
              string sourceId = source.Attribute("ObjectRef").Value;
              var audioMediaSource = tree.XPathSelectElement($".//AudioMediaSource[@ObjectID='{sourceId}']");
              string originalDuration = audioMediaSource.Element("OriginalDuration").Value;

              var mediaSource = audioMediaSource.Element("MediaSource");

              var mediaRef = mediaSource.Element("Media");
              string mediaObjectUID = mediaRef.Attribute("ObjectURef").Value;
              var media = tree.XPathSelectElement($".//Media[@ObjectUID='{mediaObjectUID}']");
              string title = media.Element("Title").Value;
              string audioRate = media.Element("ConformedAudioRate").Value;

              int trackFrameRate = int.Parse(trackFrameRateString);
              double songStart = int.Parse(clipStartString);
              
              log($"at {clipStartString}-{clipEndString} [{long.Parse(songInPoint) / trackFrameRate}-{long.Parse(songOutPoint) / trackFrameRate}] {title}");
              
              // TODO: reformat timestaps into mp3s.
              // Using the logic of this script
              //var clip = new Clip(filename, title, )
            }
          }
        }
        trackGroupId++;
      }
    }
  }
}
