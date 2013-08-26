using System.Windows;
using System.Windows.Media.Animation;

namespace WPF_SL_Combined_Toolkit.Controls.Animation
{
    internal class Easings
    {
        private static IEasingFunction _slideDown1;
        private static Easings _slideDown2;
        private static Easings _slideDown3;
        private static IEasingFunction _slideUp1;
        private static Easings _slideUp2;
        private static Easings _slideUp3;


        internal Easings(double x1, double y1, double x2, double y2)
        {
            Point1 = new Point(x1, y1);
            Point2 = new Point(x2, y2);
        }

        // Properties
        internal Point Point1 { get; set; }

        internal Point Point2 { get; set; }

        internal static IEasingFunction SlideDown1
        {
            get { return _slideDown1 ?? (_slideDown1 = new QuadraticEase()); }
        }

        internal static Easings SlideDown2
        {
            get { return _slideDown2 ?? (_slideDown2 = new Easings(0.264, 0.0, 0.228, 1.0)); }
        }

        internal static Easings SlideDown3
        {
            get { return _slideDown3 ?? (_slideDown3 = new Easings(0.02, 0.196, 0.362, 1.0)); }
        }

        internal static IEasingFunction SlideUp1
        {
            get { return _slideUp1 ?? (_slideUp1 = new QuadraticEase()); }
        }

        internal static Easings SlideUp2
        {
            get { return _slideUp2 ?? (_slideUp2 = new Easings(0.224, 0.0, 0.0, 1.0)); }
        }

        internal static Easings SlideUp3
        {
            get { return _slideUp3 ?? (_slideUp3 = new Easings(0.0, 0.116, 0.431, 1.0)); }
        }
    }
}