using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MusikMacher.dialog
{
  /// <summary>
  /// Interaction logic for PreloadWaveformsWindow.xaml
  /// </summary>
  public partial class PreloadWaveformsWindow : FluentWindow
  {
    private PreloadWaveformsViewModel _model;

    public PreloadWaveformsWindow()
    {
      InitializeComponent();
      _model = new PreloadWaveformsViewModel();
      DataContext = _model;
      Closed += Window_Closed;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Console.WriteLine("Preload window closed");
      _model.Cancle();
    }
  }
}
