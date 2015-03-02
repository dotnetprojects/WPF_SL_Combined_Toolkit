using System;
using System.Windows;
using WPF_SL_Combined_Toolkit.Converters.Base;

namespace MCC.ControlPanel.SharedClasses.Converters
{
    public class NullToValueConverter<T> : ValueConverter
    {
        public T NullValue { get; set; }
        public T NotNullValue { get; set; }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return NullValue;
            else
                return NotNullValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class NullToVisibileConverter : NullToValueConverter<Visibility>
    {
        private static readonly Lazy<ValueConverter> _instance = new Lazy<ValueConverter>(() => new NullToVisibileConverter());
        public static ValueConverter Instance { get { return _instance.Value; } }

        public NullToVisibileConverter()
        {
            NullValue = Visibility.Visible;
            NotNullValue = Visibility.Collapsed;
        }
    }

    public class NullToCollapsedConverter : NullToValueConverter<Visibility>
    {
        private static readonly Lazy<ValueConverter> _instance = new Lazy<ValueConverter>(() => new NullToCollapsedConverter());
        public static ValueConverter Instance { get { return _instance.Value; } }

        public NullToCollapsedConverter()
        {
            NullValue = Visibility.Collapsed;
            NotNullValue = Visibility.Visible;
        }
    }    
}