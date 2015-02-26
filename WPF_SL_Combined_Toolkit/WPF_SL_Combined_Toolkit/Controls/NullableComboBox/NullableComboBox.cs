using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

[assembly: XmlnsDefinition("urn:wpfsl-combined-toolkit", "WPF_SL_Combined_Toolkit.Controls.NullableComboBox")]

namespace WPF_SL_Combined_Toolkit.Controls.NullableComboBox
{
    [TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]

    public class NullableComboBox : ComboBox
    {
#if !SILVERLIGHT
        static NullableComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableComboBox), new FrameworkPropertyMetadata(typeof(NullableComboBox)));
        }
#endif

        public NullableComboBox()
        {
#if SILVERLIGHT
            DefaultStyleKey = typeof(NullableComboBox);
#endif
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var btn = GetTemplateChild("PART_ClearButton") as Button;

            btn.Click += btn_Click;
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            var clearButton = (Button)sender;
            var parent = VisualTreeHelper.GetParent(clearButton);

            while (!(parent is ComboBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var comboBox = (ComboBox)parent;
            //clear the selection
            comboBox.SelectedIndex = -1;
        }



        public bool IsNullable
        {
            get { return (bool)GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        public static readonly DependencyProperty IsNullableProperty =
            DependencyProperty.Register("IsNullable", typeof(bool), typeof(NullableComboBox), new PropertyMetadata(true));        
    }
}
