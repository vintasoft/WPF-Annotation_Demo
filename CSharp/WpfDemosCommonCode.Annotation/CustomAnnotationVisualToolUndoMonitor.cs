using System;

using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.Undo;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Undo;


namespace WpfDemosCommonCode.Annotation
{

    /// <summary>
    /// The undo monitor that monitors the <see cref="WpfAnnotationVisualTool"/> object and
    /// adds <see cref="CompositeUndoAction"/> to an undo manager if <see cref="WpfAnnotationVisualTool"/> added an annotation.
    /// </summary>
    public class CustomAnnotationVisualToolUndoMonitor : WpfAnnotationVisualToolUndoMonitor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAnnotationVisualToolUndoMonitor"/> class.
        /// </summary>
        /// <param name="undoManager">The undo manager.</param>
        /// <param name="visualTool">The visual tool.</param>
        public CustomAnnotationVisualToolUndoMonitor(UndoManager undoManager, WpfAnnotationVisualTool visualTool)
            : base(undoManager, visualTool)
        {
        }

        /// <summary>
        /// Returns the name of the annotation.
        /// </summary>
        /// <param name="annotationView">The view of annotation.</param>
        /// <returns>
        /// The name of the annotation.
        /// </returns>
        protected override string GetAnnotationName(WpfAnnotationView annotationView)
        {
            string annotationName = annotationView.ToString();
            if (annotationName.Length > 14 &&
                annotationName.EndsWith("AnnotationView", StringComparison.InvariantCulture))
                annotationName = annotationName.Substring(0, annotationName.Length - 14);
            if (annotationName.Length > 15 &&
                annotationName.StartsWith("AnnotationDemo.", StringComparison.InvariantCulture))
                annotationName = annotationName.Substring(15, annotationName.Length - 15);
            return annotationName;
        }

    }

}
