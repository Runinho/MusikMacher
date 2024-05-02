using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusikMacher.converter
{
    public class SecondsToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double s;
            if (value is double seconds)
            {
                s = seconds;
            }
            else if (value is int intSeconds)
            {
                s = intSeconds;
            }
            else
            {
                return "--:--"; // Default value if conversion fails
            }
            
            TimeSpan timeSpan = TimeSpan.FromSeconds(s);
            if (s > 60 * 60)
            {
                // more than 1 hour
                return timeSpan.ToString(@"hh\:mm\:ss");
            }
            return timeSpan.ToString(@"mm\:ss");

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need this in this case
        }
    }
}
