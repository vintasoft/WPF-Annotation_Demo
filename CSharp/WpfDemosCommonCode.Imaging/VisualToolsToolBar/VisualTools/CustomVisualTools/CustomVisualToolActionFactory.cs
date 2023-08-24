using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfScrollPages"/> and
    /// the composite visual tool with <see cref="WpfRectangularSelectionTool"/> and <see cref="WpfScrollPages"/>) in image viewer, and adds actions to the toolstrip.
    /// </summary>
    public class CustomVisualToolActionFactory
    {

        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfScrollPages"/> and
        /// the composite visual tool with <see cref="WpfRectangularSelectionTool"/> and <see cref="WpfScrollPages"/>) in image viewer, and adds actions to the toolstrip.
        /// </summary>
        /// <param name="toolBar">The toolbar, where actions must be added.</param>
        public static void CreateActions(VisualToolsToolBar toolBar)
        {
            // create action, which allows to scroll pages in image viewer
            VisualToolAction scrollPagesVisualToolAction = new VisualToolAction(
                new WpfScrollPages(),
                "Scroll Pages",
                "Scroll Pages",
                GetIcon("WpfScrollPagesTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(scrollPagesVisualToolAction);

            // create visual tool, which allows to select rectangular area in image viewer and scroll pages in image viewer
            WpfCompositeVisualTool selectionAndScrollPages = new WpfCompositeVisualTool(
                new WpfRectangularSelectionTool(), new WpfScrollPages());
            // create action, which allows to select rectangular area in image viewer and scroll pages in image viewer
            VisualToolAction rectangularSelectionAndScrollPagesVisualToolAction = new VisualToolAction(
                selectionAndScrollPages,
                selectionAndScrollPages.ToolName,
                selectionAndScrollPages.ToolName,
                GetIcon("WpfSelectionScrollingTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(rectangularSelectionAndScrollPagesVisualToolAction);
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
                string.Format("WpfDemosCommonCode.Imaging.VisualToolsToolBar.VisualTools.CustomVisualTools.Resources.{0}", iconName);

            return DemosResourcesManager.GetResourceAsBitmap(iconPath);
        }

        #endregion

        #endregion

    }
}
