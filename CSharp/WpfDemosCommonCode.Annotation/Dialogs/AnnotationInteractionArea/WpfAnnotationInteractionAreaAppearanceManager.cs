using System.Windows.Controls;

using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Manages and stores the settings for annotation interaction areas of visual tool.
    /// </summary>
    public class WpfAnnotationInteractionAreaAppearanceManager : WpfInteractionAreaAppearanceManager
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfAnnotationInteractionAreaAppearanceManager"/> class.
        /// </summary>
        public WpfAnnotationInteractionAreaAppearanceManager()
            : base()
        {
        }

        #endregion



        #region Properties

        bool _isSpellCheckingEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether the spell checking of text box is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the spell checking of text box is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool IsSpellCheckingEnabled
        {
            get
            {
                return _isSpellCheckingEnabled;
            }
            set
            {
                _isSpellCheckingEnabled = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Sets the properties of specified interaction controller.
        /// </summary>
        /// <param name="controller">The controller of interaction areas.</param>
        protected override void SetTransformerProperties(IWpfInteractionController controller)
        {
            base.SetTransformerProperties(controller);

            if (controller is WpfPointBasedAnnotationRectangularTransformer)
            {
                SetResizePointSettings(((WpfPointBasedAnnotationRectangularTransformer)controller).ResizePoints);
            }
            else if (controller is WpfPointBasedAnnotationPointTransformer)
            {
                WpfPointBasedAnnotationPointTransformer pointTransformer =
                  (WpfPointBasedAnnotationPointTransformer)controller;
                if (pointTransformer != null)
                {
                    if (!(pointTransformer.PolygonPointTemplate is WpfTriangleAnnotationInteractionPoint))
                    {
                        WpfInteractionPolygonPoint point = (WpfInteractionPolygonPoint)pointTransformer.PolygonPointTemplate.Clone();
                        SetPolygonPointSettings(point);
                        pointTransformer.PolygonPointTemplate = point;
                        pointTransformer.InteractionPointBackColor = PolygonPointBackgroundColor;
                        pointTransformer.SelectedInteractionPointBackColor = SelectedPolygonPointBackgroundColor;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the text box settings.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        public override void SetTextBoxSettings(TextBox textBox)
        {
            base.SetTextBoxSettings(textBox);

            textBox.SpellCheck.IsEnabled = IsSpellCheckingEnabled;
        }

        #endregion

    }
}
