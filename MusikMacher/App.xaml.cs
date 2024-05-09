using System.Configuration;
using System.Data;
using System.Windows;

namespace MusikMacher
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App() : base()
    {
      var language = Settings.getSettings().Language;
      if (language == "")
      {
        var name = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        string[] cultures = { "de-DE", "en-US" };
        if (cultures.Contains(name))
        {
          language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        }
        else
        {
          language = "en-US"; // fallback and default
        }
        Settings.getSettings().Language = language;
      }
      System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message + " Stack trace:" + e.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

}
