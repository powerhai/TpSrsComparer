using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TpSrsComparer.Domain;
namespace TpSrsComparer.Converters
{
    public class ComparedTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ComparedType)value)
            {
                case ComparedType.All:
                    return "SRS Matched TP";
                case ComparedType.OnlyLeft:
                    return "Only SRS";
                case ComparedType.OnlyRight:
                    return "Only TP";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
