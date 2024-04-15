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

namespace MusikMacher
{
  /// <summary>
  /// Interaction logic for NewTagDialog.xaml
  /// </summary>
  public partial class NewTagDialog : FluentWindow
  {
    public NewTagDialog()
    {
      InitializeComponent();
      this.Loaded += Dialog_Loaded;
      this.Closed += Window_Closed;

      Settings settings = Settings.getSettings();
      Left = settings.DialogLeft;
      Top = settings.DialogTop;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      // Save the loaction of the window
      Settings settings = Settings.getSettings();

      settings.DialogLeft = Left;
      settings.DialogTop = Top;
      Settings.saveSettings(); // maybee this is enough and we can remove all the other saves?
    }

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
      textBoxTagName.Focus();
    }
  }
}
