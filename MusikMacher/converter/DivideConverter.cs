using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MusikMacher.converter
{
  public class DivideConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values[0] != DependencyProperty.UnsetValue)
      {

        if (values.Length != 2)
          throw new ArgumentException("Two values are required for division.");

        double dividend = System.Convert.ToDouble(values[0]);
        double divisor = System.Convert.ToDouble(values[1]);

        if (divisor == 0)
          return 0;//DependencyProperty.UnsetValue;

        return dividend / divisor;
      }
      else
      {
        return 0;
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
