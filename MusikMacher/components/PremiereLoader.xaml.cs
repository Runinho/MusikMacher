using System.Windows.Controls;

namespace MusikMacher.components;

public partial class PremiereLoader : UserControl
{
  public PremiereLoader()
  {
      InitializeComponent();
      DataContext = new PremiereDataLoaderViewModel();
  }

  private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    // Scroll to the bottom of the TextBox
    textBox.ScrollToEnd();
  }
}