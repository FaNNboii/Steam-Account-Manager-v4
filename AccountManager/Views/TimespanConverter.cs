using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AccountManager.Views
{
    class TimespanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                TimeSpan t = (TimeSpan)value;
                return (t.Days * 24 + t.Hours).ToString().PadLeft(2, '0') + ":" + t.Minutes.ToString().PadLeft(2, '0') + ":" + t.Seconds.ToString().PadLeft(2, '0');
            }
            else return "Error";

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
