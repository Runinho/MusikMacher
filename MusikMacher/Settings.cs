using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusikMacher
{

  // persistant Settings storage
  internal class Settings
  {
    private const string FilePath = "settings.json";
    private static Settings? Instance;

    [DefaultValue(0.5)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double Volume;

    [DefaultValue("C:/some/path")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string ImportPath = "C:/some/path";

    [DefaultValue(false)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public bool ANDTagCombination;


    [DefaultValue(200)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double MainWindowLeft;

    [DefaultValue(200)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double MainWindowTop;

    [DefaultValue(1000)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double MainWindowWidth;

    [DefaultValue(700)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double MainWindowHeight;

    [DefaultValue(200)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double DialogLeft;

    [DefaultValue(200)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double DialogTop;

    [DefaultValue(false)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public bool ImportSubfolders;

    [DefaultValue(0.3)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public double SkipPosition;

    [DefaultValue("")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    public string Search = "";

    public static Settings getSettings()
    {
      if (Instance == null)
      {
        System.Diagnostics.Debug.WriteLine("loading settings");
        string json = "{}"; // empty json -> use default values.
        if (File.Exists(FilePath))
        {
          json = File.ReadAllText(FilePath);
        }
        Instance = JsonConvert.DeserializeObject<Settings>(json);
        if(Instance == null)
        {
          // call constructor? But should not happen
          Instance = new Settings();
        }
      }
      return Instance;
    }

    public static void saveSettings()
    {
      string json = JsonConvert.SerializeObject(Instance);
      File.WriteAllText(FilePath, json);
      System.Diagnostics.Debug.WriteLine("saved settings");
    }
  }
}
