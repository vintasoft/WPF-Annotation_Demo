using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.UI;
using Vintasoft.Imaging.Annotation.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents a user control that displays annotation comments of focused page on image viewer.
    /// </summary>
    public partial class AnnotationCommentsControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The annotation comment builder.
        /// </summary>
        AnnotationCommentBuilder _annotationCommentBuilder;

        /// <summary>
        /// A value indicating whether the comment tool was enabled (<see cref="CommentVisualTool.IsEnabled"/>) before annotation building is started.
        /// </summary>
        bool _commentToolEnabledBeforeAnnotationBuilding = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationCommentsControl"/> class.
        /// </summary>
        public AnnotationCommentsControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the image viewer.
        /// </summary>
        public WpfImageViewer ImageViewer
        {
            get
            {
                return commentsControl1.ImageViewer;
            }
            set
            {
                commentsControl1.ImageViewer = value;
            }
        }

        WpfAnnotationVisualTool _annotationTool;
        /// <summary>
        /// Gets or sets the PDF annotation tool.
        /// </summary>
        public WpfAnnotationVisualTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                if (_annotationTool != null)
                {
                    _annotationTool.AnnotationBuildingStarted -= AnnotationTool_AnnotationBuildingStarted;
                    _annotationTool.AnnotationBuildingFinished -= AnnotationTool_AnnotationBuildingFinished;
                    _annotationTool.FocusedAnnotationViewChanged -= AnnotationTool_FocusedAnnotationViewChanged;
                    _annotationTool.MouseDoubleClick -= AnnotationTool_MouseDoubleClick;
                }

                _annotationTool = value;

                if (_annotationTool != null)
                {
                    _annotationTool.AnnotationBuildingStarted += AnnotationTool_AnnotationBuildingStarted;
                    _annotationTool.AnnotationBuildingFinished += AnnotationTool_AnnotationBuildingFinished;
                    _annotationTool.FocusedAnnotationViewChanged += AnnotationTool_FocusedAnnotationViewChanged;
                    _annotationTool.MouseDoubleClick += AnnotationTool_MouseDoubleClick;
                }

                UpdateUI();

                UpdateAnnotationCommentBuilder();
            }
        }

        /// <summary>
        /// Gets or sets the comment visual tool.
        /// </summary>
        public WpfCommentVisualTool CommentTool
        {
            get
            {
                return commentsControl1.CommentTool;
            }
            set
            {
                if (value != null)
                {
                    commentsControl1.CommentTool = value;
                    commentsControl1.CommentController = value.CommentController;

                    // disable selection of comments in viewer using mouse because this control controls selection of comments by yourself
                    value.SelectCommentControlOnMouseClick = false;
                    value.SelectCommentControlOnMouseDoubleClick = false;
                }
                else
                {
                    commentsControl1.CommentTool = null;
                    commentsControl1.CommentController = null;
                }

                UpdateUI();

                UpdateAnnotationCommentBuilder();
            }
        }

        /// <summary>
        /// Gets a value indicating whether comments are visible on image viewer.
        /// </summary>
        public bool IsCommentsOnViewerVisible
        {
            get
            {
                return commentsControl1.IsCommentsOnViewerVisible;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            if (AnnotationTool == null || CommentTool == null)
            {
                mainPanel.IsEnabled = false;
            }
            else
            {
                mainPanel.IsEnabled = true;

                bool isAnnotationFocused = AnnotationTool.FocusedAnnotationView != null;
                bool isInteractionModeAuthor = AnnotationTool.AnnotationInteractionMode == AnnotationInteractionMode.Author;
                bool isCommentSelected = isAnnotationFocused && AnnotationTool.FocusedAnnotationView.Data.Comment != null;

                addNewCommentButton.IsEnabled = isInteractionModeAuthor;
                removeCommentFromAnnotationButton.IsEnabled = isAnnotationFocused && isCommentSelected && isInteractionModeAuthor;
                addCommentToAnnotationButton.IsEnabled = isAnnotationFocused && isInteractionModeAuthor;
            }
        }

        /// <summary>
        /// Updates the annotation comment builder.
        /// </summary>
        private void UpdateAnnotationCommentBuilder()
        {
            if (CommentTool != null && AnnotationTool != null)
                _annotationCommentBuilder = new AnnotationCommentBuilder(CommentTool, AnnotationTool);
            else
                _annotationCommentBuilder = null;
        }

        /// <summary>
        /// "Add New Comment" button is clicked.
        /// </summary>
        private void AddNewCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommentTool.Enabled)
            {
                _annotationCommentBuilder.AddNewComment();

                UpdateUI();
            }
        }

        /// <summary>
        /// "Add Comment To Annotation" button is clicked.
        /// </summary>
        private void AddCommentToAnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommentTool.Enabled)
            {
                _annotationCommentBuilder.AddCommentToAnnotation(AnnotationTool.FocusedAnnotationView);

                UpdateUI();
            }
        }

        /// <summary>
        /// "Remove Comment From Annotation" button is clicked.
        /// </summary>
        private void RemoveCommentFromAnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommentTool.Enabled)
            {
                // remove comment from focused annotation
                AnnotationTool.FocusedAnnotationView.Data.Comment.Remove();

                UpdateUI();
            }
        }

        /// <summary>
        /// "Close All Comments" button is clicked.
        /// </summary>
        private void CloseAllCommentsButton_Click(object sender, RoutedEventArgs e)
        {
            CommentCollection comments =
                CommentTool.CommentController.GetComments(ImageViewer.Image);

            foreach (Comment comment in comments)
                comment.IsOpen = false;
        }

        /// <summary>
        /// Annotation building is started.
        /// </summary>
        private void AnnotationTool_AnnotationBuildingStarted(object sender, WpfAnnotationViewEventArgs e)
        {
            if (commentsControl1.IsCommentsOnViewerVisible &&
                AnnotationTool.AnnotationInteractionMode == AnnotationInteractionMode.Author)
            {
                _commentToolEnabledBeforeAnnotationBuilding = CommentTool.IsEnabled;

                CommentTool.IsEnabled = false;

                UpdateUI();
            }
        }

        /// <summary>
        /// Annotation building is finished.
        /// </summary>
        private void AnnotationTool_AnnotationBuildingFinished(object sender, WpfAnnotationViewEventArgs e)
        {
            if (commentsControl1.IsCommentsOnViewerVisible &&
                AnnotationTool.AnnotationInteractionMode == AnnotationInteractionMode.Author)
            {
                CommentTool.IsEnabled = _commentToolEnabledBeforeAnnotationBuilding;

                UpdateUI();
            }

            SelectComment(e.AnnotationView);
        }

        /// <summary>
        /// Focused annotation is changed.
        /// </summary>
        private void AnnotationTool_FocusedAnnotationViewChanged(object sender, WpfAnnotationViewChangedEventArgs e)
        {
            SelectComment(e.NewValue);

            UpdateUI();
        }

        /// <summary>
        /// Mouse is double clicked in viewer.
        /// </summary>
        private void AnnotationTool_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CommentTool != null && WpfObjectConverter.CreateVintasoftMouseButtons(e) == AnnotationTool.ActionButton)
            {
                // find annnotation view
                WpfAnnotationView view = AnnotationTool.FindAnnotationView(e.GetPosition(AnnotationTool));

                // if annotation contains the annotation
                if (view != null && view.Data.Comment != null)
                {
                    // get annotation comment
                    Comment comment = view.Data.Comment;
                    // find comment control
                    ICommentControl commentControl = CommentTool.FindCommentControl(comment);

                    // if comment control is found
                    if (commentControl != null)
                    {
                        if (CommentTool.SelectedComment != commentControl)
                            CommentTool.SelectedComment = commentControl;

                        // open comment
                        comment.IsOpen = true;
                    }
                }
            }
        }

        /// <summary>
        /// Selects the comment of specified annotation view.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SelectComment(WpfAnnotationView view)
        {
            if (view == null)
            {
                CommentTool.SelectedComment = null;
            }
            else
            {
                if (view.Data.Comment != null)
                    CommentTool.SelectedComment = CommentTool.FindCommentControl(view.Data.Comment);
            }
        }

        #endregion

    }
}
