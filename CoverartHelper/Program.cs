using System;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;

class ChildProcess
{
  static void Main(string[] args)
  {
    Console.WriteLine($"Coverart Helper started.");
    string clientid = "";
    if (args.Length > 0)
    {
      Console.WriteLine($"Received argument: {args[0]}");
      clientid = args[0];
    }

    using (var pipeClient = new NamedPipeClientStream(".", "CoverartPipe"+ clientid, PipeDirection.InOut))
    {
      pipeClient.Connect();

      using (var reader = new StreamReader(pipeClient))
      using (var writer = new StreamWriter(pipeClient))
      {
        while (true)
        {
          string message = reader.ReadLine();
          if(message == "close")
          {
            Console.WriteLine("got close command. Exiting!");
            return;
          }
          Console.WriteLine($"Received: {message}");

          string response = $"Load Artwork for: {message}";
          writer.WriteLine(response);
          writer.Flush();

          byte[]? data = null;
          try
          {

              data = LoadArtworkData(message);
          }
          catch (Exception e)
          {
            writer.WriteLine($"Helper Exception: {e}");
            writer.Flush();
            continue;
          }
          if(data == null)
          {
            writer.WriteLine("null");
            writer.Flush();
          }
          else
          {
            writer.WriteLine(Convert.ToBase64String(data));
            writer.Flush();
          }
        }
      }
    }
  }

  public static byte[]? LoadArtworkData(string path)
  {
    if (path.ToLower().EndsWith(".mp3"))
    {
      // load meta data from mp3 file
      // Get the tag for the file
      TagLib.Tag tag = null;
      TagLib.File tagFile = null;
      tagFile = TagLib.File.Create(path);
      tag = tagFile.Tag;

      // If we have no pictures, bail out
      if (tagFile == null || tag == null || tag.Pictures.Length == 0) return null;

      // Find the frontcover
      var picture = tag.Pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover);
      //if (picture == null) picture = tag.Pictures.First();
      if (picture == null) return null;


      return picture.Data.ToArray();
    }
    else
    {
      // fall back to ShellFile provided thumbnail
      var thumbnail = ShellFile.FromFilePath(path).Thumbnail;
      // convert to bytes
      if (thumbnail != null)
      {
        using (MemoryStream outStream = new MemoryStream())
        {
          thumbnail.Bitmap.Save(outStream, ImageFormat.Png);
          return outStream.ToArray();
        }
      }
    }

    return null;
  }
}
