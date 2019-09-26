using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

[assembly: XmlnsDefinition("urn:wpfsl-combined-toolkit", "WPF_SL_Combined_Toolkit.Behaviors")]

namespace WPF_SL_Combined_Toolkit.Behaviors
{
    public class TextUpdateBindingBehavior : Behavior<FrameworkElement>
    {
        protected bool _updateBindingOnEverykeyPress = false;
        public bool UpdateBindingOnEverykeyPress
        {
            get { return this._updateBindingOnEverykeyPress; }
            set { this._updateBindingOnEverykeyPress = value; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.KeyUp += new KeyEventHandler(this.AssociatedObject_KeyUp);
            this.AssociatedObject.LostFocus += new RoutedEventHandler(this.AssociatedObject_LostFocus);
        }

        void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || this._updateBindingOnEverykeyPress)
            {
                BindingExpression be = null;
                if (this.AssociatedObject is TextBox)
                    be = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);                
                if (be != null)
                    be.UpdateSource();
            }
        }

        void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            BindingExpression be = null;
            if (this.AssociatedObject is TextBox)
                be = this.AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            if (be != null)
                be.UpdateSource();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.KeyUp -= new KeyEventHandler(this.AssociatedObject_KeyUp);
            this.AssociatedObject.LostFocus -= new RoutedEventHandler(this.AssociatedObject_LostFocus);

            base.OnDetaching();
        }        
    }
}