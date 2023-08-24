using System.Windows;

using Vintasoft.Imaging.UI.VisualTools.UserInteraction;

using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Interaction controller that builds a mark annotation.
    /// </summary>
    public class WpfMarkAnnotationBuilder : WpfInteractionControllerBase<IWpfRectangularInteractiveObject>
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfMarkAnnotationBuilder"/> class.
        /// </summary>
        /// <param name="view">The mark annotation.</param>
        /// <param name="initialSize">The initial size of annotation.</param>
        public WpfMarkAnnotationBuilder(WpfMarkAnnotationView view)
            : base(view)
        {
            WpfImageViewerArea buildArea = new WpfImageViewerArea(
                InteractionAreaType.Hover | InteractionAreaType.Movable | InteractionAreaType.Clickable);
            buildArea.AnyActionMouseButton = true;
            InteractionAreaList.Add(buildArea);
            _initialSize = view.Size;
        }

        #endregion



        #region Properties

        Size _initialSize;
        /// <summary>
        /// Gets or sets an annotation initial size.
        /// </summary>
        public Size InitialSize
        {
            get
            {
                return _initialSize;
            }
            set
            {
                _initialSize = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Performs an interaction between user and interaction area.
        /// </summary>
        /// <param name="args">An interaction event args.</param>
        protected override void PerformInteraction(WpfInteractionEventArgs args)
        {
            InteractiveObject.SetRectangle(
                        args.Location.X - _initialSize.Width / 2, args.Location.Y - _initialSize.Height / 2,
                        args.Location.X + _initialSize.Width / 2, args.Location.Y + _initialSize.Height / 2);
            args.InteractionOccured();
            if (args.Action == InteractionAreaAction.Click)
                args.InteractionFinished = true;
        }

        #endregion

    }
}
