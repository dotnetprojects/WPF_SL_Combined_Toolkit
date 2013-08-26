using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPF_SL_Combined_Toolkit.ExtensionMethods;

namespace WPF_SL_Combined_Toolkit.Controls.Animation
{
    public abstract class AnimationBase
    {
        internal static readonly double PixelsPerSecond = 500.0;

        public string AnimationName { get; set; }

        public double SpeedRatio { get; set; }

        public abstract Storyboard CreateAnimation(Control control);

        internal static FrameworkElement FindTarget(Control control, string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                return control;
            }
            if (VisualTreeHelper.GetChildrenCount(control) > 0)
            {
                var child = VisualTreeHelper.GetChild(control, 0) as FrameworkElement;
                if (child != null)
                {
                    return (child.FindName(targetName) as FrameworkElement);
                }
            }
            return null;
        }

        internal static double GetDurationSecondsForLength(double pixelsLength)
        {
            double num = pixelsLength/PixelsPerSecond;
            return Math.Max(num, 0.2);
        }

        public virtual void UpdateAnimation(Control control, Storyboard storyboard, params object[] args)
        {
            storyboard.SpeedRatio = !DoubleUtil.IsZero(SpeedRatio)
                                        ? SpeedRatio
                                        : AnimationManager.AnimationSpeedRatio;
        }
    }
}