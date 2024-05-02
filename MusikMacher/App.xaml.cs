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
      this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message + " Stack trace:" + e.Exception.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

}
