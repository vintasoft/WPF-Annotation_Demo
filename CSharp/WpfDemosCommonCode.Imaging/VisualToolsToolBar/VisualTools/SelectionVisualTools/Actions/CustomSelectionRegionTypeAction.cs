using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Stores information about a <see cref="WpfSelectionRegionBase"/> action.
    /// </summary>
    public class CustomSelectionRegionTypeAction : VisualToolAction
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSelectionRegionTypeAction"/> class.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="text">The action text.</param>
        /// <param name="toolTip">The action tool tip.</param>
        /// <param name="icon">The action icon.</param>
        /// <param name="subActions">The sub-actions of the action.</param>
        public CustomSelectionRegionTypeAction(
            WpfSelectionRegionBase region,
            string text,
            string toolTip,
            BitmapSource icon,
            params VisualToolAction[] subActions)
            : base(null, text, toolTip, icon, subActions)
        {
            _region = region;
        }

        #endregion



        #region Properties

        WpfSelectionRegionBase _region;
        /// <summary>
        /// Gets the region.
        /// </summary>
        public WpfSelectionRegionBase Region
        {
            get
            {
                return _region;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this action can change the visual tool in viewer.
        /// </summary>
        /// <value>
        /// <b>true</b> if this action can change the visual tool in viewer; otherwise, <b>false</b>.
        /// </value>
        public override bool CanChangeImageViewerVisualTool
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the action button must be checked when action is activated.
        /// </summary>
        /// <value>
        /// <b>true</b> if the action button must be checked when action is activated; otherwise, <b>false</b>.
        /// </value>
        public override bool CheckActionButtonOnActivate
        {
            get
            {
                return false;
            }
        }

        #endregion

    }
}
