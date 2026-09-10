using System.Windows.Media;
using System.Windows.Shapes;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Shapes.Shape" />
    public class StarShade : Shape
    {
        /// <summary>
        /// The ellipse
        /// </summary>
        public EllipseGeometry ellipse;

        //public static readonly DependencyProperty TextBoxRProperty = DependencyProperty.Register("TextBoxR", typeof(TextBox), typeof(StarShade), new FrameworkPropertyMetadata(null));
        //public TextBox TextBox
        //{
        //    get { return (TextBox)GetValue(TextBoxRProperty); }
        //    set { SetValue(TextBoxRProperty, value); }
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="StarShade"/> class.
        /// </summary>
        public StarShade()
        {
            ellipse = new EllipseGeometry();

            this.Stroke = Brushes.Gray;
            this.StrokeThickness = 3;
        }

        /// <summary>
        /// Gets a value that represents the <see cref="T:System.Windows.Media.Geometry" /> of the <see cref="T:System.Windows.Shapes.Shape" />.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                ellipse.RadiusX = 100 / 2;
                ellipse.RadiusY = 100 / 2;

                return ellipse;
            }
        }
    }
}
