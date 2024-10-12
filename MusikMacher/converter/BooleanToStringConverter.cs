using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusikMacher.converter
{
  public class BooleanToStringConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length < 3 || !(values[0] is bool))
        return values[1]; // Default to the "hide" string if something's wrong

      bool isHidden = (bool)values[0];
      return isHidden ? values[2] : values[1]; // values[2] is "unhide" string, values[1] is "hide" string
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
