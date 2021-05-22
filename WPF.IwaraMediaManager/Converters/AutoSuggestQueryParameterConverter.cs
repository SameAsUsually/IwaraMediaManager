using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace IwaraMediaManager.WPF.Converters
{
    public class AutoSuggestQueryParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var args = (AutoSuggestBoxQuerySubmittedEventArgs)value;
            return (string)args.QueryText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
