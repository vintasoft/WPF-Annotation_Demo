using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
#endif
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents a user control that displays comments of focused page of image viewer.
    /// </summary>
    public partial class CommentsControl : UserControl
    {

        #region Constructors

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsControl"/> class.
        /// </summary>
        public CommentsControl()
        {
            InitializeComponent();
        }
#endif

        #endregion



        #region Properties

#if !REMOVE_ANNOTATION_PLUGIN
        WpfImageViewer _imageViewer;
        /// <summary>
        /// Gets or sets the image viewer.
        /// </summary>
        public WpfImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                if (_imageViewer != value)
                {
                    if (_imageViewer != null)
                    {
                        _imageViewer.FocusedIndexChanged -= ImageViewer_FocusedIndexChanged;
                    }
                    _imageViewer = value;
                    if (_imageViewer != null)
                    {
                        _imageViewer.FocusedIndexChanged += ImageViewer_FocusedIndexChanged;
                    }

                    if (_commentController != null)
                        _commentController.ImageViewer = ImageViewer;

                    UpdateComments();
                }
            }
        }


        CommentCollection _comments;
        /// <summary>
        /// Gets the comments, which are displayed in control.
        /// </summary>
        public CommentCollection Comments
        {
            get
            {
                return _comments;
            }
        }


        WpfImageViewerCommentController _commentController;
        /// <summary>
        /// Gets or sets the comment controller.
        /// </summary>
        public WpfImageViewerCommentController CommentController
        {
            get
            {
                return _commentController;
            }
            set
            {
                if (_commentController != null)
                {
                    _commentController.ImageCommentsChanged -= CommentController_ImageCommentsChanged;
                }

                _commentController = value;

                if (_commentController != null)
                {
                    _commentController.ImageCommentsChanged += CommentController_ImageCommentsChanged;
                    _commentController.ImageViewer = ImageViewer;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether comments are visible on image viewer.
        /// </summary>
        public bool IsCommentsOnViewerVisible
        {
            get
            {
                return visibleOnViewerCheckBox.IsChecked.Value == true;
            }
        }

        WpfCommentVisualTool _commentateTool;
        /// <summary>
        /// Gets or sets the comment visual tool.
        /// </summary>
        public virtual WpfCommentVisualTool CommentTool
        {
            get
            {
                return _commentateTool;
            }
            set
            {
                _commentateTool = value;
                if (value != null)
                {
                    visibleOnViewerCheckBox.IsEnabled = true;
                    visibleOnViewerCheckBox.IsChecked = value.Enabled;
                }
                else
                {
                    visibleOnViewerCheckBox.IsEnabled = false;
                    visibleOnViewerCheckBox.IsChecked = false;
                }
            }
        }
#endif

        #endregion



        #region Methods

        #region PUBLIC

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Searches the comment control for specified comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public ICommentControl FindCommentControl(Comment comment)
        {
            foreach (ICommentControl control in commentsLayoutPanel.Children)
            {
                ICommentControl result = control.FindCommentControl(comment);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Searches the comment control for specified comment source.
        /// </summary>
        /// <param name="source">The comment source.</param>
        public ICommentControl FindCommentControlBySource(object source)
        {
            foreach (ICommentControl control in commentsLayoutPanel.Children)
            {
                Comment comment = control.Comment.FindCommentBySource(source);
                if (comment != null)
                {
                    ICommentControl result = control.FindCommentControl(comment);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
#endif
        #endregion


        #region PRIVATE

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Handles the ImageCommentsChanged event of the CommentController.
        /// </summary>
        private void CommentController_ImageCommentsChanged(object sender, ImageEventArgs e)
        {
            if (e.Image == ImageViewer.Image)
                UpdateComments();
        }

        /// <summary>
        /// Handles the FocusedIndexChanged event of the ImageViewer control.
        /// </summary>
        private void ImageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateComments();
        }

        /// <summary>
        /// Handles the Comments.Changed event.
        /// </summary>
        private void Comments_Changed(object sender, CollectionChangeEventArgs<Comment> e)
        {
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(new CollectionChangeEventHandler<Comment>(Comments_Changed), sender, e);
            }
            else
            {
                if (e.Action == CollectionChangeActionType.RemoveItem)
                {
                    CommentControl commentControl = (CommentControl)FindCommentControl(e.OldValue);
                    if (commentControl != null)
                    {
                        commentsLayoutPanel.Children.Remove(commentControl);
                        commentControl.Comment = null;
                    }
                }
                else if (e.Action == CollectionChangeActionType.InsertItem)
                {
                    AddCommentControl(e.NewValue);
                }
            }
        }

        /// <summary>
        /// Handles the IsCommentSelectedChanged event of the CommentControl control.
        /// </summary>
        private void CommentControl_IsCommentSelectedChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (CommentTool != null)
            {
                ICommentControl control = CommentTool.FindCommentControl(((CommentControl)sender).Comment);
                if (control != null)
                    control.IsCommentSelected = e.NewValue;
            }
        }
#endif

        /// <summary>
        /// Handles the CheckedChanged event of the visibleOnViewerCheckBox control.
        /// </summary>
        private void visibleOnViewerCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (CommentTool != null)
                CommentTool.Enabled = visibleOnViewerCheckBox.IsChecked.Value == true;
#endif        
        }

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Updates the comments.
        /// </summary>
        private void UpdateComments()
        {
            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                Dispatcher.Invoke(new ThreadStart(UpdateComments));
                return;
            }

            ClearCommentsPanel();
            if (ImageViewer == null || ImageViewer.Image == null)
                return;
            CommentCollection comments = _commentController.GetComments(ImageViewer.Image);
            if (comments != _comments)
            {
                if (_comments != null)
                    _comments.Changed -= Comments_Changed;
                _comments = comments;
                if (comments != null)
                {
                    comments.Changed += Comments_Changed;
                    foreach (Comment comment in comments)
                    {
                        AddCommentControl(comment);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the comment control for specified commment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        private void AddCommentControl(Comment comment)
        {
            CommentControl control = new CommentControl();
            control.Comment = comment;
            control.AutoHeight = true;
            control.CanClose = false;
            control.CanExpand = true;
            control.IsCommentSelectedChanged += CommentControl_IsCommentSelectedChanged;
            control.Margin = new Thickness(2, 2, 0, 5);

            commentsLayoutPanel.Children.Add(control);
        }

        /// <summary>
        /// Clears the comments panel.
        /// </summary>
        private void ClearCommentsPanel()
        {
            commentsLayoutPanel.Children.Clear();
            UIElement[] comments = new UIElement[commentsLayoutPanel.Children.Count];
            commentsLayoutPanel.Children.CopyTo(comments, 0);
            commentsLayoutPanel.Children.Clear();
            foreach (UIElement control in comments)
                if (control is CommentControl)
                    ((CommentControl)control).Comment = null;
        }
#endif

        #endregion


        #endregion

    }
}
