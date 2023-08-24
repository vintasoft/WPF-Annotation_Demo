using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfRectangularSelectionTool"/> and <see cref="WpfCustomSelectionTool"/>)
    /// in image viewer, and adds actions to the toolstrip.
    /// </summary>
    public class SelectionVisualToolActionFactory
    {

        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates visual tool actions, which allow to enable/disable visual tools (<see cref="WpfRectangularSelectionTool"/> and <see cref="WpfCustomSelectionTool"/>)
        /// in image viewer, and adds actions to the toolstrip.
        /// </summary>
        /// <param name="toolBar">The toolbar, where actions must be added.</param>
        public static void CreateActions(VisualToolsToolBar toolBar)
        {
            // create action, which allows to select rectangle on image in image viewer
            RectangularSelectionAction rectangularSelectionAction =
                new RectangularSelectionAction(
                new WpfRectangularSelectionToolWithCopyPaste(),
                "Rectangular Selection",
                "Rectangular Selection",
                GetIcon("WpfRectangularSelectionTool.png"));
            // add the action to the toolstrip
            toolBar.AddAction(rectangularSelectionAction);


            // create the custom selection tool
            WpfCustomSelectionTool elipticalSelection = new WpfCustomSelectionTool();
            // set the elliptical selection as the current selection in the custom selection tool
            elipticalSelection.Selection = new WpfEllipticalSelectionRegion();

            // create action, which allows to select the elliptical image region in an image viewer
            CustomSelectionAction ellipticalSelectionAction = new CustomSelectionAction(
                elipticalSelection,
                "Elliptical Selection",
                "Elliptical Selection",
                GetIcon("WpfCustomSelectionToolEllipse.png"));
            // add the action to the toolstrip
            toolBar.AddAction(ellipticalSelectionAction);


            // the default selection region type
            CustomSelectionRegionTypeAction defaultSelectedRegion = null;
            // create action, which allows to select the custom image region in an image viewer
            CustomSelectionAction customSelectionAction =
                new CustomSelectionAction(
                    new WpfCustomSelectionTool(),
                    "Custom Selection",
                    "Custom Selection",
                    null,
                    CreateSelectionRegionTypeActions(out defaultSelectedRegion));
            // set the default selection region type
            customSelectionAction.SelectRegion(defaultSelectedRegion);
            // add the action to the toolstrip
            toolBar.AddAction(customSelectionAction);
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
                string.Format("WpfDemosCommonCode.Imaging.VisualToolsToolBar.VisualTools.SelectionVisualTools.Resources.{0}", iconName);

            return DemosResourcesManager.GetResourceAsBitmap(iconPath);
        }

        /// <summary>
        /// Returns the selection region type actions.
        /// </summary>
        /// <param name="defaultSelectedRegion">The default selected region.</param>
        /// <returns>
        /// The selection region type actions.
        /// </returns>
        private static VisualToolAction[] CreateSelectionRegionTypeActions(
            out CustomSelectionRegionTypeAction defaultSelectedRegion)
        {
            // create action, which allows to select the rectangular image region in an image viewer.
            CustomSelectionRegionTypeAction rectangleSelectionRegion =
                new CustomSelectionRegionTypeAction(
                        new WpfRectangularSelectionRegion(),
                        "Rectangle",
                        "Rectangle",
                        GetIcon("WpfCustomSelectionToolRectangle.png"));

            // create action, which allows to select the elliptical image region in an image viewer.
            CustomSelectionRegionTypeAction ellipticalSelectionRegion =
                new CustomSelectionRegionTypeAction(
                        new WpfEllipticalSelectionRegion(),
                        "Ellipse",
                        "Ellipse",
                        GetIcon("WpfCustomSelectionToolEllipse.png"));

            // create action, which allows to select the polygonal image region in an image viewer.
            CustomSelectionRegionTypeAction polygonSelectionRegion =
                new CustomSelectionRegionTypeAction(
                        new WpfPolygonalSelectionRegion(),
                        "Polygon",
                        "Polygon",
                        GetIcon("WpfCustomSelectionToolPolygon.png"));

            // create action, which allows to select the freehand polygon image region in an image viewer.
            CustomSelectionRegionTypeAction lassoSelectionRegion =
                new CustomSelectionRegionTypeAction(
                        new WpfLassoSelectionRegion(),
                        "Lasso",
                        "Lasso",
                        GetIcon("WpfCustomSelectionToolLasso.png"));

            // create action, which allows to select the curve image region in an image viewer.
            CustomSelectionRegionTypeAction curvesSelectionRegion =
                new CustomSelectionRegionTypeAction(
                        new WpfCurvilinearSelectionRegion(),
                        "Curves",
                        "Curves",
                        GetIcon("WpfCustomSelectionToolCurves.png"));

            // set the default selection region
            defaultSelectedRegion = lassoSelectionRegion;

            // returns the actions
            return new VisualToolAction[] {
                    rectangleSelectionRegion,
                    ellipticalSelectionRegion,
                    polygonSelectionRegion,
                    lassoSelectionRegion,
                    curvesSelectionRegion};
        }

        #endregion

        #endregion

    }
}
