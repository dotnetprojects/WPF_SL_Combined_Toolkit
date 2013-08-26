using System.Windows.Controls;

namespace WPF_SL_Combined_Toolkit.Controls.Animation
{
    public abstract class AnimationSelectorBase
    {
        public abstract AnimationBase SelectAnimation(Control control, string name);
    }
}