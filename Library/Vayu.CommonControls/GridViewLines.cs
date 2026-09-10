using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Vayu.CommonControls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.GridViewRowPresenter" />
    public class GridViewLines : GridViewRowPresenter
    {
        /// <summary>
        /// The default separator style
        /// </summary>
        private static readonly Style DefaultSeparatorStyle;
        /// <summary>
        /// The separator style property
        /// </summary>
        public static readonly DependencyProperty SeparatorStyleProperty;
        /// <summary>
        /// The lines
        /// </summary>
        private readonly List<FrameworkElement> _lines = new List<FrameworkElement>();

        /// <summary>
        /// Initializes the <see cref="GridViewLines"/> class.
        /// </summary>
        static GridViewLines()
        {
            DefaultSeparatorStyle = new Style(typeof (Rectangle));
            DefaultSeparatorStyle.Setters.Add(new Setter(Shape.FillProperty, SystemColors.ControlLightBrush));
            SeparatorStyleProperty = DependencyProperty.Register("SeparatorStyle", typeof(Style), typeof(GridViewLines),
                                                                    new UIPropertyMetadata(DefaultSeparatorStyle, SeparatorStyleChanged));
        }

        /// <summary>
        /// Gets or sets the separator style.
        /// </summary>
        /// <value>
        /// The separator style.
        /// </value>
        public Style SeparatorStyle
        {
            get { return (Style) GetValue(SeparatorStyleProperty); }
            set { SetValue(SeparatorStyleProperty, value); }
        }

        #region Private Methods

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        private IEnumerable<FrameworkElement> Children
        {
            get { return LogicalTreeHelper.GetChildren(this).OfType<FrameworkElement>(); }
        }

        /// <summary>
        /// Separators the style changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void SeparatorStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (GridViewLines)d;
            var style = (Style)e.NewValue;
            foreach (FrameworkElement line in presenter._lines)
            {
                line.Style = style;
            }
        }

        /// <summary>
        /// Ensures the lines.
        /// </summary>
        /// <param name="count">The count.</param>
        private void EnsureLines(int count)
        {
            count = count - _lines.Count;
            for (var i = 0; i < count; i++)
            {
                var line = (FrameworkElement)Activator.CreateInstance(SeparatorStyle.TargetType);
                line = new Rectangle { Fill = Brushes.LightGray };
                line.Style = SeparatorStyle;
                AddVisualChild(line);
                _lines.Add(line);
            }
        }
        
        #endregion

        #region Protected Methods

        /// <summary>
        /// Positions the content of a row according to the size of the corresponding <see cref="T:System.Windows.Controls.GridViewColumn" /> objects.
        /// </summary>
        /// <param name="arrangeSize">The area to use to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.</param>
        /// <returns>
        /// The actual <see cref="T:System.Windows.Size" /> that is used to display the <see cref="P:System.Windows.Controls.GridViewRowPresenter.Content" />.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var size = base.ArrangeOverride(arrangeSize);
            var children = Children.ToList();
            EnsureLines(children.Count);
            for (var i = 0; i < _lines.Count; i++)
            {
                var child = children[i];
                var x = child.TransformToAncestor(this).Transform(new Point(child.ActualWidth, 0)).X + child.Margin.Right;
                var rect = new Rect(x, -Margin.Top, 1, size.Height + Margin.Top + Margin.Bottom);
                var line = _lines[i];
                line.Measure(rect.Size);
                line.Arrange(rect);
            }
            return size;
        }

        /// <summary>
        /// Gets the number of visual children for a row.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + _lines.Count; }
        }

        /// <summary>
        /// Gets the visual child in the row item at the specified index.
        /// </summary>
        /// <param name="index">The index of the child.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Media.Visual" /> object that contains the child at the specified index.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            var count = base.VisualChildrenCount;
            return index < count ? base.GetVisualChild(index) : _lines[index - count];
        }

        #endregion
    }
}