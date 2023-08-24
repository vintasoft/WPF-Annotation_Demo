using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfCropSelectionTool"/>, <see cref="WpfDragDropSelectionTool"/>,
    /// <see cref="WpfOverlayImageTool"/> and <see cref="WpfPanTool"/>) in image viewer, and adds actions to the toolstrip.
    /// </summary>
    public class ImageProcessingVisualToolActionFactory
    {

        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfCropSelectionTool"/>, <see cref="WpfDragDropSelectionTool"/>,
        /// <see cref="WpfOverlayImageTool"/> and <see cref="WpfPanTool"/>) in image viewer, and adds actions to the toolstrip.
        /// </summary>
        /// <param name="toolBar">The toolbar, where actions must be added.</param>
        public static void CreateActions(VisualToolsToolBar toolBar)
        {
            // create action, which allows to crop an image in image viewer
            VisualToolAction cropSelectionToolAction = new VisualToolAction(
                new WpfCropSelectionTool(),
                "Crop Selection Tool",
                "Crop selection",
                GetIcon("WpfCropSelectionTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(cropSelectionToolAction);

            // create action, which allows to drag-and-drop an image region in image viewer
            VisualToolAction dragDropSelectionToolAction = new VisualToolAction(
                new WpfDragDropSelectionTool(),
                "Drag-n-drop Selection Tool",
                "Drag-n-drop selection",
                GetIcon("WpfDragDropTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(dragDropSelectionToolAction);

            // create action, which allows to overlay an image on a top of image in image viewer
            OverlayImageToolAction overlayImageToolAction = new OverlayImageToolAction(
                new WpfOverlayImageTool(),
                "Overlay Image Tool",
                "Overlay Image",
                GetIcon("WpfOverlayTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(overlayImageToolAction);

            WpfPanTool panTool = new WpfPanTool();
            try
            {
                // only .NET Framework 4.0 or higher can process the events of touch screen
                panTool.ProcessTouchEvents = true;
            }
            catch
            {
            }
            // create action, which allows to pan an image in image viewer
            VisualToolAction panToolAction = new VisualToolAction(
                panTool,
                "Pan Tool",
                "Pan",
                GetIcon("WpfPanTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(panToolAction);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Returns the visual tool icon of specified name.
        /// </summary>
        /// <param name="iconName">The visual tool icon name.</param>
        /// <returns>
        /// The visual tool icon.
        /// </returns>
        private static BitmapSource GetIcon(string iconName)
        {
            string iconPath =
                string.Format("WpfDemosCommonCode.Imaging.VisualToolsToolBar.VisualTools.ImageProcessingVisualTools.Resources.{0}", iconName);

            return DemosResourcesManager.GetResourceAsBitmap(iconPath);
        }

        #endregion

        #endregion

    }
}
