using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPF_SL_Combined_Toolkit.Controls.Animation
{
    internal static class AnimationExtensions
    {
        internal static AnimationContext AdjustSpeed(this AnimationContext target)
        {
            target.Instance.SpeedRatio = AnimationManager.AnimationSpeedRatio;
            return target;
        }

        internal static AnimationContext Animate(this AnimationContext target, params FrameworkElement[] newTargets)
        {
            target.Targets.Clear();
            foreach (FrameworkElement element in newTargets)
            {
                target.Targets.Add(element);
            }
            return target;
        }

        internal static AnimationContext Create()
        {
            return new AnimationContext();
        }

        internal static AnimationContext Discrete(
            this AnimationContext target, DependencyProperty propertyPath, params object[] args)
        {
            List<object> list = args.ToList();
            if ((args.Length%2) != 0)
            {
                throw new InvalidOperationException("Params should come in a time-value pair");
            }
            target.StartIndex = target.EndIndex;
            target.EndIndex += target.Targets.Count;
            int num = 0;
            foreach (FrameworkElement element in target.Targets)
            {
                if (target.IsUpdate)
                {
                    var frames = target.Instance.Children[target.StartIndex + num] as ObjectAnimationUsingKeyFrames;
                    if (frames != null)
                    {
                        for (int j = 0; j < list.Count; j += 2)
                        {
                            var frame = frames.KeyFrames[j/2] as DiscreteObjectKeyFrame;
                            if (frame != null)
                            {
                                frame.KeyTime =
                                    KeyTime.FromTimeSpan(
                                        TimeSpan.FromSeconds(Convert.ToDouble(list[j], CultureInfo.InvariantCulture)));
                                frame.Value = list[j + 1];
                            }
                        }
                    }
                    num++;
                    continue;
                }
                var frames2 = new ObjectAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames2, element);
                Storyboard.SetTargetProperty(frames2, new PropertyPath(propertyPath));
                for (int i = 0; i < list.Count; i += 2)
                {
                    frames2.KeyFrames.Add(
                        new DiscreteObjectKeyFrame
                            {
                                KeyTime =
                                    KeyTime.FromTimeSpan(
                                        TimeSpan.FromSeconds(Convert.ToDouble(list[i], CultureInfo.InvariantCulture))),
                                Value = list[i + 1]
                            });
                }
                target.Instance.Children.Add(frames2);
            }
            return target;
        }

        internal static AnimationContext EaseAll(this AnimationContext target, IEasingFunction easing)
        {
            for (int i = target.StartIndex; i < target.EndIndex; i++)
            {
                var frames = target.Instance.Children[i] as DoubleAnimationUsingKeyFrames;
                if (frames != null)
                {
                    foreach (EasingDoubleKeyFrame frame in
                        frames.KeyFrames.Cast<EasingDoubleKeyFrame>())
                    {
                        frame.EasingFunction = easing;
                    }
                }
            }
            return target;
        }

        internal static AnimationContext Easings(this AnimationContext target, IEasingFunction easing)
        {
            return target.Easings(1, easing);
        }

        internal static AnimationContext Easings(this AnimationContext target, int index, IEasingFunction easing)
        {
            for (int i = target.StartIndex; i < target.EndIndex; i++)
            {
                var frames = target.Instance.Children[i] as DoubleAnimationUsingKeyFrames;
                if (frames != null)
                {
                    var frame = frames.KeyFrames[index] as EasingDoubleKeyFrame;
                    if (frame != null)
                    {
                        frame.EasingFunction = easing;
                    }
                }
            }
            return target;
        }

        internal static AnimationContext EnsureDefaultTransforms(this AnimationContext target)
        {
            foreach (FrameworkElement element in target.Targets)
            {
                element.RenderTransform = new TransformGroup
                                              {
                                                  Children =
                                                      {
                                                          new ScaleTransform(),
                                                          new SkewTransform(),
                                                          new RotateTransform(),
                                                          new TranslateTransform()
                                                      }
                                              };
            }
            return target;
        }

        internal static AnimationContext EnsureOpacityMask(this AnimationContext target)
        {
            foreach (FrameworkElement element in target.Targets)
            {
                element.OpacityMask = new LinearGradientBrush
                                          {
                                              EndPoint = new Point(0.0, 1.0),
                                              GradientStops =
                                                  {
                                                      new GradientStop {Offset = 0.0, Color = Colors.Transparent},
                                                      new GradientStop {Offset = 0.0, Color = Colors.Black},
                                                      new GradientStop {Offset = 1.0, Color = Colors.Black},
                                                      new GradientStop {Offset = 1.0, Color = Colors.Transparent}
                                                  },
                                              RelativeTransform = new TranslateTransform()
                                          };
            }
            return target;
        }

        internal static AnimationContext Height(this AnimationContext target, params double[] args)
        {
            return target.SingleProperty("(FrameworkElement.Height)", args);
        }

        internal static AnimationContext HoldEndFillBehavior(this AnimationContext target)
        {
            target.Instance.FillBehavior = FillBehavior.HoldEnd;
            return target;
        }

        internal static AnimationContext MoveX(this AnimationContext target, params double[] args)
        {
            return
                target.SingleProperty(
                    "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)", args);
        }

        internal static AnimationContext MoveY(this AnimationContext target, params double[] args)
        {
            return
                target.SingleProperty(
                    "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)", args);
        }

        internal static AnimationContext OnComplete(this AnimationContext target, Action callback)
        {
            if (target.Instance != null)
            {
                EventHandler completeHandler = null;
                EventHandler handler = completeHandler;
                completeHandler = (s, e) =>
                                      {
                                          callback();
                                          target.Instance.Completed -= handler;
                                      };
                target.Instance.Completed += completeHandler;
            }
            return target;
        }

        internal static AnimationContext Opacity(this AnimationContext target, params double[] args)
        {
            return target.SingleProperty("(UIElement.Opacity)", args);
        }

        internal static AnimationContext OpacityMaskRelativeMoveY(this AnimationContext target, params double[] args)
        {
            return target.SingleProperty(
                "(UIElement.OpacityMask).(Brush.RelativeTransform).(TranslateTransform.Y)", args);
        }

        internal static AnimationContext Origin(this AnimationContext target, double x1, double x2)
        {
            foreach (FrameworkElement element in target.Targets)
            {
                element.RenderTransformOrigin = new Point(x1, x2);
            }
            return target;
        }

        internal static AnimationContext PlayIfPossible(this AnimationContext target, Control hostControl)
        {
            if (AnimationManager.IsGlobalAnimationEnabled && AnimationManager.GetIsAnimationEnabled(hostControl))
            {
                target.Instance.Begin();
            }
            return target;
        }

        internal static AnimationContext RotationY(this AnimationContext target, params double[] args)
        {
            return target.SingleProperty("(UIElement.Projection).(PlaneProjection.RotationY)", args);
        }

        internal static AnimationContext Scale(this AnimationContext target, params double[] args)
        {
            List<double> list = args.ToList();
            if ((args.Length%2) != 0)
            {
                throw new InvalidOperationException("Params should come in a time-value pair");
            }
            foreach (FrameworkElement element in target.Targets)
            {
                if (target.IsUpdate)
                {
                    throw new NotImplementedException();
                }
                var frames = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames, element);
                Storyboard.SetTargetProperty(
                    frames,
                    new PropertyPath(
                        "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)",
                        new object[0]));
                var frames2 = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames2, element);
                Storyboard.SetTargetProperty(
                    frames2,
                    new PropertyPath(
                        "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)",
                        new object[0]));
                for (int i = 0; i < list.Count; i += 2)
                {
                    frames.KeyFrames.Add(
                        new EasingDoubleKeyFrame
                            {KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[i])), Value = list[i + 1]});
                    frames2.KeyFrames.Add(
                        new EasingDoubleKeyFrame
                            {KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[i])), Value = list[i + 1]});
                }
                target.Instance.Children.Add(frames);
                target.Instance.Children.Add(frames2);
            }
            target.StartIndex = target.EndIndex;
            target.EndIndex += 2*target.Targets.Count;
            return target;
        }

        private static AnimationContext SingleProperty(
            this AnimationContext target, string propertyPath, params double[] args)
        {
            List<double> list = args.ToList();
            if ((args.Length%2) != 0)
            {
                throw new InvalidOperationException("Params should come in a time-value pair");
            }
            target.StartIndex = target.EndIndex;
            target.EndIndex += target.Targets.Count;
            int num = 0;
            foreach (FrameworkElement element in target.Targets)
            {
                if (target.IsUpdate)
                {
                    var frames = target.Instance.Children[target.StartIndex + num] as DoubleAnimationUsingKeyFrames;
                    if (frames != null)
                    {
                        for (int j = 0; j < list.Count; j += 2)
                        {
                            var frame = frames.KeyFrames[j/2] as EasingDoubleKeyFrame;
                            if (frame != null)
                            {
                                frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[j]));
                                frame.Value = list[j + 1];
                            }
                        }
                    }
                    num++;
                    continue;
                }
                var frames2 = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames2, element);
                Storyboard.SetTargetProperty(frames2, new PropertyPath(propertyPath, new object[0]));
                for (int i = 0; i < list.Count; i += 2)
                {
                    frames2.KeyFrames.Add(
                        new EasingDoubleKeyFrame
                            {KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[i])), Value = list[i + 1]});
                }
                target.Instance.Children.Add(frames2);
            }
            return target;
        }

        internal static AnimationContext SingleProperty(
            this AnimationContext target, DependencyProperty propertyPath, params double[] args)
        {
            List<double> list = args.ToList();
            if ((args.Length%2) != 0)
            {
                throw new InvalidOperationException("Params should come in a time-value pair");
            }
            target.StartIndex = target.EndIndex;
            target.EndIndex += target.Targets.Count;
            int num = 0;
            foreach (FrameworkElement element in target.Targets)
            {
                if (target.IsUpdate)
                {
                    var frames = target.Instance.Children[target.StartIndex + num] as DoubleAnimationUsingKeyFrames;
                    if (frames != null)
                    {
                        for (int j = 0; j < list.Count; j += 2)
                        {
                            var frame = frames.KeyFrames[j/2] as EasingDoubleKeyFrame;
                            if (frame != null)
                            {
                                frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[j]));
                                frame.Value = list[j + 1];
                            }
                        }
                    }
                    num++;
                    continue;
                }
                var frames2 = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(frames2, element);
                Storyboard.SetTargetProperty(frames2, new PropertyPath(propertyPath));
                for (int i = 0; i < list.Count; i += 2)
                {
                    frames2.KeyFrames.Add(
                        new EasingDoubleKeyFrame
                            {KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(list[i])), Value = list[i + 1]});
                }
                target.Instance.Children.Add(frames2);
            }
            return target;
        }

        internal static AnimationContext StopFillBehavior(this AnimationContext target)
        {
            target.Instance.FillBehavior = FillBehavior.Stop;
            return target;
        }

        public static AnimationContext Update(this Storyboard target)
        {
            return new AnimationContext {Instance = target, IsUpdate = true};
        }

        internal static AnimationContext Width(this AnimationContext target, params double[] args)
        {
            return target.SingleProperty("(FrameworkElement.Width)", args);
        }

        internal static AnimationContext With(this AnimationContext target, params FrameworkElement[] newElements)
        {
            foreach (FrameworkElement element in newElements)
            {
                target.Targets.Add(element);
            }
            return target;
        }

        internal static AnimationContext Without(this AnimationContext target, params FrameworkElement[] newElements)
        {
            foreach (FrameworkElement element in newElements)
            {
                target.Targets.Remove(element);
            }
            return target;
        }

        #region Nested type: AnimationContext

        internal class AnimationContext
        {
            public AnimationContext()
            {
                Instance = new Storyboard {FillBehavior = FillBehavior.HoldEnd};
                Targets = new List<FrameworkElement>(8);
            }

            // Properties
            public int EndIndex { get; set; }

            internal Storyboard Instance { get; set; }

            internal bool IsUpdate { get; set; }

            public int StartIndex { get; set; }

            internal ICollection<FrameworkElement> Targets { get; private set; }
        }

        #endregion
    }
}