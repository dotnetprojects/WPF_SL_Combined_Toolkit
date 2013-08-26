using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPF_SL_Combined_Toolkit.Controls.Animation;
using WPF_SL_Combined_Toolkit.ExtensionMethods;
#if SILVERLIGHT
using WPF_SL_Combined_Toolkit.HelperClasses;
#endif

namespace WPF_SL_Combined_Toolkit.Controls.TileView
{
    [DefaultEvent("TileStateChanged"), DefaultProperty("Items")]
    public class TileView : ItemsControl
    {
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof (DataTemplate), typeof (TileView), null);

        public static readonly DependencyProperty ContentTemplateSelectorProperty =
            DependencyProperty.Register(
                "ContentTemplateSelector", typeof (DataTemplateSelector), typeof (TileView), null);

        public static readonly DependencyProperty IsItemDraggingEnabledProperty =
            DependencyProperty.Register(
                "IsItemDraggingEnabled", typeof (bool), typeof (TileView), new PropertyMetadata(true));

        public static readonly DependencyProperty IsItemsAnimationEnabledProperty =
            DependencyProperty.Register(
                "IsItemsAnimationEnabled", typeof (bool), typeof (TileView),
                new PropertyMetadata(true, OnIsItemsAnimationEnabledChanged));

        public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
            "MaxColumns", typeof (int), typeof (TileView), new PropertyMetadata(0x7fffffff, OnMaxColumnsChanged));

        public static readonly DependencyProperty MaximizedItemProperty = DependencyProperty.Register(
            "MaximizedItem", typeof (object), typeof (TileView), new PropertyMetadata(null, OnMaximizedItemChanged));

        public static readonly DependencyProperty MaximizeModeProperty = DependencyProperty.Register(
            "MaximizeMode", typeof (TileViewMaximizeMode), typeof (TileView),
            new PropertyMetadata(TileViewMaximizeMode.ZeroOrOne));

        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
            "MaxRows", typeof (int), typeof (TileView), new PropertyMetadata(0x7fffffff, OnMaxRowsChanged));

        public static readonly DependencyProperty MinimizedColumnWidthProperty =
            DependencyProperty.Register(
                "MinimizedColumnWidth", typeof (double), typeof (TileView),
                new PropertyMetadata(250.0, OnMinimizedColumnWidthChanged));

        public static readonly DependencyProperty MinimizedItemsPositionProperty =
            DependencyProperty.Register(
                "MinimizedItemsPosition", typeof (Dock), typeof (TileView),
                new PropertyMetadata(Dock.Right, OnMinimizedItemsPositionChanged));

        public static readonly DependencyProperty MinimizedRowHeightProperty =
            DependencyProperty.Register(
                "MinimizedRowHeight", typeof (double), typeof (TileView),
                new PropertyMetadata(75.0, OnMinimizedRowHeightChanged));

        public static readonly DependencyProperty ReorderingDurationProperty =
            DependencyProperty.Register(
                "ReorderingDuration", typeof (Duration), typeof (TileView),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(330.0))));

        public static readonly DependencyProperty ReorderingEasingProperty =
            DependencyProperty.Register("ReorderingEasing", typeof (IEasingFunction), typeof (TileView), null);

        public static readonly DependencyProperty ResizingDurationProperty =
            DependencyProperty.Register(
                "ResizingDuration", typeof (Duration), typeof (TileView),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(330.0))));

        public static readonly DependencyProperty ResizingEasingProperty = DependencyProperty.Register(
            "ResizingEasing", typeof (IEasingFunction), typeof (TileView), null);

        public static readonly DependencyProperty TileStateChangeTriggerProperty =
            DependencyProperty.Register(
                "TileStateChangeTrigger", typeof (TileStateChangeTrigger), typeof (TileView),
                new PropertyMetadata(TileStateChangeTrigger.DoubleClick));

        private int _columns = 1;
        private TileViewItem _draggedContainer;
        private bool _removingItemFlag;
        private int _rows = 1;

#if !SILVERLIGHT
        static TileView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (TileView), new FrameworkPropertyMetadata(typeof (TileView)));
        }
