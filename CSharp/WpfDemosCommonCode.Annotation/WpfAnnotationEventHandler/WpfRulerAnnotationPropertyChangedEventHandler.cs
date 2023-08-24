using System.Text;

using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation.Wpf.UI;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Class for handling of changes of IRuler.UnitOfMeasure property.
    /// </summary>
    public class WpfRulerAnnotationPropertyChangedEventHandler : WpfAnnotationsEventsHandler
    {

        #region Constructor

        public WpfRulerAnnotationPropertyChangedEventHandler(WpfAnnotationViewer annotationViewer)
            : base(annotationViewer, true, true)
        {
            IsEnabled = true;
        }

        #endregion



        #region Methods

        protected override void OnAnnotationDataPropertyChanged(
            AnnotationDataCollection annotationDataCollection,
            AnnotationData annotationData,
            ObjectPropertyChangedEventArgs e)
        {
            IRuler ruler = annotationData as IRuler;
            if (ruler != null)
            {
                if (e.PropertyName == "UnitOfMeasure")
                {
                    StringBuilder formatString = new StringBuilder("0.0 ");
                    switch (ruler.UnitOfMeasure)
                    {
                        case UnitOfMeasure.Inches:
                            formatString.Append("in");
                            break;
                        case UnitOfMeasure.Centimeters:
                            formatString.Append("cm");
                            break;
                        case UnitOfMeasure.Millimeters:
                            formatString.Append("mm");
                            break;
                        case UnitOfMeasure.Pixels:
                            formatString.Append("px");
                            break;
                        case UnitOfMeasure.Points:
                            formatString.Append("points");
                            break;
                        case UnitOfMeasure.DeviceIndependentPixels:
                            formatString.Append("dip");
                            break;
                        case UnitOfMeasure.Twips:
                            formatString.Append("twip");
                            break;
                        case UnitOfMeasure.Emu:
                            formatString.Append("emu");
                            break;
                    }
                    ruler.FormatString = formatString.ToString();
                }
            }
        }

        #endregion

    }
}
