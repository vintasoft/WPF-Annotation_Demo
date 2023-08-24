using System;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Creates visual tool action, which allows to enable/disable visual tool <see cref="WpfMagnifierTool"/> in image viewer, and adds action to the toolstrip.
    /// </summary>
    public class ZoomVisualToolActionFactory
    {

        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates visual tool action, which allows to enable/disable visual tool <see cref="WpfMagnifierTool"/> in image viewer, and adds action to the toolstrip.
        /// </summary>
        /// <param name="toolBar">The toolbar, where actions must be added.</param>
        public static void CreateActions(VisualToolsToolBar toolBar)
        {
            // create action, which allows to magnify of image region in image viewer
            MagnifierToolAction magnifierToolAction = new MagnifierToolAction(
                new WpfMagnifierTool(),
                "Magnifier Tool",
                "Magnifier",
                GetIcon("WpfMagnifierTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(magnifierToolAction);

            // create action, which allows to zoom an image region in image viewer
            VisualToolAction zoomSelectionToolAction = new VisualToolAction(
                new WpfZoomSelectionTool(),
                "Zoom Selection Tool",
                "Zoom selection",
                GetIcon("WpfZoomSelection.png"));
            // add the action to the toolstrip
            toolBar.AddAction(zoomSelectionToolAction);



            WpfZoomTool zoomTool = new WpfZoomTool();
            // if .NET version 4 or higher
            if (Environment.Version.Major >= 4)
            {
                try
                {
                    // only .NET Framework 4.0 or higher can process the events of touch screen
                    zoomTool.ProcessTouchEvents = true;
                }
                catch
                {
                }
            }

            // create action, which allows to zoom an image in image viewer
            VisualToolAction zoomToolAction = new VisualToolAction(
                zoomTool,
                "Zoom Tool",
                "Zoom",
                GetIcon("WpfZoomTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(zoomToolAction);



            // if .NET version 4 or higher
            if (Environment.Version.Major >= 4)
            {
                WpfPanTool panTool = new WpfPanTool();
                try
                {
                    // only .NET Framework 4.0 or higher can process the events of touch screen
                    panTool.ProcessTouchEvents = true;
                }
                catch
                {
                }
                WpfCompositeVisualTool zoomPanTool = new WpfCompositeVisualTool(panTool, zoomTool);
                zoomPanTool.Name = "ZoomPanTool";

                // create action, which allows to zoom and pan an image in image viewer
                VisualToolAction zoomPanToolAction = new VisualToolAction(
                    zoomPanTool,
                    "Zoom Pan Tool",
                    "Zoom Pan (touchscreen)",
                    GetIcon("WpfZoomPanTool.png"));
                // add the action to the toolstrip
                toolBar.AddAction(zoomPanToolAction);
            }
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
                string.Format("WpfDemosCommonCode.Imaging.VisualToolsToolBar.VisualTools.ZoomVisualTools.Resources.{0}", iconName);

            return DemosResourcesManager.GetResourceAsBitmap(iconPath);
        }

        #endregion

        #endregion

    }
}
