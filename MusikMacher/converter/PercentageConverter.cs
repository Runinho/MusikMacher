using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusikMacher.converter
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                return number.ToString("P", CultureInfo.InvariantCulture);
            }

            if (value is float numberFloat)
            {
                return numberFloat.ToString("P", CultureInfo.InvariantCulture);
            }

            return "0%";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need this in this case
        }
    }
}
