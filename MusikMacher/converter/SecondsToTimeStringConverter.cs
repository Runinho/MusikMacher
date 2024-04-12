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
            if (value is double seconds)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
                return timeSpan.ToString(@"mm\:ss");
            }
            if (value is int intSeconds)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(intSeconds);
                return timeSpan.ToString(@"mm\:ss");
            }

            return "--:--"; // Default value if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // We don't need this in this case
        }
    }
}
