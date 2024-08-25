using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Task = System.Threading.Tasks.Task;

namespace MusikMacher
{
  internal class ArtworkOutsideLoader : LoaderWorker<string, byte[]?>
  {
    private static ArtworkOutsideLoader Instance = new ArtworkOutsideLoader();
    private Process childProcess;
    private NamedPipeServerStream pipeServer;
    private StreamWriter writer;
    private StreamReader reader;
    private string clientId;

    private ArtworkOutsideLoader() : base(true, false)
    {
      clientId = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 8)
    .Select(s => s[new Random().Next(s.Length)]).ToArray());
    }

    ~ArtworkOutsideLoader()
    {
      Console.WriteLine("Killing child");
      childProcess.Kill();
      Cancle();
      CleanUp();  
    }

    public static ArtworkOutsideLoader getInstance()
    {
      return Instance;
    }
    internal override byte[]? Handle(string path)
    {
      return LoadArtworkAsync(path).Result;
    }

    private async Task<byte[]?> LoadArtworkAsync(string path)
    {
      await StartOrConnectToChildProcess();
      await SendMessage(path);
      //Console.WriteLine($"loading for {path}");
      var response = await ReceiveMessage();
      while(!(response.StartsWith("Load Artwork for:") && response.EndsWith(path)))
      {
        Console.WriteLine("ArtworkLoader:strange response:" + response);
        response = await ReceiveMessage();
        // load another one
      }
      //Console.WriteLine($"got response: {response}");
      var response2 = await ReceiveMessage();
      if(response2 == null || response2 == "null")
      {
        // Console.WriteLine($"ArtworkLoader:got response2 null :'(((");
        return null;
      }
      if(response2.Length < 200 || response2.StartsWith("Helper Exception:"))
      {
        Console.WriteLine($"ArtworkLoader: got response2: {response2}");
      } else
      {
        //Console.WriteLine($"got response2: omitted");
      }
      return Convert.FromBase64String(response2);
    }

    private async Task StartOrConnectToChildProcess(bool allowStart=true)
    {
      if (pipeServer == null || !pipeServer.IsConnected)
      {
        if(pipeServer != null)
        {
          Console.WriteLine($"ArtworkLoader: Pipe seems to be broken so we restart: {pipeServer.IsConnected} running processes: {Process.GetProcessesByName("CoverartHelper").Length}");
          pipeServer.Close();

          // force restart
          childProcess.Kill();
          childProcess = null;
        }

        pipeServer = new NamedPipeServerStream("CoverartPipe"+ clientId, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 65536);

        if (allowStart && childProcess == null)
        {
          Console.WriteLine("ArtworkLoader: Starting Coverart Helper");
          childProcess = new Process
          {
            StartInfo = new ProcessStartInfo
            {
              Arguments = clientId,
              FileName = "CoverartHelper.exe",
              UseShellExecute = false,
              CreateNoWindow = true
            }
          };
          childProcess.Start();
        }

        await pipeServer.WaitForConnectionAsync();
        Console.WriteLine("ArtworkLoader: got connection");

        writer = new StreamWriter(pipeServer);
        reader = new StreamReader(pipeServer);
      }
    }

    private async Task SendMessage(string message)
    {
      await writer.WriteLineAsync(message);
      await writer.FlushAsync();
    }

    private async Task<string> ReceiveMessage()
    {
      return await reader.ReadLineAsync();
    }

    public void CleanUp()
    {
      writer?.Close();
      reader?.Close();
      pipeServer?.Close();
      childProcess?.Close();
    }
  }
}