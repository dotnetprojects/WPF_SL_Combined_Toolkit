using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
#if SILVERLIGHT
using WPF_SL_Combined_Toolkit.HelperClasses;
#endif

namespace WPF_SL_Combined_Toolkit.Controls.TileView
{
    [DefaultProperty("Header"), DefaultEvent("TileStateChanged")]
    public class TileViewItem : HeaderedContentControl
    {
        private const string ElementGripBar = "GripBarElement";
        private const string ElementMaximizeToggleButton = "MaximizeToggleButton";
        private static int _currentZIndex = 1;
        private static readonly TimeSpan DoubleClickDelta = TimeSpan.FromMilliseconds(300.0);

        public static readonly DependencyProperty MinimizedHeightProperty =
            DependencyProperty.Register("MinimizedHeight", typeof (double), typeof (TileViewItem), null);

        public static readonly DependencyProperty MinimizedWidthProperty = DependencyProperty.Register(
            "MinimizedWidth", typeof (double), typeof (TileViewItem), null);

        public event HandledEventHandler PreviewTileStateChanged;

        public event EventHandler TileStateChanged;

        public static readonly DependencyProperty TileStateProperty = DependencyProperty.Register(
            "TileState", typeof (TileViewItemState), typeof (TileViewItem),
            new PropertyMetadata(OnTileStateChanged));

        private UIElement _gripBar;
        private bool _ignoreCheckedChanged;
        private DateTime _lastGripBarClickTime;
        private ToggleButton _maximizeToggle;
        private WeakReference _parentTileViewReference;

#if !SILVERLIGHT
        static TileViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (TileViewItem), new FrameworkPropertyMetadata(typeof (TileViewItem)));
        }
#endif

        public TileViewItem()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(TileViewItem);
#endif

            TileState = TileViewItemState.Restored;
            _lastGripBarClickTime = new DateTime();
        }

        internal static int CurrentZIndex
        {
            get { return _currentZIndex; }
            set { _currentZIndex = value; }
        }

        public double MinimizedHeight
        {
            get { return (double) GetValue(MinimizedHeightProperty); }
            set { SetValue(MinimizedHeightProperty, value); }
        }

        public double MinimizedWidth
        {
            get { return (double) GetValue(MinimizedWidthProperty); }
            set { SetValue(MinimizedWidthProperty, value); }
        }

        [DefaultValue((string) null), Description("Gets the parent tileview that the item is assigned to."), Browsable(false)]
