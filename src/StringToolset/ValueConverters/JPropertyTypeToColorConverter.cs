using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace StringToolset.ValueConverters
{
    public sealed class JPropertyTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JProperty jProp)
            {
                switch (jProp.Value.Type)
                {
                    case JTokenType.String:
                        return new BrushConverter().ConvertFrom("#4e9a06");
                    case JTokenType.Float:
                    case JTokenType.Integer:
                        return new BrushConverter().ConvertFrom("#ad7fa8");
                    case JTokenType.Boolean:
                        return new BrushConverter().ConvertFrom("#c4a000");
                    case JTokenType.Null:
                        return new SolidColorBrush(Colors.OrangeRed);
                    default:
                        return new BrushConverter().ConvertFrom("#efefef");
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
