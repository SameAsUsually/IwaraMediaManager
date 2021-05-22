using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace IwaraMediaManager.WPF.Converters
{
    public class AutoSuggestTextChangedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var args = (AutoSuggestBoxTextChangedEventArgs)value;

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                return (string)parameter;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
