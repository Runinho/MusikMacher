using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualBasic.Logging;
using MusikMacher.Resources;
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
    public static string VERSION = "v0.1.16-alpha";

    public string Version
    {
      get { return VERSION; }
    }

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


    private DateTime? _lastVersionCheck;
    public DateTime? LastVersionCheck
    {
      get { return _lastVersionCheck; }
      set
      {
        if (_lastVersionCheck != value)
        {
          _lastVersionCheck = value;
          RaisePropertyChanged(nameof(LastVersionCheck));
        }
      }
    }

    private string _checkResultMessage;
    public string CheckResultMessage
    {
      get { return _checkResultMessage; }
      set
      {
        if (_checkResultMessage != value)
        {
          _checkResultMessage = value;
          RaisePropertyChanged(nameof(CheckResultMessage));
        }
      }
    }

    private UpdateCheckState _updateCheckState = UpdateCheckState.Unkown;
    public UpdateCheckState UpdateCheckState
    {
      get { return _updateCheckState; }
      set
      {
        if (_updateCheckState != value)
        {
          _updateCheckState = value;
          RaisePropertyChanged(nameof(UpdateCheckState));
        }
      }
    }


    public ICommand CheckCommand { get; private set; }

    public CheckUpdateViewModel()
    {
      CheckCommand = new RelayCommand(Check);

      var settings = Settings.getSettings();

      LastVersionCheck = Settings.LastVersionCheck;
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
                UpdateCheckState = UpdateCheckState.UpToDate;
                CheckResultMessage = Strings.UpToDate;
                LogUpdateInfo(CheckResultMessage);
              }
              else
              {
                UpdateCheckState = UpdateCheckState.NewVersion;
                var test = Strings.NewVersionAvailable;
                CheckResultMessage = String.Format(Strings.NewVersionAvailable, latestVersion, VERSION);
                LogUpdateInfo(CheckResultMessage);
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

              // update last check time
              LastVersionCheck = DateTime.Now;
              Settings.LastVersionCheck = LastVersionCheck;
              Settings.saveSettings();
            }
            else
            {
              UpdateCheckState = UpdateCheckState.Failed;
              CheckResultMessage = String.Format(Strings.FailedToRetrieve, response.StatusCode);
              LogUpdateInfo(CheckResultMessage);
            }
          }
          catch (Exception ex)
          {
            UpdateCheckState = UpdateCheckState.Failed;
            CheckResultMessage = String.Format(Strings.ErrorOccured, ex.Message);
            LogUpdateInfo(CheckResultMessage);
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
