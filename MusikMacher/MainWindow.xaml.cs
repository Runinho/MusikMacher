using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusikMacher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
      /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
    MainWindowModel model = new MainWindowModel();

    public MainWindow()
    {
      InitializeComponent();
      DataContext = model;
    }

    private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      // can this also be done in the binding???
      model.TrackPreviewMouseLeftButtonDown(e);
    }

    private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      model.TrackPreviewMouseMove(sender, e);
    }
  }
}