using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusikMacher.converter
{
  class HiddenToSymbolConverter : IValueConverter
  {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
      bool isHidden = (bool)value;
        return isHidden ? "EyeOff16" : "Eye16"; // values[2] is "unhide" string, values[1] is "hide" string
      }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}