#endif

        public TileView()
        {
#if SILVERLIGHT
            this.DefaultStyleKey = typeof(TileView);
#endif

            SizeChanged += TileViewSizeChanged;
            PreviewTileStateChanged += TileViewPreviewTileStateChanged;
        }

        [Category("Appearance")]
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate) GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        [Category("Appearance")]
        public DataTemplateSelector ContentTemplateSelector
        {
            get { return (DataTemplateSelector) GetValue(ContentTemplateSelectorProperty); }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        public bool IsItemDraggingEnabled
        {
            get { return (bool) GetValue(IsItemDraggingEnabledProperty); }
            set { SetValue(IsItemDraggingEnabledProperty, value); }
        }

        public bool IsItemsAnimationEnabled
        {
            get
            {
                return (((bool) GetValue(IsItemsAnimationEnabledProperty)) &&
                        AnimationManager.GetIsAnimationEnabled(this));
            }
            set { SetValue(IsItemsAnimationEnabledProperty, value); }
        }

        internal Point LastDragPosition { get; set; }

        public int MaxColumns
        {
            get { return (int) GetValue(MaxColumnsProperty); }
            set { SetValue(MaxColumnsProperty, value); }
        }

        public object MaximizedItem
        {
            get { return GetValue(MaximizedItemProperty); }
            set { SetValue(MaximizedItemProperty, value); }
        }

        public TileViewMaximizeMode MaximizeMode
        {
            get { return (TileViewMaximizeMode) GetValue(MaximizeModeProperty); }
            set { SetValue(MaximizeModeProperty, value); }
        }

        public int MaxRows
        {
            get { return (int) GetValue(MaxRowsProperty); }
            set { SetValue(MaxRowsProperty, value); }
        }

        public double MinimizedColumnWidth
        {
            get { return (double) GetValue(MinimizedColumnWidthProperty); }
            set { SetValue(MinimizedColumnWidthProperty, value); }
        }

        public Dock MinimizedItemsPosition
        {
            get { return (Dock) GetValue(MinimizedItemsPositionProperty); }
            set { SetValue(MinimizedItemsPositionProperty, value); }
        }

        public double MinimizedRowHeight
        {
            get { return (double) GetValue(MinimizedRowHeightProperty); }
            set { SetValue(MinimizedRowHeightProperty, value); }
        }

        public Duration ReorderingDuration
        {
            get { return (Duration) GetValue(ReorderingDurationProperty); }
            set { SetValue(ReorderingDurationProperty, value); }
        }

        public IEasingFunction ReorderingEasing
        {
            get { return (IEasingFunction) GetValue(ReorderingEasingProperty); }
            set { SetValue(ReorderingEasingProperty, value); }
        }

        public Duration ResizingDuration
        {
            get { return (Duration) GetValue(ResizingDurationProperty); }
            set { SetValue(ResizingDurationProperty, value); }
        }

        public IEasingFunction ResizingEasing
        {
            get { return (IEasingFunction) GetValue(ResizingEasingProperty); }
            set { SetValue(ResizingEasingProperty, value); }
        }

        public TileStateChangeTrigger TileStateChangeTrigger
        {
            get { return (TileStateChangeTrigger) GetValue(TileStateChangeTriggerProperty); }
            set { SetValue(TileStateChangeTriggerProperty, value); }
        }

        public event HandledEventHandler PreviewTileStateChanged;

        public event EventHandler TileStateChanged;
        
        private void AnimateItemPosition(TileViewItem item, Point targetPosition)
        {
            if (item.PositionAnimating)
            {
                return;
            }
            Point currentItemPosition = GetCurrentItemPosition(item);
            double num = targetPosition.X - currentItemPosition.X;
            double num2 = targetPosition.Y - currentItemPosition.Y;
            if (Equals(MaximizedItem, item))
            {
                int currentZIndex = TileViewItem.CurrentZIndex;
                TileViewItem.CurrentZIndex = currentZIndex + 1;
                Canvas.SetZIndex(item, currentZIndex);
            }
            double totalSeconds = ReorderingDuration.TimeSpan.TotalSeconds;
            var args = new double[4];
            args[1] = -num;
            args[2] = totalSeconds;
            var numArray2 = new double[4];
            numArray2[1] = -num2;
            numArray2[2] = totalSeconds;
            Storyboard sb =
                AnimationExtensions.Create().Animate(new FrameworkElement[] {item}).EnsureDefaultTransforms().MoveX(
                    args).MoveY(numArray2).Origin(0.0, 0.0).EaseAll((ReorderingEasing ?? Easings.SlideDown1)).
                    AdjustSpeed().Instance;
            sb.Completed += (sender, e) =>
                                {
                                    sb.Stop();
                                    item.PositionAnimating = false;
                                };
            SetItemPosition(item, targetPosition);
            item.PositionAnimating = true;
            sb.Begin();
        }

        private void AnimateItemSize(TileViewItem item, Size targetSize)
        {
            if (!item.SizeAnimating)
            {
                var size = new Size(item.ActualWidth, item.ActualHeight);
                item.Height = targetSize.Height;
                item.Width = targetSize.Width;
                double totalSeconds = ResizingDuration.TimeSpan.TotalSeconds;
                var args = new double[4];
                args[1] = size.Height;
                args[2] = totalSeconds;
                args[3] = targetSize.Height;
                var numArray2 = new double[4];
                numArray2[1] = size.Width;
                numArray2[2] = totalSeconds;
                numArray2[3] = targetSize.Width;
                Storyboard sb =
                    AnimationExtensions.Create().Animate(new FrameworkElement[] {item}).EnsureDefaultTransforms().Height
                        (args).Width(numArray2).EaseAll((ResizingEasing ?? Easings.SlideDown1)).AdjustSpeed().Instance;
                sb.Completed += (sender, e) =>
                                    {
                                        sb.Stop();
                                        item.SizeAnimating = false;
                                    };
                item.SizeAnimating = true;
                sb.Begin();
            }
        }

        private void AnimateItemSizes()
        {
            if (!ShouldAnimateItemSizes())
            {
                return;
            }
            foreach (TileViewItem item in GetGeneratedItemContainers())
            {
                AnimateItemSize(item, GetItemSize(item));
            }
        }

        private void AnimateItemsLayout()
        {
            if (!IsVisualUpdateNeccessary())
            {
                return;
            }
            IEnumerable<TileViewItem> generatedItemContainers = GetGeneratedItemContainers();
            if (MaximizedItem == null)
            {
                AnimateRestoredItemsPositions(generatedItemContainers);
            }
            else
            {
                AnimateItemsWhenThereIsMaximized();
            }
        }

        private void AnimateItemsWhenThereIsMaximized()
        {
            Dictionary<int, TileViewItem> orderedContainers = GetOrderedContainers();
            double currentOffset = 0.0;
            for (int i = 0; i < orderedContainers.Count; i++)
            {
                var item = ItemContainerGenerator.ContainerFromItem(MaximizedItem) as TileViewItem;
                if (!Equals(orderedContainers[i], item))
                {
                    AnimateItemPosition(orderedContainers[i], GetNewDockingPosition(currentOffset));
                    currentOffset = GetCurrentOffset(
                        currentOffset, new Size(orderedContainers[i].Width, orderedContainers[i].Height));
                }
                else
                {
                    AnimateItemPosition(orderedContainers[i], GetNewMaximizedDockingPosition());
                }
            }
        }

        private void AnimateRestoredItemsPositions(IEnumerable<TileViewItem> containers)
        {
            foreach (TileViewItem item in containers.Where(ShouldAnimateItemPosition))
            {
                AnimateItemPosition(item, GetTargetItemPosition(item));
            }
        }

        private void AssignItemContainersToCells(IDictionary<int, TileViewItem> orderedContainers)
        {
            if (orderedContainers.Count == 0)
            {
                return;
            }
            ConstructLayoutGrid();
            int key = 0;
            int num2 = CountItemsWithGeneratedContainers();
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (orderedContainers.ContainsKey(key))
                    {
                        Grid.SetRow(orderedContainers[key], i);
                        Grid.SetColumn(orderedContainers[key], j);
                    }
                    key++;
                    if (key == num2)
                    {
                        break;
                    }
                }
                if (key == num2)
                {
                    return;
                }
            }
        }

        protected virtual void AttachItemContainerEventHandlers(TileViewItem item)
        {
            item.TileStateChanged += TileViewItemTileStateChanged;
            if (item.TileState == TileViewItemState.Maximized)
            {
                MaximizedItem = ItemContainerGenerator.ItemFromContainer(item);
            }
        }

        private Point CalculateDraggedItemPosition(Point currentPoint)
        {
            Point point = currentPoint;
            Rect absoluteCoordinates = GetAbsoluteCoordinates(this);
            bool flag = (currentPoint.X > absoluteCoordinates.Left) && (currentPoint.X < absoluteCoordinates.Right);
            bool flag2 = (currentPoint.Y > absoluteCoordinates.Top) && (currentPoint.Y < absoluteCoordinates.Bottom);
            var point2 = new Point(Canvas.GetLeft(_draggedContainer), Canvas.GetTop(_draggedContainer));
            double num = point2.X + (point.X - LastDragPosition.X);
            double num2 = point2.Y + (point.Y - LastDragPosition.Y);
            double x = flag ? num : point2.X;
            double y = flag2 ? num2 : point2.Y;
            var point3 = new Point(x, y);
            LastDragPosition = point;
            return point3;
        }

        private int CalculateItemIndex(TileViewItem item)
        {
            return ((Grid.GetRow(item)*_columns) + Grid.GetColumn(item));
        }

        private bool CanDragItems()
        {
            return ((MaximizedItem == null) && IsItemDraggingEnabled);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            _removingItemFlag = true;
            base.ClearContainerForItemOverride(element, item);
            var item2 = element as TileViewItem;
            DetachItemContainerEventHandlers(item2);
            AssignItemContainersToCells(GetOrderedContainers());
            if ((item == MaximizedItem) || (Items.Count == 1))
            {
                MaximizedItem = null;
                if ((MaximizeMode == TileViewMaximizeMode.One) && (Items.Count > 0))
                {
                    var item3 = ItemContainerGenerator.ContainerFromItem(Items[0]) as TileViewItem;
                    if (item3 != null)
                    {
                        item3.TileState = TileViewItemState.Maximized;
                    }
                }
                if (IsItemsAnimationEnabled)
                {
                    AnimateItemSizes();
                    AnimateItemsLayout();
                }
                else
                {
                    UpdateItemSizes();
                    UpdateItemsLayout();
                }
            }
            else
            {
                UpdateItemsSizeAndPosition();
            }
            _removingItemFlag = false;
        }

        private void ConstructLayoutGrid()
        {
            double d = CountItemsWithGeneratedContainers();
            _rows = (int) Math.Floor(Math.Sqrt(d));
            if (MaxRows > 0)
            {
                if (_rows > MaxRows)
                {
                    _rows = MaxRows;
                }
                _columns = (int) Math.Ceiling(d/_rows);
            }
            if (MaxColumns > 0)
            {
                _columns = (int) Math.Ceiling(d/_rows);
                if (_columns > MaxColumns)
                {
                    _columns = MaxColumns;
                    _rows = (int) Math.Ceiling(d/_columns);
                }
            }
            if ((MaxColumns == 0) && (MaxRows == 0))
            {
                _columns = (int) Math.Ceiling(d/_rows);
            }
        }

        private int CountItemsWithGeneratedContainers()
        {
            return GetGeneratedItemContainers().Count();
        }

        protected virtual void DetachItemContainerEventHandlers(TileViewItem item)
        {
            item.TileStateChanged -= TileViewItemTileStateChanged;
            if (MaximizedItem == ItemContainerGenerator.ItemFromContainer(item))
            {
                MaximizedItem = null;
            }
        }

        private static DependencyObject GetRoot(DependencyObject element)
        {
            var root = element;
            while (true)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element == null)
                    return root;
                root = element;
            }
           
        }

        private static Rect GetAbsoluteCoordinates(UIElement element)
        {
            return new Rect(
                element.TransformToVisual(GetRoot(element) as UIElement).Transform(new Point(0.0, 0.0)),
                element.RenderSize);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TileViewItem();
        }

        private int GetCurrentColumn(Point mousePosition)
        {
            return (int) Math.Floor(mousePosition.X/(ActualWidth/_columns));
        }

        private static Point GetCurrentItemPosition(UIElement item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            double left = Canvas.GetLeft(item);
            return new Point(left, Canvas.GetTop(item));
        }

        private double GetCurrentOffset(double currentOffset, Size lastItemSize)
        {
            if (MinimizedItemsPosition.Equals(Dock.Left) || MinimizedItemsPosition.Equals(Dock.Right))
            {
                currentOffset += lastItemSize.Height;
                return currentOffset;
            }
            currentOffset += lastItemSize.Width;
            return currentOffset;
        }

        private int GetCurrentRow(Point mousePosition)
        {
            return (int) Math.Floor(mousePosition.Y/(ActualHeight/_rows));
        }

        private Point GetDraggedItemCurrentPosition()
        {
            return new Point(Canvas.GetLeft(_draggedContainer), Canvas.GetTop(_draggedContainer));
        }

        private IEnumerable<TileViewItem> GetGeneratedItemContainers()
        {
            return
                Items.OfType<object>().Select(item => ItemContainerGenerator.ContainerFromItem(item) as TileViewItem)
                    .Where(item => (item != null) && (item.Visibility == Visibility.Visible));
        }

        private Size GetItemSize(TileViewItem item)
        {
            if (MaximizedItem == null)
            {
                return GetRestoredItemSize(item);
            }
            var item2 = ItemContainerGenerator.ContainerFromItem(MaximizedItem) as TileViewItem;
            return Equals(item, item2) ? GetMaximizedItemSize(item) : GetMinimizedItemSize(item);
        }

        private TileViewItem GetItemToSwapWith(Point currentPoint)
        {
            return GetGeneratedItemContainers().FirstOrDefault(item2 => ShouldSwapWithItem(item2, currentPoint));
        }

        private Size GetMaximizedItemSize(TileViewItem item)
        {
            double num = ((ActualWidth - MinimizedColumnWidth) - item.Margin.Left) - item.Margin.Right;
            double num2 = (ActualHeight - item.Margin.Top) - item.Margin.Bottom;
            if (MinimizedItemsPosition.Equals(Dock.Bottom) || MinimizedItemsPosition.Equals(Dock.Top))
            {
                num = (ActualWidth - item.Margin.Left) - item.Margin.Right;
                num2 = ((ActualHeight - MinimizedRowHeight) - item.Margin.Top) - item.Margin.Bottom;
            }
            num = Math.Max(0.0, num);
            return new Size(num, Math.Max(0.0, num2));
        }

        private Size GetMinimizedItemSize(TileViewItem item)
        {
            double num8;
            double num9;
            var generatedItemContainers = GetGeneratedItemContainers().ToList();
            int num = CountItemsWithGeneratedContainers();
            double num2 = generatedItemContainers
                .Where(i => i.MinimizedHeight > double.Epsilon)
                .Select(i => i.MinimizedHeight).Sum();
            double num3 = generatedItemContainers
                .Where(i => i.MinimizedWidth > double.Epsilon)
                .Select(i => i.MinimizedWidth).Sum();
            int num4 = num - generatedItemContainers.Count(i => i.MinimizedHeight > 0.0);
            int num5 = generatedItemContainers.Count(i => i.MinimizedWidth > 0.0);
            if (MaximizedItem != null)
            {
                var item2 = ItemContainerGenerator.ContainerFromItem(MaximizedItem) as TileViewItem;
                if ((item2 != null) && (item2.MinimizedWidth > double.Epsilon))
                {
                    num3 -= item2.MinimizedWidth;
                    num5++;
                }
                if ((item2 != null) && (item2.MinimizedHeight > double.Epsilon))
                {
                    num2 -= item2.MinimizedHeight;
                    num4++;
                }
            }
            double num6 = ActualWidth - num3;
            double num7 = ActualHeight - num2;
            bool isDockedTopBottom = (MinimizedItemsPosition == Dock.Bottom) || (MinimizedItemsPosition == Dock.Top);
            if (isDockedTopBottom)
            {
                num8 = ((num6/(num5 - 1)) - item.Margin.Left) - item.Margin.Right;
                num9 = (MinimizedRowHeight - item.Margin.Top) - item.Margin.Bottom;
            }
            else
            {
                num8 = (MinimizedColumnWidth - item.Margin.Left) - item.Margin.Right;
                num9 = ((num7/(num4 - 1)) - item.Margin.Top) - item.Margin.Bottom;
            }
            num8 = ((item.MinimizedWidth > 0.0) && isDockedTopBottom) ? item.MinimizedWidth : Math.Max(0.0, num8);
            return new Size(num8,
                            ((item.MinimizedHeight > 0.0) && !isDockedTopBottom)
                                ? item.MinimizedHeight
                                : Math.Max(0.0, num9));
        }

        private Point GetNewDockingPosition(double currentOffset)
        {
            double x = 0.0;
            double y = currentOffset;
            switch (MinimizedItemsPosition)
            {
                case Dock.Left:
                    x = 0.0;
                    y = currentOffset;
                    break;

                case Dock.Top:
                    x = currentOffset;
                    y = 0.0;
                    break;

                case Dock.Right:
                    x = ActualWidth - MinimizedColumnWidth;
                    y = currentOffset;
                    break;

                case Dock.Bottom:
                    x = currentOffset;
                    y = ActualHeight - MinimizedRowHeight;
                    break;
            }
            return new Point(x, y);
        }

        private Point GetNewMaximizedDockingPosition()
        {
            double x = 0.0;
            double y = 0.0;
            switch (MinimizedItemsPosition)
            {
                case Dock.Left:
                    x = MinimizedColumnWidth;
                    y = 0.0;
                    break;

                case Dock.Top:
                    x = 0.0;
                    y = MinimizedRowHeight;
                    break;

                case Dock.Right:
                    x = 0.0;
                    y = 0.0;
                    break;

                case Dock.Bottom:
                    x = 0.0;
                    y = 0.0;
                    break;
            }
            return new Point(x, y);
        }

        private Dictionary<int, TileViewItem> GetOrderedContainers()
        {
            var dictionary = new Dictionary<int, TileViewItem>();
            foreach (TileViewItem item in GetGeneratedItemContainers())
            {
                int key = CalculateItemIndex(item);
                if (_removingItemFlag)
                {
                    if ((dictionary.Count == 0) && (key > 0))
                    {
                        key = 0;
                    }
                    if (dictionary.Count > 0)
                    {
                        int num2 = dictionary.Keys.Max() + 1;
                        key = (key != num2) ? num2 : key;
                    }
                }
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
            }
            return dictionary;
        }

        private Point GetRelativeMousePosition(Point absolutePosition)
        {
            var point = new Point();
            if (IsMouseOverVisual(absolutePosition, this))
            {
                Rect absoluteCoordinates = GetAbsoluteCoordinates(this);
                double x = absolutePosition.X - absoluteCoordinates.Left;
                double y = absolutePosition.Y - absoluteCoordinates.Top;
                point = new Point(x, y);
            }
            return point;
        }

        private Point GetRestoredItemPosition(TileViewItem item)
        {
            double x = Grid.GetColumn(item)*(ActualWidth/_columns);
            return new Point(x, Grid.GetRow(item)*(ActualHeight/_rows));
        }

        private Size GetRestoredItemSize(TileViewItem item)
        {
            double num = ((ActualWidth/_columns) - item.Margin.Left) - item.Margin.Right;
            double num2 = ((ActualHeight/_rows) - item.Margin.Top) - item.Margin.Bottom;
            num = Math.Max(0.0, num);
            return new Size(num, Math.Max(0.0, num2));
        }

        private Point GetSanitizedDraggedItemPosition(Point currentPoint)
        {
            Point draggedItemCurrentPosition = GetDraggedItemCurrentPosition();
            Point newPosition = CalculateDraggedItemPosition(currentPoint);
            return !IsValidDraggedItemPosition(newPosition) ? draggedItemCurrentPosition : newPosition;
        }

        private Point GetTargetItemPosition(TileViewItem item)
        {
            double x = Grid.GetColumn(item)*(ActualWidth/_columns);
            return new Point(x, Grid.GetRow(item)*(ActualHeight/_rows));
        }

        private void HandleTileViewItemMaximized(TileViewItem item)
        {
            int currentZIndex = TileViewItem.CurrentZIndex;
            TileViewItem.CurrentZIndex = currentZIndex + 1;
            Canvas.SetZIndex(item, currentZIndex);
            MaximizedItem = ItemContainerGenerator.ItemFromContainer(item);
            foreach (var item2 in GetGeneratedItemContainers())
            {
                if (!Equals(item2, item))
                    item2.TileState = TileViewItemState.Minimized;
            }
            UpdateItemsSizeAndPosition();
            SetItemSize(item, GetMaximizedItemSize(item));
        }

        private void HandleTileViewItemRestored(TileViewItem item)
        {
            if (MaximizeMode != TileViewMaximizeMode.One)
            {
                MaximizedItem = null;
                foreach (TileViewItem item2 in GetGeneratedItemContainers())
                {
                    item2.TileState = TileViewItemState.Restored;
                }
                if (IsItemsAnimationEnabled)
                {
                    AnimateItemSizes();
                    AnimateItemsLayout();
                }
                else
                {
                    UpdateItemSizes();
                    UpdateItemsLayout();
                }
            }
            else
            {
                item.TileState = TileViewItemState.Maximized;
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is TileViewItem);
        }

        private bool IsItemMaximizedByDefault(TileViewItem item)
        {
            if (((MaximizeMode != TileViewMaximizeMode.One) || (MaximizedItem != null)) &&
                (item.TileState != TileViewItemState.Maximized))
            {
                return (MaximizedItem == ItemContainerGenerator.ItemFromContainer(item));
            }
            return true;
        }

        private static bool IsMouseOverVisual(Point mousePosition, UIElement visual)
        {
            Rect absoluteCoordinates = GetAbsoluteCoordinates(visual);
            return ((((mousePosition.X >= absoluteCoordinates.Left) && (mousePosition.X <= absoluteCoordinates.Right)) &&
                     (mousePosition.Y >= absoluteCoordinates.Top)) && (mousePosition.Y <= absoluteCoordinates.Bottom));
        }

        private bool IsValidDraggedItemPosition(Point newPosition)
        {
            var size = new Size(_draggedContainer.ActualWidth, _draggedContainer.ActualHeight);
            var rect = new Rect(newPosition, size);
            return (((((rect.Bottom <= ActualHeight) && (rect.Bottom >= size.Height)) &&
                      ((rect.Left >= 0.0) && (rect.Left <= (ActualWidth - size.Width)))) &&
                     (((rect.Right >= size.Width) && (rect.Right <= ActualWidth)) && (rect.Top >= 0.0))) &&
                    (rect.Top <= (ActualHeight - size.Height)));
        }

        private bool IsVisualUpdateNeccessary()
        {
            return ((!double.IsInfinity(ActualWidth) && !double.IsNaN(ActualWidth)) && !DoubleUtil.IsZero(ActualWidth));
        }

        private void MaximizedContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var item = sender as TileViewItem;
            if (item != null)
            {
                if (ShouldRestrainMaximizedItemWidth(e.NewSize.Width))
                {
                    item.Width = e.PreviousSize.Width;
                    UpdateItemSizes();
                }
                else if (ShouldRestrainMaximizedItemHeight(e.NewSize.Height))
                {
                    item.Height = e.PreviousSize.Height;
                    UpdateItemSizes();
                }
            }
        }

        private static void OnIsItemsAnimationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                AnimationManager.SetIsAnimationEnabled(view, (bool) e.NewValue);
            }
        }

        internal void OnItemDragging(TileViewItem draggedItem, MouseEventArgs e)
        {
            if (IsItemDraggingEnabled && CanDragItems())
            {
                CaptureMouse();
                _draggedContainer = draggedItem;
                LastDragPosition = e.GetPosition(null);
                int currentZIndex = TileViewItem.CurrentZIndex;
                TileViewItem.CurrentZIndex = currentZIndex + 1;
                Canvas.SetZIndex(_draggedContainer, currentZIndex);
                MouseMove += TileViewMouseMove;
                AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(TileViewMouseLeftButtonUp), true);
            }
        }

        private static void OnMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                view.AssignItemContainersToCells(view.GetOrderedContainers());
                view.AnimateItemSizes();
                view.AnimateItemsLayout();
            }
        }

        private static void OnMaximizedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                var item = view.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TileViewItem;
                if ((item != null) && (item.TileState != TileViewItemState.Maximized))
                {
                    if (view.MaximizedItem != null)
                    {
                        var item2 = view.ItemContainerGenerator.ContainerFromItem(view.MaximizedItem) as TileViewItem;
                        if (item2 != null)
                        {
                            item2.TileState = TileViewItemState.Restored;
                        }
                    }
                    item.TileState = TileViewItemState.Maximized;
                }
            }
        }

        private static void OnMaxRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                view.AssignItemContainersToCells(view.GetOrderedContainers());
                view.AnimateItemSizes();
                view.AnimateItemsLayout();
            }
        }

        private static void OnMinimizedColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                view.UpdateItemsLayout();
            }
        }

        private static void OnMinimizedItemsPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                view.AnimateItemSizes();
                view.AnimateItemsLayout();
            }
        }

        private static void OnMinimizedRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TileView;
            if (view != null)
            {
                view.UpdateItemsLayout();
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var item2 = element as TileViewItem;
            if (item2 != null)
            {
                item2.ParentTileView = this;

                Dictionary<int, TileViewItem> orderedContainers = GetOrderedContainers();
                if (!orderedContainers.ContainsKey(orderedContainers.Count))
                {
                    orderedContainers.Add(orderedContainers.Count, item2);
                }
                AttachItemContainerEventHandlers(item2);
                AssignItemContainersToCells(orderedContainers);
                UpdateItemsSizeAndPosition();
                if ((MaximizeMode == TileViewMaximizeMode.One) && IsItemMaximizedByDefault(item2))
                {
                    item2.SizeChanged += MaximizedContainerSizeChanged;
                    if (item2.TileState == TileViewItemState.Maximized)
                    {
                        HandleTileViewItemMaximized(item2);
                    }
                    else
                    {
                        item2.TileState = TileViewItemState.Maximized;
                    }
                }
                if ((MaximizedItem != null) && (MaximizedItem != item))
                {
                    item2.TileState = TileViewItemState.Minimized;
                }
                if (item2.ContentTemplate == null)
                {
                    if (ContentTemplate != null)
                    {
                        item2.ContentTemplate = ContentTemplate;
                    }
                    else if (ContentTemplateSelector != null)
                    {
                        item2.ContentTemplate = ContentTemplateSelector.SelectTemplate(item, item2);
                    }
                }
            }

            UpdateItemsLayout();
        }

        private void TileViewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedContainer = null;
            MouseMove -= TileViewMouseMove;
            MouseLeftButtonUp -= TileViewMouseLeftButtonUp;
            ReleaseMouseCapture();
            UpdateItemsLayout();
        }

        private void TileViewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedContainer == null)
            {
                return;
            }
            Point position = e.GetPosition(null);
            Point sanitizedDraggedItemPosition = GetSanitizedDraggedItemPosition(position);
            if (sanitizedDraggedItemPosition != GetDraggedItemCurrentPosition())
            {
                SetItemPosition(_draggedContainer, sanitizedDraggedItemPosition);
            }
            TileViewItem itemToSwapWith = GetItemToSwapWith(position);
            if (itemToSwapWith != null)
            {
                SwapItems(itemToSwapWith);
            }
        }

        private void TileViewPreviewTileStateChanged(object sender, HandledEventArgs e)
        {
            var source = sender as TileViewItem;
            var control = ItemsControlFromItemContainer(source);
            if (((source != null) && (control != null)) &&
                ((Equals(this, control)) && (source.TileState == TileViewItemState.Maximized)))
            {
                e.Handled = MaximizeMode == TileViewMaximizeMode.Zero;
            }
        }

        private void TileViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemSizes();
            UpdateItemsLayout();
        }

        private static void SetItemPosition(TileViewItem item, Point position)
        {
            Canvas.SetLeft(item, position.X);
            Canvas.SetTop(item, position.Y);
        }

        private static void SetItemSize(TileViewItem item, Size itemSize)
        {
            item.Width = itemSize.Width;
            item.Height = itemSize.Height;
        }

        private bool ShouldAnimateItemPosition(TileViewItem item)
        {
            if (Equals(item, _draggedContainer))
            {
                return false;
            }
            Point currentItemPosition = GetCurrentItemPosition(item);
            Point targetItemPosition = GetTargetItemPosition(item);
            if (DoubleUtil.AreClose(targetItemPosition.X, currentItemPosition.X))
            {
                return !DoubleUtil.AreClose(targetItemPosition.Y, currentItemPosition.Y);
            }
            return true;
        }

        private bool ShouldAnimateItemSizes()
        {
            return (((!double.IsInfinity(ActualWidth) && !double.IsNaN(ActualWidth)) && !DoubleUtil.IsZero(ActualWidth)) &&
                    !DoubleUtil.IsZero(ActualHeight));
        }

        private bool ShouldRestrainMaximizedItemHeight(double newHeight)
        {
            if ((MinimizedItemsPosition != Dock.Top) && (MinimizedItemsPosition != Dock.Bottom))
            {
                return false;
            }
            return (newHeight > (ActualHeight - MinimizedRowHeight));
        }

        private bool ShouldRestrainMaximizedItemWidth(double newWidth)
        {
            if ((MinimizedItemsPosition != Dock.Left) && (MinimizedItemsPosition != Dock.Right))
            {
                return false;
            }
            return (newWidth > (ActualWidth - MinimizedColumnWidth));
        }

        private bool ShouldSwapWithItem(TileViewItem item, Point currentPoint)
        {
            Point absolutePosition = currentPoint;
            Point relativeMousePosition = GetRelativeMousePosition(absolutePosition);
            int currentRow = GetCurrentRow(relativeMousePosition);
            int currentColumn = GetCurrentColumn(relativeMousePosition);
            int row = Grid.GetRow(item);
            int column = Grid.GetColumn(item);
            bool flag = IsMouseOverVisual(absolutePosition, item);
            if (((Equals(item, _draggedContainer)) || !flag) || item.PositionAnimating)
            {
                return false;
            }
            if (column != currentColumn)
            {
                return (row == currentRow);
            }
            return true;
        }

        private void SwapItems(TileViewItem itemToSwapWith)
        {
            int column = Grid.GetColumn(itemToSwapWith);
            int row = Grid.GetRow(itemToSwapWith);
            Grid.SetColumn(itemToSwapWith, Grid.GetColumn(_draggedContainer));
            Grid.SetRow(itemToSwapWith, Grid.GetRow(_draggedContainer));
            Grid.SetColumn(_draggedContainer, column);
            Grid.SetRow(_draggedContainer, row);
            if (IsItemsAnimationEnabled)
            {
                AnimateItemsLayout();
            }
            else
            {
                UpdateItemsLayout();
            }
        }

        private void TileViewItemTileStateChanged(object sender, EventArgs e)
        {
            var container = sender as TileViewItem;
            if ((container != null) && (Equals(ItemsControlFromItemContainer(container), this)))
            {
                if (container.TileState == TileViewItemState.Maximized)
                {
                    HandleTileViewItemMaximized(container);
                }
                else if (container.TileState == TileViewItemState.Restored)
                {
                    HandleTileViewItemRestored(container);
                }
            }
        }

        private void UpdateItemSizes()
        {
            foreach (TileViewItem item in GetGeneratedItemContainers())
            {
                SetItemSize(item, GetItemSize(item));
            }
        }

        private void UpdateItemsLayout()
        {
            if (IsVisualUpdateNeccessary())
            {
                IEnumerable<TileViewItem> generatedItemContainers = GetGeneratedItemContainers();
                if (MaximizedItem == null)
                {
                    UpdateRestoredItemsPositions(generatedItemContainers);
                }
                else
                {
                    UpdateItemsPositionsWhenThereIsMaximized();
                }
            }
        }

        private void UpdateItemsPositionsWhenThereIsMaximized()
        {
            Dictionary<int, TileViewItem> orderedContainers = GetOrderedContainers();
            double currentOffset = 0.0;
            for (int i = 0; i < orderedContainers.Count; i++)
            {
                Size minimizedItemSize;
                Point newDockingPosition;
                var item = ItemContainerGenerator.ContainerFromItem(MaximizedItem) as TileViewItem;
                if (!Equals(orderedContainers[i], item))
                {
                    minimizedItemSize = GetMinimizedItemSize(orderedContainers[i]);
                    newDockingPosition = GetNewDockingPosition(currentOffset);
                    currentOffset = GetCurrentOffset(currentOffset, minimizedItemSize);
                }
                else
                {
                    minimizedItemSize = GetMaximizedItemSize(orderedContainers[i]);
                    newDockingPosition = GetNewMaximizedDockingPosition();
                }
                SetItemSize(orderedContainers[i], minimizedItemSize);
                SetItemPosition(orderedContainers[i], newDockingPosition);
            }
        }

        private void UpdateItemsSizeAndPosition()
        {
            if (IsItemsAnimationEnabled)
            {
                AnimateItemSizes();
                AnimateItemsLayout();
            }
            else
            {
                UpdateItemSizes();
                UpdateItemsLayout();
            }
        }

        private void UpdateRestoredItem(TileViewItem item)
        {
            SetItemPosition(item, GetRestoredItemPosition(item));
            SetItemSize(item, GetRestoredItemSize(item));
        }

        private void UpdateRestoredItemsPositions(IEnumerable<TileViewItem> containers)
        {
            foreach (TileViewItem item in containers)
            {
                if (!item.PositionAnimating && (!Equals(item, _draggedContainer)))
                {
                    UpdateRestoredItem(item);
                }
            }
        }
    }


    public enum TileStateChangeTrigger
    {
        None,
        DoubleClick
    }

    public enum TileViewItemState
    {
        Restored,
        Maximized,
        Minimized
    }

    public enum TileViewMaximizeMode
    {
        Zero,
        ZeroOrOne,
        One
    }
}