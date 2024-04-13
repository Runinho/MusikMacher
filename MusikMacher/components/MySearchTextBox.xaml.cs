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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusikMacher.components
{
  /// <summary>
  /// Interaction logic for SearchTextBox.xaml
  /// </summary>
  public partial class MySearchTextBox : UserControl
  {
    public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(MySearchTextBox), new PropertyMetadata(""));

    public MySearchTextBox()
    {
      InitializeComponent();
      LayoutRoot.DataContext = this;
    }

    public string Placeholder { get; set; }
    public string Value
    {
      get { return (string)GetValue(ValueProperty); }
      set {
        System.Diagnostics.Debug.WriteLine($"changed search: {value}");
        SetValue(ValueProperty, value); }
    }

    public void FocusAndSelect()
    {
      SearchTermTextBox.Focus();
      SearchTermTextBox.SelectAll();
    }

    public bool IsTextBoxFocus()
    {
      return SearchTermTextBox.IsFocused;
    }
  }
}
