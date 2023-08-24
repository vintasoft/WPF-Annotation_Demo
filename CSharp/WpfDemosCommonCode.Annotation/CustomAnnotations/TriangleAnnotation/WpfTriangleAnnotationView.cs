using System.Windows.Media;

using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Class that determines how to display the annotation that displays a triangle
    /// and how user can interact with annotation.
    /// </summary>
    public class WpfTriangleAnnotationView : WpfPolygonAnnotationView
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfTriangleAnnotationView"/> class.
        /// </summary>
        /// <param name="annotationData">Object that stores the annotation data.</param>
        public WpfTriangleAnnotationView(TriangleAnnotationData annotationData)
            : base(annotationData)
        {
            FillBrush = null;

            // create a point-based builder
            Builder = new WpfPointBasedAnnotationPointBuilder(this, 3, 3);

            // create a transformer for rectangular mode
            WpfPointBasedAnnotationRectangularTransformer rectangleTransformer = new WpfPointBasedAnnotationRectangularTransformer(this);
            // show bounding box area
            rectangleTransformer.BoundingBoxArea.IsVisible = true;
            RectangularTransformer = rectangleTransformer;

            // create a transformer for point mode
            WpfPointBasedAnnotationPointTransformer pointsTransformer = new WpfPointBasedAnnotationPointTransformer(this);
            // change interaction points color
            pointsTransformer.InteractionPointBackColor = Color.FromArgb((byte)100, (byte)255, (byte)0, (byte)0);
            pointsTransformer.SelectedInteractionPointBackColor = Color.FromArgb((byte)150, (byte)255, (byte)0, (byte)0);
            // change interaction points type
            pointsTransformer.PolygonPointTemplate = new WpfTriangleAnnotationInteractionPoint();
            PointTransformer = pointsTransformer;

            GripMode = GripMode.RectangularAndPoints;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current 
        /// <see cref="TriangleAnnotationView"/> instance.
        /// </summary>
        /// <returns>A new object that is a copy of this <see cref="TriangleAnnotationView"/>
        /// instance.</returns>
        public override object Clone()
        {
            return new WpfTriangleAnnotationView((TriangleAnnotationData)this.Data.Clone());
        }

        #endregion

    }
}
