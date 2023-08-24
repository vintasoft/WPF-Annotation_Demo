using System.Windows;
#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.Comments;
#endif

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents a window that allows to display a comment state history.
    /// </summary>
    public partial class CommentStateHistoryWindow : Window
    {

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentStateHistoryWindow"/> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public CommentStateHistoryWindow(Comment comment)
        {
            InitializeComponent();

            commentStateHistoryControl.Comment = comment;
        }
#endif


        /// <summary>
        /// Closes the window.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
