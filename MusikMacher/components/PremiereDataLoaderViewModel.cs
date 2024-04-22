using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace MusikMacher.components
{
  public class PremiereDataLoaderViewModel : ViewModelBase
  {
    public PremiereDataLoaderViewModel()
    {
      LoadPremiereDataCommand = new RelayCommand(LoadPremiereData);
    }

    private void Log(string message)
    {
      Console.WriteLine($"Premiere log: {message}");
      PremiereLog += message + "\n";
    }
    
    private void LoadPremiereData()
    {
      PremiereLog = $"starting data load from {PremiereFilePath}\n";
      Task.Run(()=>{
        PremiereFileReader.ReadFile(Log, PremiereFilePath);
      });
    }

    public ICommand LoadPremiereDataCommand { get; set; }

    private string _premiereLog = "";
    public string PremiereLog
    {
      get => _premiereLog;
      set
      {
        if (value != _premiereLog)
        {
          _premiereLog = value;
          RaisePropertyChanged(nameof(PremiereLog));
        }
      }
    }

    private string _premiereFilePath;
    public string PremiereFilePath
    {
      get => _premiereFilePath; 
      set
      {
        if (value != _premiereFilePath)
        {
          _premiereFilePath = value;
          RaisePropertyChanged(nameof(PremiereFilePath));
        }
      }
    }


  }
}