#if !SILVERLIGHT
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public TileView ParentTileView
        {
            get
            {
                if (_parentTileViewReference == null)
                {
                    return null;
                }
                return (TileView) _parentTileViewReference.Target;
            }
            internal set { _parentTileViewReference = new WeakReference(value); }
        }

        internal bool PositionAnimating { get; set; }

        internal bool SizeAnimating { get; set; }

        public TileViewItemState TileState
        {
            get { return (TileViewItemState) GetValue(TileStateProperty); }
            set { SetValue(TileStateProperty, value); }
        }

        internal bool TileStateRevertedFlag { get; set; }

        private void GripBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((IsDoubleClick(_lastGripBarClickTime) && (ParentTileView != null)) &&
                (ParentTileView.TileStateChangeTrigger == TileStateChangeTrigger.DoubleClick))
            {
                HandleDoubleClick();
            }
            _lastGripBarClickTime = DateTime.Now;
            var view = ItemsControl.ItemsControlFromItemContainer(this) as TileView;
            if (view != null)
            {
                view.OnItemDragging(this, e);
            }
        }

        private void HandleDoubleClick()
        {
            if (_maximizeToggle != null)
            {
                bool? isChecked = _maximizeToggle.IsChecked;
                _maximizeToggle.IsChecked = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : null;
            }
        }

        private void HandleItemMaximized()
        {
            if (((ParentTileView != null) && (_maximizeToggle != null)) &&
                (ParentTileView.MaximizeMode == TileViewMaximizeMode.One))
            {
                _maximizeToggle.IsEnabled = false;
            }
            if (_maximizeToggle != null)
            {
                _maximizeToggle.IsChecked = true;
                _ignoreCheckedChanged = false;
            }
        }

        private void HandleItemMinimized()
        {
            if (((ParentTileView != null) && (_maximizeToggle != null)) &&
                ((ParentTileView.MaximizeMode != TileViewMaximizeMode.Zero) && !_maximizeToggle.IsEnabled))
            {
                _maximizeToggle.IsEnabled = true;
            }
            if (_maximizeToggle != null)
            {
                _ignoreCheckedChanged = _maximizeToggle.IsChecked.HasValue && _maximizeToggle.IsChecked.Value;
                _maximizeToggle.IsChecked = false;
            }
        }

        private void HandleItemRestored()
        {
            _ignoreCheckedChanged = true;
            if (_maximizeToggle != null)
            {
                _maximizeToggle.IsChecked = false;
            }
        }

        private void HandleMaximizedReverted()
        {
            if (_maximizeToggle != null)
            {
                _ignoreCheckedChanged = _maximizeToggle.IsChecked.HasValue && _maximizeToggle.IsChecked.Value;
                _maximizeToggle.IsChecked = false;
            }
        }

        private static bool IsDoubleClick(DateTime lastClickTime)
        {
            return ((DateTime.Now - lastClickTime) <= DoubleClickDelta);
        }

        private void MaximizeToggleChecked(object sender, RoutedEventArgs e)
        {
            TileState = TileViewItemState.Maximized;
        }

        private void MaximizeToggleUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_ignoreCheckedChanged)
            {
                TileState = TileViewItemState.Restored;
            }
            _ignoreCheckedChanged = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_gripBar != null)
            {
            //    DragAndDropManager.SetAllowDrag(_gripBar, false);
                _gripBar.MouseLeftButtonDown -= GripBarMouseLeftButtonDown;
            }
            _gripBar = GetTemplateChild(ElementGripBar) as UIElement;
            if (_gripBar != null)
            {
            //    DragAndDropManager.SetAllowDrag(_gripBar, true);
                _gripBar.MouseLeftButtonDown += GripBarMouseLeftButtonDown;
            }
            if (_maximizeToggle != null)
            {
                _maximizeToggle.Checked -= MaximizeToggleChecked;
                _maximizeToggle.Unchecked -= MaximizeToggleUnchecked;
            }
            _maximizeToggle = GetTemplateChild(ElementMaximizeToggleButton) as ToggleButton;
            if (_maximizeToggle != null)
            {
                _maximizeToggle.Checked += MaximizeToggleChecked;
                _maximizeToggle.Unchecked += MaximizeToggleUnchecked;
                if (TileState == TileViewItemState.Restored)
                {
                    _ignoreCheckedChanged = _maximizeToggle.IsChecked.HasValue && _maximizeToggle.IsChecked.Value;
                    _maximizeToggle.IsChecked = false;
                }
                else if (TileState == TileViewItemState.Maximized)
                {
                    _maximizeToggle.IsChecked = true;
                }
            }
            if (((ParentTileView != null) && (_maximizeToggle != null)) &&
                ((ParentTileView.MaximizeMode == TileViewMaximizeMode.Zero) ||
                 ((ParentTileView.MaximizeMode == TileViewMaximizeMode.One) &&
                  (TileState == TileViewItemState.Maximized))))
            {
                _maximizeToggle.IsEnabled = false;
            }
        }

        protected internal virtual void OnPreviewTileStateChanged(HandledEventArgs e)
        {
            var evt = PreviewTileStateChanged;
            if (evt != null)
                evt(this, e);
        }

        protected internal virtual void OnTileStateChanged(EventArgs e)
        {
            var evt = TileStateChanged;
            if (evt != null)
                evt(this, e);
        }

        private static void OnTileStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as TileViewItem;
            if ((source != null) && !source.TileStateRevertedFlag)
            {
                var args = new HandledEventArgs();                
                source.OnPreviewTileStateChanged(args);
                if (args.Handled)
                {
                    try
                    {
                        source.TileStateRevertedFlag = true;
                        source.TileState = (TileViewItemState) e.OldValue;
                        if (((TileViewItemState) e.NewValue) == TileViewItemState.Maximized)
                        {
                            source.HandleMaximizedReverted();
                        }
                    }
                    finally
                    {
                        source.TileStateRevertedFlag = false;
                    }
                }
                else
                {
                    switch (((TileViewItemState) e.NewValue))
                    {
                        case TileViewItemState.Restored:
                            source.HandleItemRestored();
                            break;

                        case TileViewItemState.Maximized:
                            source.HandleItemMaximized();
                            break;

                        case TileViewItemState.Minimized:
                            source.HandleItemMinimized();
                            break;
                    }
                    source.OnTileStateChanged(new HandledEventArgs());
                }
            }
        }
    }
}