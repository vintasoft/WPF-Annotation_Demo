using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents rounded interaction point for vertices of triangle annotation.
    /// </summary>
    internal class WpfTriangleAnnotationInteractionPoint : WpfInteractionPolygonPoint
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfTriangleAnnotationInteractionPoint"/> class.
        /// </summary>
        internal WpfTriangleAnnotationInteractionPoint()
            : base("Resize")
        {
            BorderPen = new Pen(Brushes.Black, 1.0);
            Background = new SolidColorBrush(Color.FromArgb((byte)100, (byte)255, (byte)0, (byte)0));
            this.Radius = 6;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Renders the interaction area on specified <see cref="DrawingContext"/>.
        /// </summary>
        /// <param name="viewer">The image viewer.</param>
        /// <param name="drawingContext">An instance of <see cref="System.Windows.Media.DrawingContext" />
        /// used to render the area.</param>
        public override void Render(WpfImageViewer viewer, DrawingContext drawingContext)
        {
            Rect rect = GetDrawingRect();

            drawingContext.DrawEllipse(
                Background,
                BorderPen,
                new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                rect.Width / 2,
                rect.Height / 2);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override WpfInteractionArea Clone()
        {
            WpfTriangleAnnotationInteractionPoint area = new WpfTriangleAnnotationInteractionPoint();
            CopyTo(area);
            return area;
        }

        #endregion

    }
}
