using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

[assembly: XmlnsDefinition("urn:wpfsl-combined-toolkit", "WPF_SL_Combined_Toolkit.Controls.EnumComboBox")]

namespace WPF_SL_Combined_Toolkit.Controls.EnumComboBox
{
    public class EnumComboBox : NullableComboBox.NullableComboBox
    {
        public EnumComboBox()
        {
            IsNullable = false;
        }

        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }

        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumComboBox), new PropertyMetadata(OnEnumTypeChanged));

        private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tp = (Type) e.NewValue;
            var ctl = d as EnumComboBox;
            ctl.ItemsSource = Enum.GetValues(tp);
        }                
    }
}
