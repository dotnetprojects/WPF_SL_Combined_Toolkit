using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPF_SL_Combined_Toolkit.Controls.Animation
{
    public static class AnimationManager
    {
        internal static readonly DependencyProperty AnimationProperty = DependencyProperty.RegisterAttached(
            "Animation", typeof (WeakReference), typeof (AnimationManager), null);

#if SILVERLIGHT
        public static readonly DependencyProperty AnimationSelectorProperty =
                    DependencyProperty.RegisterAttached(
                        "AnimationSelector", typeof(AnimationSelectorBase), typeof(AnimationManager),
                        new PropertyMetadata(null, OnAnimationSelectorChanged));
#else
        public static readonly DependencyProperty AnimationSelectorProperty =
            DependencyProperty.RegisterAttached(
                "AnimationSelector", typeof (AnimationSelectorBase), typeof (AnimationManager),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.Inherits, OnAnimationSelectorChanged, null));
#endif

        internal static readonly DependencyProperty CallbacksProperty = DependencyProperty.RegisterAttached(
            "Callbacks", typeof (ICollection<Action>), typeof (AnimationManager), null);

        private static double _globalSpeedRatio = 1.0;
        private static bool _isAnimationEnabled = true;

#if SILVERLIGHT
        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsAnimationEnabled", typeof(bool), typeof(AnimationManager),
                new PropertyMetadata(true));
#else
        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsAnimationEnabled", typeof (bool), typeof (AnimationManager),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
#endif

        public static double AnimationSpeedRatio
        {
            get { return _globalSpeedRatio; }
            set { _globalSpeedRatio = value; }
        }

        public static bool IsGlobalAnimationEnabled
        {
            get { return _isAnimationEnabled; }
            set { _isAnimationEnabled = value; }
        }


        internal static void AddCallback(Storyboard storyboard, Action callback)
        {
            ICollection<Action> callbacks = GetCallbacks(storyboard);
            if (callbacks == null)
            {
                SetCallbacks(storyboard, new List<Action> {callback});
            }
            else
            {
                callbacks.Add(callback);
            }
        }

        internal static AnimationBase GetAnimation(DependencyObject obj)
        {
            var reference = obj.GetValue(AnimationProperty) as WeakReference;
            if (reference == null)
            {
                return null;
            }
            return (reference.Target as AnimationBase);
        }

        public static AnimationSelectorBase GetAnimationSelector(DependencyObject obj)
        {
            return (AnimationSelectorBase) obj.GetValue(AnimationSelectorProperty);
        }

        private static ICollection<Action> GetCallbacks(DependencyObject obj)
        {
            return (obj.GetValue(CallbacksProperty) as ICollection<Action>);
        }

        public static bool GetIsAnimationEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsAnimationEnabledProperty);
        }

        internal static void InvokeCallbacks(Storyboard storyboard)
        {
            if (storyboard != null)
            {
                ICollection<Action> callbacks = GetCallbacks(storyboard);
                foreach (Storyboard storyboard2 in
                    storyboard.Children.OfType<Storyboard>())
                {
                    InvokeCallbacks(storyboard2);
                }
                if (callbacks != null)
                {
                    foreach (Action action in callbacks)
                    {
                        action();
                    }
                    callbacks.Clear();
                }
            }
        }

        private static void OnAnimationSelectorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ItemsControl;
            if (control != null)
            {
                foreach (
                    FrameworkElement element in
                        (from object obj2 in control.Items select control.ItemContainerGenerator.ContainerFromItem(obj2))
                            .OfType<FrameworkElement>())
                {
                    SetAnimationSelector(element, e.NewValue as AnimationSelectorBase);
                }
            }
        }

        private static void OnStoryboardCompleted(object sender, EventArgs e)
        {
#if !SILVERLIGHT
            var group = sender as ClockGroup;
            if (group != null)
            {
                InvokeCallbacks(group.Timeline as Storyboard);
            }
#endif
        }

        public static bool Play(Control target, string animationName)
        {
            return Play(target, animationName, null, new object[0]);
        }

        public static bool Play(Control target, string animationName, Action completeCallback, params object[] args)
        {
            if ((!GetIsAnimationEnabled(target) || !IsGlobalAnimationEnabled) ||
                (VisualTreeHelper.GetChildrenCount(target) <= 0))
            {
                if (completeCallback != null)
                {
                    completeCallback();
                }
                return false;
            }
            var child = VisualTreeHelper.GetChild(target, 0) as FrameworkElement;
            if (child != null)
            {
                var storyboard = child.Resources[animationName] as Storyboard;
                if (storyboard == null)
                {
                    var animationSelector = GetAnimationSelector(target);
                    if (animationSelector == null)
                    {
                        if (completeCallback != null)
                        {
                            completeCallback();
                        }
                        return false;
                    }
                    var animation = animationSelector.SelectAnimation(target, animationName);
                    if (animation == null)
                    {
                        if (completeCallback != null)
                        {
                            completeCallback();
                        }
                        return false;
                    }
                    storyboard = animation.CreateAnimation(target);
                    if (storyboard == null)
                    {
                        if (completeCallback != null)
                        {
                            completeCallback();
                        }
                        return false;
                    }
                    storyboard.Completed += OnStoryboardCompleted;
                    SetAnimation(storyboard, animation);
                    child.Resources.Add(animationName, storyboard);
                }
                AnimationBase animation2 = GetAnimation(storyboard);
                if (completeCallback != null)
                {
                    AddCallback(storyboard, completeCallback);
                }
                if (storyboard.Children.Count > 0)
                {
                    animation2.UpdateAnimation(target, storyboard, args);
                }
                storyboard.Begin();
            }
            return true;
        }

        internal static void SetAnimation(DependencyObject obj, AnimationBase value)
        {
            obj.SetValue(AnimationProperty, value == null ? null : new WeakReference(value));
        }

        public static void SetAnimationSelector(DependencyObject obj, AnimationSelectorBase value)
        {
            obj.SetValue(AnimationSelectorProperty, value);
        }

        private static void SetCallbacks(DependencyObject obj, ICollection<Action> value)
        {
            obj.SetValue(CallbacksProperty, value);
        }

        public static void SetIsAnimationEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAnimationEnabledProperty, value);
        }

        public static void Stop(FrameworkElement target, string animationName)
        {
            var storyboard = target.Resources[animationName] as Storyboard;
            if ((storyboard != null) && (storyboard.GetCurrentState() != ClockState.Stopped))
            {
                storyboard.Stop();
            }
            InvokeCallbacks(storyboard);
        }
    }
}