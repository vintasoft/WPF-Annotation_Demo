using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging;

using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools.UserInteraction;

using Vintasoft.Imaging.Wpf;

using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;


namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Class that determines how to display the annotation that displays a mark
    /// and how user can interact with annotation.
    /// </summary>
    public class WpfMarkAnnotationView : WpfAnnotationView, IWpfRectangularInteractiveObject
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfMarkAnnotationView"/> class.
        /// </summary>
        /// <param name="annotationData">Object that stores the annotation data.</param>
        public WpfMarkAnnotationView(MarkAnnotationData annotationData)
            : base(annotationData)
        {
            Size initialSize = Size;
            if (initialSize.Width == 0 || initialSize.Height == 0)
            {
                initialSize = new Size(64, 64);
                Size = initialSize;
            }
            Builder = new WpfMarkAnnotationBuilder(this);

            WpfRectangularAnnotationTransformer transformer = new WpfRectangularAnnotationTransformer(this);
            transformer.HideInteractionPointsWhenMoving = true;
            foreach (WpfInteractionPoint point in transformer.ResizePoints)
                point.Background = new SolidColorBrush(Color.FromArgb((byte)100, (byte)255, (byte)0, (byte)0));
            Transformer = transformer;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets a mark type.
        /// </summary>
        [Description("The mark type.")]
        [DefaultValue(MarkAnnotationType.Tick)]
        public MarkAnnotationType MarkType
        {
            get { return MarkAnnoData.MarkType; }
            set { MarkAnnoData.MarkType = value; }
        }

        /// <summary>
        /// Gets an annotation data.
        /// </summary>
        MarkAnnotationData MarkAnnoData
        {
            get { return (MarkAnnotationData)Data; }
        }

        /// <summary>
        /// Gets or sets the rotation angle of interactive object.
        /// </summary>
        double IWpfRectangularInteractiveObject.RotationAngle
        {
            get { return Rotation; }
            set { Rotation = (float)value; }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Indicates whether the specified point is contained within the annotation.
        /// </summary>
        /// <param name="point">Point in image space.</param>
        /// <returns><b>true</b> if the specified point is contained within the annotation;
        /// otherwise, <b>false</b>.</returns>
        public override bool IsPointOnFigure(Point point)
        {
            Matrix m = GetTransformFromContentToImageSpace();
            m.Invert();
            point = m.Transform(point);
            Geometry path = GetAsPathGeometry();
            Pen pen = new Pen(Brushes.Transparent, this.Outline.Width);
            return path.FillContains(point) || path.StrokeContains(pen, point);
        }

        /// <summary>
        /// Returns a drawing box of annotation, in the image space.
        /// </summary>
        /// <param name="drawingSurface">The object that provides information about drawing surface.</param>
        /// <returns>Drawing box of annotation, in the image space.</returns>
        public override Rect GetDrawingBox(WpfDrawingSurface drawingSurface)
        {
            Matrix m = GetTransformFromContentToImageSpace();
            Geometry path = GetAsPathGeometry();
            path.Transform = new MatrixTransform(m);
            Pen pen = WpfAnnotationObjectConverter.CreateWindowsPen(Outline);
            return path.GetRenderBounds(pen);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current 
        /// <see cref="LineAnnotation"/> instance.
        /// </summary>
        /// <returns>A new object that is a copy of this <see cref="LineAnnotation"/>
        /// instance.</returns>
        public override object Clone()
        {
            return new WpfMarkAnnotationView((MarkAnnotationData)this.Data.Clone());
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Returns an annotation selection as <see cref="Geometry"/> in the image space. 
        /// </summary>
        public override Geometry GetSelectionAsGeometry()
        {
            Size size = Size;
            RectangleGeometry path = new RectangleGeometry(
                new Rect(-size.Width / 2, -size.Height / 2, size.Width, size.Height));
            Matrix transform = GetTransformFromContentToImageSpace();
            path.Transform = new MatrixTransform(transform);
            return path;
        }

        /// <summary>
        /// Renders the annotation on the <see cref="DrawingContext"/>
        /// in the coordinate space of annotation.
        /// </summary>
        /// <param name="drawingContext">The <see cref="DrawingContext"/> to render on.</param>
        /// <param name="drawingSurface">The object that provides information about drawing surface.</param>
        protected override void RenderInContentSpace(DrawingContext drawingContext, WpfDrawingSurface drawingSurface)
        {
            if (FillBrush != null || Border)
            {
                Brush brush = null;
                if (FillBrush != null)
                    brush = WpfAnnotationObjectConverter.CreateWindowsBrush(FillBrush);
                Pen pen = WpfAnnotationObjectConverter.CreateWindowsPen(Outline);
                if (brush != null || pen != null)
                {
                    Geometry path = GetAsPathGeometry();
                    drawingContext.DrawGeometry(brush, pen, path);
                }
            }
        }

        /// <summary>
        /// Sets the properties of interaction controller according to the properties of annotation.
        /// </summary>
        /// <param name="controller">The interaction controller.</param>
        protected override void SetInteractionControllerProperties(IWpfInteractionController controller)
        {
            base.SetInteractionControllerProperties(controller);

            WpfRectangularObjectTransformer rectangularTransformer = controller as WpfRectangularObjectTransformer;
            if (rectangularTransformer != null)
            {
                rectangularTransformer.CanMove = Data.CanMove;
                rectangularTransformer.CanResize = Data.CanResize;
                rectangularTransformer.CanRotate = Data.CanRotate;
                return;
            }
        }

        /// <summary>
        /// Returns a mark annotation as <see cref="Geometry"/> in content space.
        /// </summary>
        protected virtual Geometry GetAsPathGeometry()
        {
            PathGeometry pathGeometry = new PathGeometry();

            Point[] referencePoints =
                WpfObjectConverter.CreateWindowsPointArray(MarkAnnoData.GetReferencePointsInContentSpace());

            switch (MarkType)
            {
                case MarkAnnotationType.Tick:
                    pathGeometry.Figures.Add(GeometryUtils.CreateCurve(referencePoints, true));
                    break;
                default:
                    pathGeometry.Figures.Add(GeometryUtils.CreatePolygon(referencePoints, true));
                    break;
            }

            return pathGeometry;
        }

        /// <summary>
        /// Raises the <see cref="StateChanged"/> event. 
        /// Invoked when the property of annotation is changed.
        /// </summary>
        /// <param name="e">An <see cref="ObjectPropertyChangedEventArgs"/>
        /// that contains the event data.</param>
        protected override void OnDataPropertyChanged(ObjectPropertyChangedEventArgs e)
        {
            base.OnDataPropertyChanged(e);

            if (e.PropertyName == "Size")
            {
                if (Builder is WpfMarkAnnotationBuilder)
                    ((WpfMarkAnnotationBuilder)Builder).InitialSize =
                        WpfObjectConverter.CreateWindowsSize((System.Drawing.SizeF)e.NewValue);
            }
        }

        #endregion


        #region IRectangularInteractiveObject

        /// <summary>
        /// Returns a rectangle of interactive object.
        /// </summary>
        /// <param name="x0">Left-top X coordinate of rectangle.</param>
        /// <param name="y0">Left-top Y coordinate of rectangle.</param>
        /// <param name="x1">Right-bottom X coordinate of rectangle.</param>
        /// <param name="y1">Right-bottom Y coordinate of rectangle.</param>
        void IWpfRectangularInteractiveObject.GetRectangle(out double x0, out double y0, out double x1, out double y1)
        {
            Point location = Location;
            Size size = Size;
            x0 = location.X - size.Width / 2;
            y0 = location.Y - size.Height / 2;
            x1 = location.X + size.Width / 2;
            y1 = location.Y + size.Height / 2;
            if (Data.HorizontalMirrored)
            {
                double tmp = x0;
                x0 = x1;
                x1 = tmp;
            }
            if (Data.VerticalMirrored)
            {
                double tmp = y0;
                y0 = y1;
                y1 = tmp;
            }
        }

        /// <summary>
        /// Sets a rectangle of interactive object.
        /// </summary>
        /// <param name="x0">Left-top X coordinate of rectangle.</param>
        /// <param name="y0">Left-top Y coordinate of rectangle.</param>
        /// <param name="x1">Right-bottom X coordinate of rectangle.</param>
        /// <param name="y1">Right-bottom Y coordinate of rectangle.</param>
        void IWpfRectangularInteractiveObject.SetRectangle(double x0, double y0, double x1, double y1)
        {
            Size = new Size(Math.Abs(x0 - x1), Math.Abs(y0 - y1));
            Location = new Point((x0 + x1) / 2, (y0 + y1) / 2);

            HorizontalMirrored = x0 > x1;
            VerticalMirrored = y0 > y1;

            if (Data.IsInitializing)
                OnStateChanged();
        }

        #endregion

        #endregion

    }
}
