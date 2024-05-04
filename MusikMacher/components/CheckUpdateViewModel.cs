using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusikMacher.components
{
  class CheckUpdateViewModel: ViewModelBase
  {
    public static string VERSION = "v0.1.7-alpha";

    private string _updateResult = "";
    public string UpdateResult
    {
      get { return _updateResult; }
      set
      {
        if (_updateResult != value)
        {
          _updateResult = value;
          RaisePropertyChanged(nameof(UpdateResult));
        }
      }
    }

    private string _downloadFilename = "";
    public string DownloadFilename
    {
      get { return _downloadFilename; }
      set
      {
        if (_downloadFilename != value)
        {
          _downloadFilename = value;
          RaisePropertyChanged(nameof(DownloadFilename));
        }
      }
    }

    private string _downloadLink = "";
    public string DownloadLink
    {
      get { return _downloadLink; }
      set
      {
        if (_downloadLink != value)
        {
          _downloadLink = value;
          RaisePropertyChanged(nameof(DownloadLink));
        }
      }
    }

    public ICommand CheckCommand { get; private set; }

    public CheckUpdateViewModel()
    {
      CheckCommand = new RelayCommand(Check);
    }

    public void LogUpdateInfo(string message)
    {
      UpdateResult += message + "\n"; 
    }

    public void Check()
    {
      UpdateResult = "";
      DownloadLink = "";
      DownloadFilename = "";
      LogUpdateInfo("checking for update");

      Task.Run(async () =>
      {
        // GitHub repository information
        string owner = "Runinho";
        string repo = "MusikMacher";

        // Your application version saved during compile time

        // Create HttpClient
        using (var httpClient = new HttpClient())
        {
          // Set GitHub API base address
          httpClient.BaseAddress = new Uri("https://api.github.com/");

          // Set user-agent header (required by GitHub API)
          httpClient.DefaultRequestHeaders.Add("User-Agent", "MusikMacher");

          try
          {
            LogUpdateInfo("requesting current version information from github.com");
            // Make GET request to get latest release information
            HttpResponseMessage response = await httpClient.GetAsync($"repos/{owner}/{repo}/releases/latest");

            // Check if request was successful
            if (response.IsSuccessStatusCode)
            {
              LogUpdateInfo("got 200 OK");
              // Read response content
              string responseBody = await response.Content.ReadAsStringAsync();

              // Deserialize JSON response to Release object
              var release = JsonConvert.DeserializeObject<Release>(responseBody);
              LogUpdateInfo("deserialized");

              // Extract latest version string
              string latestVersion = release.tag_name;

              // Compare versions
              if (latestVersion == VERSION)
              {
                LogUpdateInfo($"You are using the latest version '{latestVersion}'!");
              }
              else
              {
                LogUpdateInfo($"A newer version is available ({latestVersion}) you have {VERSION}!");
                bool foundLink = false;
                foreach(var asset in release.assets)
                {
                  if (asset.name.EndsWith("-win-x64.zip"))
                  {
                    DownloadLink = asset.browser_download_url;
                    DownloadFilename = asset.name;
                    foundLink = true;
                    break;
                  }
                }
                if (!foundLink)
                {
                  // we send the user to the default page
                  DownloadLink = $"https://github.com/{owner}/{repo}/releases/latest";
                }
              }
            }
            else
            {
              LogUpdateInfo($"Failed to retrieve release information. Status code: {response.StatusCode}");
            }
          }
          catch (Exception ex)
          {
            LogUpdateInfo($"An error occurred: {ex.Message}");
          }
        }
      });
    }
  }
  // Define class to represent GitHub release object

  public class Asset
  {
    public string browser_download_url;
    public string name;
  }
  public class Release
  {
    public string tag_name { get; set; }
    public List<Asset> assets { get; set; }
  }
}
