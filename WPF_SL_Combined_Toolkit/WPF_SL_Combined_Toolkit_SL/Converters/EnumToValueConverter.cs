using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using WPF_SL_Combined_Toolkit.Converters.Base;

[assembly: XmlnsDefinition("urn:wpfsl-combined-toolkit", "WPF_SL_Combined_Toolkit.Converters")]

namespace WPF_SL_Combined_Toolkit.Converters
{
    public class EnumValue<T>
    {
        public string EnumString { get; set; }
        public T Value { get; set; }
    }

    [ContentProperty("EnumValues")]
    public class EnumToValueConverter<T> : ValueConverter
    {
        public Type EnumType { get; set; }
        public T DefaultValue { get; set; }

        private ObservableCollection<EnumValue<T>> _enumValues = new ObservableCollection<EnumValue<T>>();
        public IList EnumValues
        {
            get { return this._enumValues; }            
        }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return DefaultValue;

            if (EnumType != null && value.GetType() != EnumType)
            {
                if (value is string)
                    value = System.Enum.Parse(EnumType, value.ToString(), true);
                else
                    value = System.Enum.ToObject(EnumType, value);
            }

            foreach (EnumValue<T> enumValue in this.EnumValues)
            {
                if (enumValue.EnumString == value.ToString())
                    return enumValue.Value;
            }

            return DefaultValue;          
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToBrushConverter : EnumToValueConverter<Brush> { } ;
    public class EnumValueBrush : EnumValue<Brush> { } ;

    public class EnumToColorConverter : EnumToValueConverter<Color> { } ;
    public class EnumValueColor : EnumValue<Color> { } ;

    public class EnumToVisibilityConverter : EnumToValueConverter<Visibility> { } ;
    public class EnumValueVisibility : EnumValue<Visibility> { } ;
}
