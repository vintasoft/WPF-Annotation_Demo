using Microsoft.Win32;

using System;
using System.Windows;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Wpf.UI.VisualTools;

using WpfDemosCommonCode.Imaging.Codecs;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Stores information about a <see cref="WpfOverlayImageTool"/> action.
    /// </summary>
    public class OverlayImageToolAction : VisualToolAction
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayImageToolAction"/> class.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        /// <param name="text">The action text.</param>
        /// <param name="toolTip">The action tool tip.</param>
        /// <param name="icon">The action icon.</param>
        /// <param name="subActions">The sub-actions of the action.</param>
        public OverlayImageToolAction(
            WpfOverlayImageTool visualTool,
            string text,
            string toolTip,
            BitmapSource icon,
            params VisualToolAction[] subActions)
            : base(visualTool, text, toolTip, icon, subActions)
        {
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

            ShowVisualToolImageSelectionDialog();
        }

        /// <summary>
        /// Deactivates this action.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();

            RemoveVisualToolImage();
        }

        /// <summary>
        /// Shows the visual tool settings.
        /// </summary>
        public override void ShowVisualToolSettings()
        {
            base.ShowVisualToolSettings();

            RemoveVisualToolImage();
            ShowVisualToolImageSelectionDialog();
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Shows the image selection dialog of visual tool.
        /// </summary>
        private void ShowVisualToolImageSelectionDialog()
        {
            // create open file dialog
            OpenFileDialog openImageDialog = new OpenFileDialog();
            // set image files filters
            CodecsFileFilters.SetFilters(openImageDialog);
            // if image file selected
            if (openImageDialog.ShowDialog() == true)
            {
                try
                {
                    // get overlay tool
                    WpfOverlayImageTool tool = ((WpfOverlayImageTool)VisualTool);
                    // set overlay image
                    tool.Image = new VintasoftImage(openImageDialog.FileName);

                    // if WpfOverlayImageTool.MaintainAspectRatio must be enabled
                    if (MessageBox.Show("Do you want to maintain image aspect ratio?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        tool.MaintainAspectRatio = true;
                    else
                        tool.MaintainAspectRatio = false;
                }
                catch (Exception ex)
                {
                    // show error message
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Removes the visual tool image.
        /// </summary>
        private void RemoveVisualToolImage()
        {
            // get overlay tool
            WpfOverlayImageTool overlayImageTool = (WpfOverlayImageTool)VisualTool;
            // if image of overlay tool is specified
            if (overlayImageTool.Image != null)
            {
                // get image
                VintasoftImage image = overlayImageTool.Image;
                // reset image of overlay tool
                overlayImageTool.Image = null;
                // remove image
                image.Dispose();
            }
        }

        #endregion

        #endregion

    }
}
