using System;
using Microsoft.Maps.MapControl.WPF;
using System.Windows;

namespace Vayu.CommonControls
{
    /// <summary>
    /// 
    /// </summary>
    public static class Mapping
    {
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static LocationRect GetView(DependencyObject obj)
        {
            return (LocationRect)obj.GetValue(ViewProperty);
        }

        /// <summary>
        /// Sets the view.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetView(DependencyObject obj, LocationRect value)
        {
            obj.SetValue(ViewProperty, value);
        }

        /// <summary>
        /// The view property
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.RegisterAttached("View", typeof(LocationRect), typeof(Mapping), new PropertyMetadata(OnViewPropertyChanged));

        /// <summary>
        /// Called when [view property changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map map = d as Map;
            var rect = e.NewValue as LocationRect;
            if (map != null && rect != null)
            {
                map.SetView(rect);
            }
        }
    }
}
