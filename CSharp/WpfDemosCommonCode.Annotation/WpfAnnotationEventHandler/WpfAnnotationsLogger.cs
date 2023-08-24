using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Wpf.Utils;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Annotation
{
    public class WpfAnnotationsLogger : WpfAnnotationsEventsHandler
    {

        #region Fields

        TextBox _outputTextBox;

        #endregion



        #region Constructor

        public WpfAnnotationsLogger(WpfAnnotationViewer annotationViewer, TextBox outputTextBox)
            : base(annotationViewer, true, true)
        {
            _outputTextBox = outputTextBox;

            annotationViewer.AnnotationTransformingStarted += new EventHandler<WpfAnnotationViewEventArgs>(annotationViewer_AnnotationTransformingStarted);
            annotationViewer.AnnotationTransformingFinished += new EventHandler<WpfAnnotationViewEventArgs>(annotationViewer_AnnotationTransformingFinished);
            annotationViewer.FocusedAnnotationViewChanged += new EventHandler<WpfAnnotationViewChangedEventArgs>(annotationViewer_SelectedAnnotationViewChanged);
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the SelectedAnnotationViewChanged event of AnnotationViewer object.
        /// </summary>
        private void annotationViewer_SelectedAnnotationViewChanged(object sender, PropertyChangedEventArgs<WpfAnnotationView> e)
        {
            if (IsEnabled)
                AddLogMessage(string.Format("SelectedAnnotationViewChanged: {0} -> {1}", GetAnnotationInfo(e.OldValue), GetAnnotationInfo(e.NewValue)));
        }

        /// <summary>
        /// Handles the AnnotationTransformingStarted event of AnnotationViewer object.
        /// </summary>
        private void annotationViewer_AnnotationTransformingStarted(object sender, WpfAnnotationViewEventArgs e)
        {
            if (IsEnabled)
                AddLogMessage(string.Format("{0}: TransformingStarted: {1}", GetAnnotationInfo(e.AnnotationView), GetInteractionControllerInfo(e.AnnotationView.InteractionController)));
        }

        /// <summary>
        /// Handles the AnnotationTransformingFinished event of AnnotationViewer object.
        /// </summary>
        private void annotationViewer_AnnotationTransformingFinished(object sender, WpfAnnotationViewEventArgs e)
        {
            if (IsEnabled)
                AddLogMessage(string.Format("{0}: TransformingFinished: {1}", GetAnnotationInfo(e.AnnotationView), GetInteractionControllerInfo(e.AnnotationView.InteractionController)));
        }

        private string GetInteractionControllerInfo(IWpfInteractionController controller)
        {
            WpfCompositeInteractionController compositeController = controller as WpfCompositeInteractionController;
            if (compositeController != null)
            {
                StringBuilder sb = new StringBuilder(string.Format("CompositeInteractionController ({0}", GetInteractionControllerInfo(compositeController.Items[0])));
                for (int i = 1; i < compositeController.Items.Count; i++)
                    sb.Append(string.Format(", {0}", GetInteractionControllerInfo(compositeController.Items[i])));
                sb.Append(")");
                return sb.ToString();
            }
            return controller.GetType().Name;
        }

        protected override void OnAnnotationDataControllerChanged(WpfAnnotationViewer viewer, PropertyChangedEventArgs<AnnotationDataController> e)
        {
            AddLogMessage("AnnotationViewer.AnnotationDataControllerChanged");
        }

        protected override void OnSelectedAnnotationViewCollectionChanged(WpfAnnotationViewer viewer, PropertyChangedEventArgs<WpfAnnotationViewCollection> e)
        {
            AddLogMessage("AnnotationViewer.SelectedAnnotationViewCollectionChanged");
        }

        protected override void OnDataCollectionChanged(AnnotationDataCollection annotationDataCollection, CollectionChangeEventArgs<AnnotationData> e)
        {
            if (e.NewValue != null && e.OldValue != null)
                AddLogMessage(string.Format("DataCollection.{0}: {1}->{2}", e.Action, GetAnnotationInfo(e.OldValue), GetAnnotationInfo(e.NewValue)));
            else if (e.NewValue != null)
                AddLogMessage(string.Format("DataCollection.{0}: {1}", e.Action, GetAnnotationInfo(e.NewValue)));
            else if (e.OldValue != null)
                AddLogMessage(string.Format("DataCollection.{0}: {1}", e.Action, GetAnnotationInfo(e.OldValue)));
            else
                AddLogMessage(string.Format("DataCollection.{0}", e.Action));
        }

        protected override void OnAnnotationDataPropertyChanged(AnnotationDataCollection annotationDataCollection, AnnotationData annotationData, ObjectPropertyChangedEventArgs e)
        {
            if (e.OldValue == null && e.NewValue == null)
                AddLogMessage(string.Format("{0}.{1}",
                    GetAnnotationInfo(annotationData),
                    e.PropertyName));
            else
                AddLogMessage(string.Format("{0}.{1}: {2} -> {3}",
                    GetAnnotationInfo(annotationData),
                    e.PropertyName,
                    e.OldValue,
                    e.NewValue));
        }

        protected override void OnMouseDown(WpfAnnotationView annotationView, MouseEventArgs e)
        {
            AddMouseEventLogMessage(annotationView, "MouseDown", e);
        }

        protected override void OnMouseEnter(WpfAnnotationView annotationView, MouseEventArgs e)
        {
            AddMouseEventLogMessage(annotationView, "MouseEnter", e);
        }

        protected override void OnMouseMove(WpfAnnotationView annotationView, MouseEventArgs e)
        {
            AddMouseEventLogMessage(annotationView, "MouseMove", e);
        }

        protected override void OnMouseUp(WpfAnnotationView annotationView, MouseEventArgs e)
        {
            AddMouseEventLogMessage(annotationView, "MouseUp", e);
        }

        protected override void OnMouseWheel(WpfAnnotationView annotationView, MouseEventArgs e)
        {
            AddMouseEventLogMessage(annotationView, "MouseWheel", e);
        }


        private void AddMouseEventLogMessage(WpfAnnotationView annotationView, string eventName, MouseEventArgs e)
        {
            // location in Viewer space
            Point locationInViewerSpace = e.GetPosition(annotationView);
            
            // DIP(annotation space) -> ImageViewer space transformation
            WpfPointTransform toViewerTransform = annotationView.GetPointTransform(Viewer, Viewer.Image);
            WpfPointTransform inverseTransform = toViewerTransform.GetInverseTransform();

            // location in Annotation (DIP) space
            Point locationInAnnotationSpace = inverseTransform.TransformPoint(locationInViewerSpace);

            // location in Annotation content space
            Point locationInAnnotationContentSpace;
            // Annotation content space -> DIP(annotation space)
            Matrix fromDipToContentSpace = annotationView.GetTransformFromContentToImageSpace();
            // DIP space -> Annotation content space
            fromDipToContentSpace.Invert();
            Point[] points = new Point[] { locationInAnnotationSpace };
            fromDipToContentSpace.Transform(points);
            locationInAnnotationContentSpace = points[0];

            AddLogMessage(string.Format("{0}.{1}: ViewerSpace={2}; ContentSpace={3}",
                GetAnnotationInfo(annotationView),
                eventName,
                locationInViewerSpace,
                locationInAnnotationContentSpace));
        }

        private string GetAnnotationInfo(WpfAnnotationView annotation)
        {
            if (annotation == null)
                return "(none)";
            string type = annotation.GetType().FullName;
            type = type.Replace("Vintasoft.Imaging.Annotation.Wpf.UI.", "");
            int index = -1;
            if (Viewer.AnnotationViewCollection != null)
                index = Viewer.AnnotationViewCollection.IndexOf(annotation);
            return string.Format("[{0}]{1}", index, type);
        }

        private string GetAnnotationInfo(AnnotationData annotation)
        {
            if (annotation == null)
                return "(none)";
            string type = annotation.GetType().FullName;
            type = type.Replace("Vintasoft.Imaging.Annotation.", "");
            int index = -1;
            if (Viewer.AnnotationDataCollection != null)
                index = Viewer.AnnotationDataCollection.IndexOf(annotation);
            return string.Format("[{0}]{1}", index, type);
        }

        private void AddLogMessage(string text)
        {
            _outputTextBox.AppendText(string.Format("{0}{1}", text, Environment.NewLine));
        }

        #endregion

    }
}
