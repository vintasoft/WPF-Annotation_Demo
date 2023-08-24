using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.Undo;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Undo;


namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// The undo monitor that monitors the <see cref="WpfAnnotationViewer"/> object and
    /// adds undo action to an undo manager if <see cref="WpfAnnotationViewCollection"/> is changed.
    /// </summary>
    public class CustomAnnotationViewerUndoMonitor : WpfAnnotationViewerUndoMonitor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAnnotationViewerUndoMonitor"/> class.
        /// </summary>
        /// <param name="undoManager">The undo manager.</param>
        /// <param name="annotationViewer">The annotation viewer.</param>
        /// <remarks>
        /// The monitor will save undo history for all images in viewer
        /// if the undo manager is <see cref="CompositeUndoManager"/>.<br />
        /// The monitor will save undo history only for focused image in viewer
        /// if the undo manager is NOT <see cref="CompositeUndoManager"/>.
        /// </remarks>
        public CustomAnnotationViewerUndoMonitor(UndoManager undoManager, WpfAnnotationViewer annotationViewer)
            : base(undoManager, annotationViewer)
        {
        }

        #endregion



        #region Methods

        /// <summary>
        /// Creates the undo monitor for annotation visual tool.
        /// </summary>
        /// <param name="undoManager">The undo manager.</param>
        /// <param name="annotationVisualTool">The annotation visual tool.</param>
        /// <returns>
        /// The undo monitor for annotation visual tool.
        /// </returns>
        protected override WpfAnnotationVisualToolUndoMonitor CreateAnnotationVisualToolUndoMonitor(
            UndoManager undoManager, WpfAnnotationVisualTool annotationVisualTool)
        {
            return new CustomAnnotationVisualToolUndoMonitor(undoManager, annotationVisualTool);
        }

        #endregion

    }
}
