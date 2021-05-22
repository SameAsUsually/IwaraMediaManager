using MahApps.Metro.IconPacks;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace IwaraMediaManager.WPF.Converters
{
    public class BoolToOrderSortOrderIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var asc = value as bool?;

            if (asc != null)
                return asc.Value ? PackIconBoxIconsKind.RegularSortUp : PackIconBoxIconsKind.RegularSortDown;
            else 
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
