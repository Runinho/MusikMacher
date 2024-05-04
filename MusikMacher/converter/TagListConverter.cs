using LorusMusikMacher.database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusikMacher.converter
{
  public class TagListConverter : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is List<Tag> tags)
      {
        // concatenate the tag names to one string
        string s = "";
        int i = 0;
        foreach (Tag t in tags)
        {
          if (t.IsHidden)
          {
            // skip this one
            continue;
          }
          if(i > 0){
             s += ", ";
          }
          s += t.Name;
          i++;
        }
        return s;
      }
      return ""; // Default value if conversion fails
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException(); // We don't need this in this case
    }
  }
}
