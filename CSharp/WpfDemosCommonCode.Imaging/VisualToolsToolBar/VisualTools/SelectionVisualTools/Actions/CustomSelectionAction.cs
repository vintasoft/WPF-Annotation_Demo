using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Stores information about a <see cref="WpfCustomSelectionTool"/> action.
    /// </summary>
    public class CustomSelectionAction : VisualToolAction
    {

        #region Fields

        /// <summary>
        /// The activated region type action.
        /// </summary>
        CustomSelectionRegionTypeAction _activatedRegionTypeAction;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSelectionAction"/> class.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        /// <param name="text">The action text.</param>
        /// <param name="toolTip">The action tool tip.</param>
        /// <param name="icon">The action icon.</param>
        /// <param name="subActions">The sub-actions of the action.</param>
        public CustomSelectionAction(
            WpfCustomSelectionTool visualTool,
            string text,
            string toolTip,
            BitmapSource icon,
            params VisualToolAction[] subActions)
            : base(visualTool, text, toolTip, icon, subActions)
        {
            _activatedRegionTypeAction = null;

            if (subActions != null)
            {
                foreach (VisualToolAction subaction in subActions)
                {
                    if (subaction is CustomSelectionRegionTypeAction)
                        subaction.Activated += new EventHandler(Action_Activated);
                }
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Activates this action.
        /// </summary>
        public override void Activate()
        {
            base.Activate();

            WpfCustomSelectionTool visualTool = (WpfCustomSelectionTool)VisualTool;

            visualTool.SelectionChanged +=
                new EventHandler<WpfCustomSelectionChangedEventArgs>(customSelectionTool_SelectionChanged);
        }

        /// <summary>
        /// Deactivates this action.
        /// </summary>
        public override void Deactivate()
        {
            WpfCustomSelectionTool visualTool = (WpfCustomSelectionTool)VisualTool;

            visualTool.SelectionChanged -= customSelectionTool_SelectionChanged;

            base.Deactivate();
        }

        /// <summary>
        /// Selects the specified region.
        /// </summary>
        /// <param name="regionTypeAction">The region type action.</param>
        /// <returns>
        /// <b>True</b> - the specified region is selected; otherwise <b>false</b>.
        /// </returns>
        public bool SelectRegion(CustomSelectionRegionTypeAction regionTypeAction)
        {
            if (regionTypeAction == null || _activatedRegionTypeAction == regionTypeAction)
                return false;

            if (_activatedRegionTypeAction != null)
                _activatedRegionTypeAction.Deactivate();

            _activatedRegionTypeAction = regionTypeAction;

            WpfCustomSelectionTool visualTool = (WpfCustomSelectionTool)VisualTool;

            visualTool.Selection = regionTypeAction.Region;

            SetIcon(regionTypeAction.Icon);

            return true;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Selects the selection region.
        /// </summary>
        private void Action_Activated(object sender, EventArgs e)
        {
            CustomSelectionRegionTypeAction regionTypeAction = (CustomSelectionRegionTypeAction)sender;

            if (SelectRegion(regionTypeAction))
            {
                if (!IsActivated)
                    Activate();
            }
        }

        /// <summary>
        /// Updates visual tool status.
        /// </summary>
        private void customSelectionTool_SelectionChanged(object sender, WpfCustomSelectionChangedEventArgs e)
        {
            Rect bbox = e.Selection.GetBoundingBox();            
            string status = string.Format(
                        CultureInfo.InvariantCulture,
                        "Bounding box: X={0}, Y={1}, Width={2}, Height={3}",
                        Math.Round(bbox.X),
                        Math.Round(bbox.Y),
                        Math.Round(bbox.Width),
                        Math.Round(bbox.Height));

            SetStatus(status);
        }

        #endregion

        #endregion

    }
}
