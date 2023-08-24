#if !REMOVE_ANNOTATION_PLUGIN
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents an interaction transformer that transforms <see cref="CommentControl"/>.
    /// </summary>
    public class CommentControlTransformer : WpfRectObjectTransformer, IDisposable
    {

        #region Fields

        /// <summary>
        /// The image viewer.
        /// </summary>
        WpfImageViewer _imageViewer;

        /// <summary>
        /// A value indicating whether the <see cref="CommentControl"/> content is hidden.
        /// </summary>
        bool _commentControlContrentHidden = false;

        /// <summary>
        /// The <see cref="CommentControl"/> cursor before transform.
        /// </summary>
        Cursor _commentControlCursor = null;

        /// <summary>
        /// A value indicating whether the mouse is captured.
        /// </summary>
        bool _mouseCaptured = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentControlTransformer"/> class.
        /// </summary>
        /// <param name="commentControl">The comment control.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <i>commentControl</i> is <b>null</b>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if image viewer is not found.</exception>
        public CommentControlTransformer(CommentControl commentControl)
        {
            if (commentControl == null)
                throw new ArgumentNullException();

            _commentControl = commentControl;

            _imageViewer = WpfImageViewer.GetImageViewer(commentControl);

            if (_imageViewer == null)
                throw new InvalidOperationException("The image viewer is not found.");

            SizeChangeMargin = 6;

            if (commentControl.topGrid.ActualHeight == 0)
            {
                commentControl.topGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                LocationChangeTopMargin = Math.Max(1, commentControl.topGrid.DesiredSize.Height);
            }
            else
            {
                LocationChangeTopMargin = Math.Max(1, commentControl.topGrid.ActualHeight);
            }

            SubscribeToControlEvents(commentControl);

            ShowMoveCursor = false;
        }

        #endregion



        #region Properties

        CommentControl _commentControl;
        /// <summary>
        /// Gets the comment control.
        /// </summary>
        public CommentControl CommentControl
        {
            get
            {
                return _commentControl;
            }
        }

        bool _hideContentOnTransform = false;
        /// <summary>
        /// Gets or sets a value indicating whether transformer hides content when transformation performs.
        /// </summary>
        /// <value>
        /// <b>True</b> - transformer must hide the comment control content and set the control background to <see cref="TransfromBackgroundColor"/> color when transformation performs;<br/>
        /// <b>false</b> - transformer must not change the comment control content when transformation performs.<br />
        /// Default value is <b>False</b>.
        /// </value>
        /// <seealso cref="TransfromBackgroundColor"/>
        public bool HideContentOnTransform
        {
            get
            {
                return _hideContentOnTransform;
            }
            set
            {
                _hideContentOnTransform = value;
            }
        }

        Color _transfromBackgroundColor = Color.FromArgb(255, 204, 224, 236);
        /// <summary>
        /// Gets or sets the color of the background when comment control is transforming.
        /// </summary>
        /// <seealso cref="HideContentOnTransform"/>
        public Color TransfromBackgroundColor
        {
            get
            {
                return _transfromBackgroundColor;
            }
            set
            {
                _transfromBackgroundColor = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Releases all resources used by this <see cref="CommentControlTransformer"/> object.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromControlEvents(CommentControl);
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Returns the rectangle of an object in pixels.
        /// </summary>
        /// <returns>
        /// Object <see cref="Rectangle" /> in pixels.
        /// </returns>
        protected override Rect GetRect()
        {
            Point location = GetCommentControlPosition();
            Size size = new Size(CommentControl.ActualWidth, CommentControl.ActualHeight);

            return new Rect(location, size);
        }

        /// <summary>
        /// Sets the new rectangle of an object in pixels.
        /// </summary>
        /// <param name="rect">The new rectangle of an object in pixels.</param>
        protected override void SetRect(Rect rect)
        {
            VintasoftImage image = _imageViewer.Image;

            if (image != null)
            {
                // get transform from control to the device independent pixels (DIP)
                AffineMatrix matrix = _imageViewer.GetTransformFromVisualToolToDip();

                // convert location to the DIP
                Point locationInDip = WpfPointAffineTransform.TransformPoint(matrix, rect.Location);

                if (HideContentOnTransform)
                {
                    // if comment control content is visible
                    if (!_commentControlContrentHidden)
                    {
                        // hide comment control content
                        UpdateCommentControlContentVisibility(false);
                        _commentControlContrentHidden = true;
                    }
                }

                // update comment bounding box
                CommentControl.Comment.BoundingBox = WpfObjectConverter.CreateDrawingRectangleF(new Rect(locationInDip, rect.Size));
            }
        }

        /// <summary>
        /// Returns the minimum object size in pixels.
        /// </summary>
        /// <returns>
        /// Minimum object <see cref="Size" /> in pixels.
        /// </returns>
        protected override Size GetMinSize()
        {
            return new Size(150, 47);
        }

        /// <summary>
        /// Returns the bounding box of the object's parent container in pixels.
        /// </summary>
        /// <returns>
        /// <see cref="Rectangle" /> of the objec't container in pixels.
        /// </returns>
        protected override Rect GetBoundingBox()
        {
            return new Rect(
                _imageViewer.HorizontalOffset,
                _imageViewer.VerticalOffset,
                _imageViewer.ViewportWidth,
                _imageViewer.ViewportHeight);
        }

        #endregion


        #region PRIVATE 

        /// <summary>
        /// Updates the <see cref="CommentControl"/> content visibility.
        /// </summary>
        /// <param name="value">Indicates whether the <see cref="CommentControl"/> content is visible.</param>
        private void UpdateCommentControlContentVisibility(bool value)
        {
            if (value)
            {
                CommentControl.UpdateUI();
                CommentControl.SetSize();
            }
            else
            {
                CommentControl.Background = new SolidColorBrush(TransfromBackgroundColor);
            }

            CommentControl.mainGrid.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Subscribes to <see cref="Control"/> events.
        /// </summary>
        /// <param name="control">The control.</param>
        private void SubscribeToControlEvents(Control control)
        {
            if (control is Button)
                return;

            control.PreviewMouseDown += Control_PreviewMouseDown;
            control.PreviewMouseUp += Control_PreviewMouseUp;
            control.PreviewMouseMove += Control_PreviewMouseMove;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="Control"/> events.
        /// </summary>
        /// <param name="control">The control.</param>
        private void UnsubscribeFromControlEvents(Control control)
        {
            if (control is Button)
                return;

            control.PreviewMouseDown -= Control_PreviewMouseDown;
            control.PreviewMouseUp -= Control_PreviewMouseUp;
            control.PreviewMouseMove -= Control_PreviewMouseMove;
        }

        /// <summary>
        /// Handler of the mouse move event.
        /// </summary>
        private void Control_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // if mouse cursor is over the button
            if (GetButton(e.OriginalSource as DependencyObject) != null)
                return;

            Point mousePosition = e.GetPosition(CommentControl);
            Point commentControlLocation = GetCommentControlPosition();
            DoMouseMove(mousePosition, commentControlLocation, WpfObjectConverter.CreateVintasoftMouseButtons(e));

            // if image viewer cursor differs from comment control cursor
            if (_imageViewer.Cursor != CommentControl.Cursor)
            {
                // use comment control cursor in image viewer
                _imageViewer.Cursor = CommentControl.Cursor;
            }
        }

        /// <summary>
        /// Handler of the mouse down event.
        /// </summary>
        private void Control_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // if mouse is down on button
            if (GetButton(e.OriginalSource as DependencyObject) != null)
                return;

            Point mousePosition = e.GetPosition(CommentControl);
            Point commentControlLocation = GetCommentControlPosition();
            DoMouseDown(mousePosition, commentControlLocation, WpfObjectConverter.CreateVintasoftMouseButtons(e));

            if (IsTransforming)
            {
                Mouse.Capture(CommentControl);
                Cursor currentCursor = GetCurrentCursor();
                _commentControlCursor = CommentControl.Cursor;
                if (currentCursor != null)
                {
                    CommentControl.Cursor = currentCursor;
                    // use comment control cursor in image viewer
                    _imageViewer.Cursor = CommentControl.Cursor;
                }
                _mouseCaptured = true;
            }
        }

        /// <summary>
        /// Returns the <see cref="CommentControl"/> position on image viewer.
        /// </summary>
        /// <returns>
        /// The <see cref="CommentControl"/> position on image viewer.
        /// </returns>
        private Point GetCommentControlPosition()
        {
            double x = Canvas.GetLeft(CommentControl);
            double y = Canvas.GetTop(CommentControl);

            return new Point(x, y);
        }

        /// <summary>
        /// Handler of the mouse up event.
        /// </summary>
        private void Control_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_mouseCaptured)
            {
                if (Mouse.Captured == CommentControl)
                    Mouse.Capture(null);
                CommentControl.Cursor = _commentControlCursor;
                _imageViewer.Cursor = CommentControl.Cursor;
                _mouseCaptured = false;
            }

            if (_commentControlContrentHidden)
            {
                // show comment control content
                UpdateCommentControlContentVisibility(true);
                _commentControlContrentHidden = false;
            }
        }

        /// <summary>
        /// Returns the <see cref="DependencyObject"/> where the <paramref name="dependencyObject"/> is located.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        private Button GetButton(DependencyObject dependencyObject)
        {
            DependencyObject currentObject = dependencyObject;

            while (currentObject != null)
            {
                if (currentObject is Button)
                    return (Button)currentObject;

                currentObject = VisualTreeHelper.GetParent(currentObject);
            }

            return null;
        }

        #endregion

        #endregion

    }
}
#endif