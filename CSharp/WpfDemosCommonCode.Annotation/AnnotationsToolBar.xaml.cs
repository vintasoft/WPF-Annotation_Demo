using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging.Annotation.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using WpfDemosCommonCode.Office;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// A control that allows to add annotations to an image in viewer.
    /// </summary>
    public partial class AnnotationsToolBar : ToolBar
    {

        #region Nested Class

        /// <summary>
        /// Contains information about toolbar button.
        /// </summary>
        private abstract class ButtonInfo
        {

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ButtonInfo"/> class.
            /// </summary>
            /// <param name="name">The button name.</param>
            /// <param name="dropDownItems">The drop down items of button.</param>
            internal ButtonInfo(string name, params ButtonInfo[] dropDownItems)
            {
                _name = name;
                _dropDownItems = dropDownItems;
            }

            #endregion



            #region Properties

            string _name = string.Empty;
            /// <summary>
            /// Gets the button name.
            /// </summary>
            internal string Name
            {
                get
                {
                    return _name;
                }
            }

            ButtonInfo[] _dropDownItems;
            /// <summary>
            /// Gets the drop down items of button.
            /// </summary>
            internal ButtonInfo[] DropDownItems
            {
                get
                {
                    return _dropDownItems;
                }
            }

            #endregion



            #region Methods

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }

            #endregion

        }



        /// <summary>
        /// Contains information about separator.
        /// </summary>
        private class SeparatorButtonInfo : ButtonInfo
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="SeparatorButtonInfo"/> class.
            /// </summary>
            internal SeparatorButtonInfo()
                : base("SEPARATOR")
            {
            }

        }



        /// <summary>
        /// Contains information about annotation button.
        /// </summary>
        private class AnnotationButtonInfo : ButtonInfo
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="AnnotationButtonInfo"/> class.
            /// </summary>
            /// <param name="annotationType">The annotation type.</param>
            /// <param name="dropDownItems">The drop down items of annotation button.</param>
            internal AnnotationButtonInfo(
                AnnotationType annotationType,
                params ButtonInfo[] dropDownItems)
                : base(AnnotationNameFactory.GetAnnotationName(annotationType), dropDownItems)
            {
                _annotationType = annotationType;
            }



            AnnotationType _annotationType = AnnotationType.Unknown;
            /// <summary>
            /// Gets the annotation type.
            /// </summary>
            internal AnnotationType AnnotationType
            {
                get
                {
                    return _annotationType;
                }
            }

        }



        /// <summary>
        /// Contains information about custom button.
        /// </summary>
        private class CustomButtonInfo : ButtonInfo
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomButtonInfo"/> class.
            /// </summary>
            /// <param name="name">The button name.</param>
            /// <param name="iconName">The button icon name.</param>
            /// <param name="buttonClickHandler">The button click event handler.</param>
            /// <param name="dropDownItems">The drop down items of button.</param>
            internal CustomButtonInfo(
                string name,
                string iconName,
                EventHandler buttonClickHandler,
                params ButtonInfo[] dropDownItems)
                : base(name, dropDownItems)
            {
                _iconName = iconName;
                _buttonClickHandler = buttonClickHandler;
            }



            string _iconName;
            /// <summary>
            /// Gets the button icon name.
            /// </summary>
            internal string IconName
            {
                get
                {
                    return _iconName;
                }
            }

            EventHandler _buttonClickHandler;
            /// <summary>
            /// Gets the button click event handler.
            /// </summary>
            internal EventHandler ButtonClickHandler
            {
                get
                {
                    return _buttonClickHandler;
                }
            }

        }

        #endregion



        #region Fields

        /// <summary>
        /// Dictionary: the tool bar menu item => the annotation type.
        /// </summary>
        Dictionary<Control, AnnotationType> _menuItemToAnnotationType =
            new Dictionary<Control, AnnotationType>();

        /// <summary>
        /// Dictionary: the annotation type => the tool bar menu item.
        /// </summary>
        Dictionary<AnnotationType, Control> _annotationTypeToMenuItem =
            new Dictionary<AnnotationType, Control>();

        /// <summary>
        /// Dictionary: the tool bar menu item => the event handler.
        /// </summary>
        Dictionary<Control, EventHandler> _menuItemToEventHandler =
            new Dictionary<Control, EventHandler>();

        /// <summary>
        /// Dictionary: annotation data => annotation view.
        /// </summary>
        Dictionary<AnnotationData, WpfAnnotationView> _annotationDataToAnnotationView =
            new Dictionary<AnnotationData, WpfAnnotationView>();

        /// <summary>
        /// The open image file dialog.
        /// </summary>
        OpenFileDialog _openImageDialog;

        /// <summary>
        /// The default visual tool of annotation viewer.
        /// </summary>
        WpfVisualTool _annotationViewerDefaultVisualTool;

        /// <summary>
        /// The annotation button, which is currently checked in the control.
        /// </summary>
        Control _checkedAnnotationButton = null;

        /// <summary>
        /// The type of annotation, which is building now.
        /// </summary>
        AnnotationType _buildingAnnotationType = AnnotationType.Unknown;

        /// <summary>
        /// The name of image file for embedded or referenced image annotation.
        /// </summary>
        /// <remarks>
        /// This field is used when annotations must be built continuously.
        /// </remarks>
        string _embeddedOrReferencedImageFileName = null;

        /// <summary>
        /// Indicates that the interaction mode is changing.
        /// </summary>
        bool _isInteractionModeChanging = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationsToolBar"/> class.
        /// </summary>
        public AnnotationsToolBar()
            : base()
        {
            InitializeComponent();

            InitializeAnnotationButtons();

            AnnotationViewer = null;
        }

        #endregion



        #region Properties

        WpfAnnotationViewer _annotationViewer;
        /// <summary>
        /// Gets or sets the <see cref="WpfAnnotationViewer"/> associated with
        /// this <see cref="AnnotationsToolBar"/>.
        /// </summary>
        public WpfAnnotationViewer AnnotationViewer
        {
            get
            {
                return _annotationViewer;
            }
            set
            {
                UnsubscribeFromAnnotationViewerEvents(_annotationViewer);

                _annotationViewer = value;
                if (_annotationViewer != null)
                    // save reference to the default visual tool of annotation viewer
                    _annotationViewerDefaultVisualTool = _annotationViewer.VisualTool;

                SubscribeToAnnotationViewerEvents(_annotationViewer);
            }
        }

        AnnotationCommentBuilder _commentBuilder = null;
        /// <summary>
        /// Gets or sets the comment builder.
        /// </summary>
        public AnnotationCommentBuilder CommentBuilder
        {
            get
            {
                return _commentBuilder;
            }
            set
            {
                _commentBuilder = value;
            }
        }

        ImageViewerToolBar _viewerToolBar = null;
        /// <summary>
        /// Gets or sets the <see cref="ImageViewerToolBar"/> associated with
        /// this <see cref="AnnotationsToolBar"/>.
        /// </summary>
        public ImageViewerToolBar ViewerToolBar
        {
            get
            {
                return _viewerToolBar;
            }
            set
            {
                _viewerToolBar = value;
            }
        }

        bool _needBuildAnnotationsContinuously = false;
        /// <summary>
        /// Gets or sets a value indicating whether the annotations must be built continuously.
        /// </summary>
        public bool NeedBuildAnnotationsContinuously
        {
            get
            {
                return _needBuildAnnotationsContinuously;
            }
            set
            {
                _needBuildAnnotationsContinuously = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Adds an annotation to an image and starts the annotation building.
        /// </summary>
        /// <param name="annotationType">The annotation type.</param>
        /// <returns>
        /// The annotation view.
        /// </returns>
        public WpfAnnotationView AddAndBuildAnnotation(AnnotationType annotationType)
        {
            // if annotation viewer is not specified
            if (AnnotationViewer == null || AnnotationViewer.Image == null)
                return null;

            // if current visual tool of annotation viewer differs from the default visual tool of annotation viewer
            if (_annotationViewer.VisualTool != _annotationViewerDefaultVisualTool)
            {
                // set the default visual tool of annotation viewer as  current visual tool
                _annotationViewer.VisualTool = _annotationViewerDefaultVisualTool;
            }

            // if the focused annotation is building
            if (AnnotationViewer.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                // cancel building the focused annotation
                AnnotationViewer.AnnotationVisualTool.CancelAnnotationBuilding();


            _isInteractionModeChanging = true;
            // use the Author mode for annotation visual tool
            _annotationViewer.AnnotationInteractionMode = AnnotationInteractionMode.Author;
            _isInteractionModeChanging = false;


            WpfAnnotationView annotationView = null;
            try
            {
                // save the annotation type
                _buildingAnnotationType = annotationType;

                // select the button of annotation
                SelectAnnotationButton(annotationType);

                // create the annotation view
                annotationView = CreateAnnotationView(annotationType);

                // if annotation view is created
                if (annotationView != null)
                {
                    // start the annotation building
                    AnnotationViewer.AddAndBuildAnnotation(annotationView);
                }
                else
                {
                    if (annotationType != AnnotationType.Unknown)
                        EndAnnotationBuilding();
                }
            }
            catch (InvalidOperationException ex)
            {
                // show error message
                DemosTools.ShowErrorMessage("Building annotation", ex);

                // unselect buttons
                SelectAnnotationButton(AnnotationType.Unknown);

                annotationView = null;
            }

            return annotationView;
        }

        #endregion


        #region PRIVATE

        #region Annotation buttons

        /// <summary>
        /// Initializes the annotations buttons.
        /// </summary>
        private void InitializeAnnotationButtons()
        {
            // create information about annotation buttons of this tool strip

            ButtonInfo[] annotationButtons = {

                // Rectangle
                new AnnotationButtonInfo(AnnotationType.Rectangle,
                    // Rectangle -> Cloud Rectangle
                    new AnnotationButtonInfo(AnnotationType.CloudRectangle)),

                // Ellipse
                new AnnotationButtonInfo(AnnotationType.Ellipse,
                    // Ellipse -> Cloud Ellipse
                    new AnnotationButtonInfo(AnnotationType.CloudEllipse)),

                // Highlight
                new AnnotationButtonInfo(AnnotationType.Highlight,
                    // Highlight -> Cloud Highlight
                    new AnnotationButtonInfo(AnnotationType.CloudHighlight)),

                // Text Highlight
                new AnnotationButtonInfo(AnnotationType.TextHighlight,
                    // Text Highlight -> Freehand Higlight
                    new AnnotationButtonInfo(AnnotationType.FreehandHighlight),
                    // Text Highlight -> Polygon Highlight
                    new AnnotationButtonInfo(AnnotationType.PolygonHighlight),
                    // Text Highlight -> Freehand Polygon Highlight
                    new AnnotationButtonInfo(AnnotationType.FreehandPolygonHighlight)),

                new SeparatorButtonInfo(),


                // Embedded Image
                new AnnotationButtonInfo(AnnotationType.EmbeddedImage),

                // Referenced Image
                new AnnotationButtonInfo(AnnotationType.ReferencedImage),
                 
                // -----
                new SeparatorButtonInfo(),

                // Empty Document Annotation
                new AnnotationButtonInfo(AnnotationType.EmptyDocument),
                
                // Chart Annotation
                new AnnotationButtonInfo(AnnotationType.Chart),

                // Office Annotation
                new AnnotationButtonInfo(AnnotationType.OfficeDocument),

                // -----
                new SeparatorButtonInfo(),
                

                // Text
                new AnnotationButtonInfo(AnnotationType.Text,
                    // Text -> Cloud Text
                    new AnnotationButtonInfo(AnnotationType.CloudText)),

                // Sticky Note
                new AnnotationButtonInfo(AnnotationType.StickyNote),

                // Free Text
                new AnnotationButtonInfo(AnnotationType.FreeText,
                    // Free Text -> Cloud Free Text
                    new AnnotationButtonInfo(AnnotationType.CloudFreeText)),

                // Rubber Stamp
                new AnnotationButtonInfo(AnnotationType.RubberStamp),

                // Link
                new AnnotationButtonInfo(AnnotationType.Link),

                // Arrow
                new AnnotationButtonInfo(AnnotationType.Arrow),

                // Double Arrow
                new AnnotationButtonInfo(AnnotationType.DoubleArrow),

                new SeparatorButtonInfo(),


                // Line
                new AnnotationButtonInfo(AnnotationType.Line),

                // Lines
                new AnnotationButtonInfo(AnnotationType.Lines,
                    // Lines -> Cloud Lines
                    new AnnotationButtonInfo(AnnotationType.CloudLines),
                    // Lines -> Triangle Lines
                    new AnnotationButtonInfo(AnnotationType.TriangleLines)),

                // Lines with Interpolation
                new AnnotationButtonInfo(AnnotationType.LinesWithInterpolation,
                    // Lines with Interpolation -> Cloud Lines with Interpolation
                    new AnnotationButtonInfo(AnnotationType.CloudLinesWithInterpolation)),

                // Inc
                new AnnotationButtonInfo(AnnotationType.Ink),

                // Polygon
                new AnnotationButtonInfo(AnnotationType.Polygon,
                    // Polygon -> Cloud Polygon
                    new AnnotationButtonInfo(AnnotationType.CloudPolygon),
                    // Polygon -> Triangle Polygon 
                    new AnnotationButtonInfo(AnnotationType.TrianglePolygon)),

                // Polygon with Interpolation
                new AnnotationButtonInfo(AnnotationType.PolygonWithInterpolation,
                    // Polygon with Interpolation -> Cloud Polygon with Interpolation
                    new AnnotationButtonInfo(AnnotationType.CloudPolygonWithInterpolation)),

                // Freehand Polygon
                new AnnotationButtonInfo(AnnotationType.FreehandPolygon),

                // Ruler
                new AnnotationButtonInfo(AnnotationType.Ruler),

                // Rulers
                new AnnotationButtonInfo(AnnotationType.Rulers),

                // Angle
                new AnnotationButtonInfo(AnnotationType.Angle),

                 // Arc
                new AnnotationButtonInfo(AnnotationType.Arc,
                    // Arc -> With Arrow
                    new AnnotationButtonInfo(AnnotationType.ArcWithArrow),
                    // Arc -> With Double Arrow
                    new AnnotationButtonInfo(AnnotationType.ArcWithDoubleArrow)),

                new SeparatorButtonInfo(),


                // Triangle
                new AnnotationButtonInfo(AnnotationType.Triangle,
                    // Triangle -> Cloud Triangle
                    new AnnotationButtonInfo(AnnotationType.CloudTriangle)),

                // Mark
                new AnnotationButtonInfo(AnnotationType.Mark),

                new SeparatorButtonInfo(),


                new CustomButtonInfo(
                    "Add New Comment",
                    "AddNewComment",
                    AddNewCommentButton_Click),

                new CustomButtonInfo(
                    "Add New Comment To Annotation",
                    "AddNewCommentToAnnotation",
                    AddCommentToAnnotationButton_Click),
            };


            _menuItemToEventHandler.Clear();
            _menuItemToAnnotationType.Clear();
            _annotationTypeToMenuItem.Clear();

            // initialize the annotation buttons of this toolbar
            InitializeButtons(Items, annotationButtons);
        }

        /// <summary>
        /// Initializes the buttons.
        /// </summary>
        /// <param name="buttonCollection">The button collection to which new button must be added.</param>
        /// <param name="buttonInfos">Information about buttons.</param>
        private void InitializeButtons(
            ItemCollection buttonCollection,
            ButtonInfo[] buttonInfos)
        {
            foreach (ButtonInfo annotationButtonInfo in buttonInfos)
                InitializeButton(buttonCollection, annotationButtonInfo);
        }

        /// <summary>
        /// Creates the button and adds the button to the collection of buttons.
        /// </summary>
        /// <param name="buttonCollection">The button collection to which new button must be added.</param>
        /// <param name="buttonInfo">An information about button.</param>
        private void InitializeButton(
            ItemCollection buttonCollection,
            ButtonInfo buttonInfo)
        {
            Control annotationButton = null;
            Control controlWithEventHandler = null;

            if (buttonInfo is SeparatorButtonInfo)
            {
                annotationButton = new Separator();
            }
            else if (buttonInfo is AnnotationButtonInfo)
            {
                AnnotationButtonInfo annotationButtonInfo = (AnnotationButtonInfo)buttonInfo;

                AnnotationType annotationType = annotationButtonInfo.AnnotationType;

                annotationButton = CreateButton(
                    buttonCollection,
                    AnnotationNameFactory.GetAnnotationName(annotationType),
                    AnnotationIconNameFactory.GetAnnotationIconName(annotationType),
                    buildAnnotationButton_Click,
                    buttonInfo.DropDownItems,
                    out controlWithEventHandler);

                _menuItemToAnnotationType.Add(controlWithEventHandler, annotationType);
                _annotationTypeToMenuItem.Add(annotationType, controlWithEventHandler);
            }
            else if (buttonInfo is CustomButtonInfo)
            {
                CustomButtonInfo customButton = (CustomButtonInfo)buttonInfo;

                annotationButton = CreateButton(
                    buttonCollection,
                    customButton.Name,
                    customButton.IconName,
                    customButton.ButtonClickHandler,
                    customButton.DropDownItems,
                    out controlWithEventHandler);
            }
            else
            {
                throw new NotImplementedException();
            }

            buttonCollection.Add(annotationButton);
        }

        /// <summary>
        /// Creates the button.
        /// </summary>
        /// <param name="buttonCollection">The button collection.</param>
        /// <param name="buttonName">The button name.</param>
        /// <param name="buttonIconName">The button icon name.</param>
        /// <param name="buttonClickHandler">The button click event handler.</param>
        /// <param name="children">The button children.</param>
        /// <param name="controlWithEventHandler">The control with event handler.</param>
        private Control CreateButton(
            ItemCollection buttonCollection,
            string buttonName,
            string buttonIconName,
            EventHandler buttonClickHandler,
            ButtonInfo[] children,
            out Control controlWithEventHandler)
        {
            bool addToRoot = buttonCollection == Items;

            Control resultControl = null;
            if (addToRoot)
            {
                Button button = new Button();
                resultControl = button;
                button.Background = new SolidColorBrush(Colors.Gray);
                button.Background.Opacity = 0;
                button.ToolTip = buttonName;
                _menuItemToEventHandler.Add(button, buttonClickHandler);
                button.Click += new RoutedEventHandler(button_Click);
                button.Content = LoadImageFromResource(buttonIconName);
                controlWithEventHandler = button;

                if (children != null && children.Length != 0)
                {
                    buttonCollection.Add(resultControl);

                    ComboBox comboBox = new ComboBox();
                    resultControl = comboBox;

                    comboBox.Tag = button;
                    comboBox.ToolTip = buttonName;
                    comboBox.Width = 13;
                    comboBox.VerticalAlignment = VerticalAlignment.Stretch;
                    comboBox.Margin = new Thickness(0, 2, 0, 2);
                    comboBox.BorderBrush = Brushes.Transparent;
                    comboBox.BorderThickness = new Thickness(0);
                    comboBox.Focusable = false;

                    InitializeButtons(comboBox.Items, children);
                }
            }
            else
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                _menuItemToEventHandler.Add(comboBoxItem, buttonClickHandler);
                comboBoxItem.PreviewMouseDown +=
                    new MouseButtonEventHandler(comboBoxItem_PreviewMouseDown);
                comboBoxItem.ToolTip = buttonName;
                comboBoxItem.VerticalContentAlignment = VerticalAlignment.Center;
                resultControl = comboBoxItem;
                controlWithEventHandler = comboBoxItem;

                DockPanel dockPanel = new DockPanel();
                dockPanel.Height = 20;
                dockPanel.Margin = new Thickness(0, 2, 0, 2);
                comboBoxItem.Content = dockPanel;
                dockPanel.VerticalAlignment = VerticalAlignment.Top;

                Border border = new Border();
                border.BorderThickness = new Thickness(1);
                border.CornerRadius = new CornerRadius(0);
                border.Child = LoadImageFromResource(buttonIconName);
                dockPanel.Children.Add(border);
                TextBlock textBlock = new TextBlock();
                textBlock.Margin = new Thickness(5, 0, 0, 0);
                textBlock.Height = 16;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = buttonName;
                dockPanel.Children.Add(textBlock);
            }

            return resultControl;
        }

        /// <summary>
        /// Button with event handler is clicked.
        /// </summary>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            CallEventHandler(sender as Control);
        }

        /// <summary>
        /// The combo box with event handler is clicked.
        /// </summary>
        private void comboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            CallEventHandler(sender as Control);
        }

        /// <summary>
        /// Calls the event handler of control.
        /// </summary>
        /// <param name="control">The control.</param>
        private void CallEventHandler(Control control)
        {
            EventHandler eventHandler = null;
            if (control != null && _menuItemToEventHandler.TryGetValue(control, out eventHandler))
                eventHandler(control, EventArgs.Empty);
        }

        #endregion


        #region Annotations

        /// <summary>
        /// "Build annotation" button is clicked.
        /// </summary>
        private void buildAnnotationButton_Click(object sender, EventArgs e)
        {
            AddAndBuildAnnotation((Control)sender);
        }

        /// <summary>
        /// "Add New Comment" button is clicked.
        /// </summary>
        private void AddNewCommentButton_Click(object sender, EventArgs e)
        {
            if (_commentBuilder != null &&
                _commentBuilder.CommentVisualTool != null &&
                _commentBuilder.CommentVisualTool.Enabled)
                _commentBuilder.AddNewComment();
        }

        /// <summary>
        /// "Add New Comment To Annotation" button is clicked.
        /// </summary>
        private void AddCommentToAnnotationButton_Click(object sender, EventArgs e)
        {
            if (_commentBuilder != null &&
                _commentBuilder.CommentVisualTool != null &&
                _commentBuilder.CommentVisualTool.Enabled &&
                AnnotationViewer.FocusedAnnotationData != null)
                _commentBuilder.AddCommentToAnnotation(AnnotationViewer.FocusedAnnotationData);
        }

        /// <summary>
        /// Adds and build the annotation.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        private void AddAndBuildAnnotation(Control currentItem)
        {
            // get the annotation type
            AnnotationType annotationType = _menuItemToAnnotationType[currentItem];

            // is buinding must be stopped
            if (annotationType == _buildingAnnotationType ||
                (currentItem is Button &&
                ((Button)currentItem).BorderBrush == DemosTools.SELECTED_CONTROL_BRUSH))
            {
                // stop the buiding annotations
                annotationType = AnnotationType.Unknown;
            }

            // add and build annotation
            AddAndBuildAnnotation(annotationType);
        }

        /// <summary>
        /// Creates an annotation view for specified annotation type.
        /// </summary>
        /// <param name="annotationType">The annotation type.</param>
        /// <returns>
        /// The annotation view.
        /// </returns>
        private WpfAnnotationView CreateAnnotationView(AnnotationType annotationType)
        {
            AnnotationData data = null;
            WpfAnnotationView view = null;

#if !REMOVE_OFFICE_PLUGIN
            Vintasoft.Imaging.Annotation.Office.OfficeAnnotationData officeAnnotation;
#endif

            switch (annotationType)
            {
                case AnnotationType.Rectangle:
                    data = new RectangleAnnotationData();
                    break;

                case AnnotationType.CloudRectangle:
                    view = CreateAnnotationView(AnnotationType.Rectangle);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.Ellipse:
                    data = new EllipseAnnotationData();
                    break;

                case AnnotationType.CloudEllipse:
                    view = CreateAnnotationView(AnnotationType.Ellipse);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.Highlight:
                    data = new HighlightAnnotationData();
                    break;

                case AnnotationType.CloudHighlight:
                    view = CreateAnnotationView(AnnotationType.Highlight);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.FreehandHighlight:
                    view = CreateAnnotationView(AnnotationType.FreehandLines);
                    WpfLinesAnnotationView linesView = (WpfLinesAnnotationView)view;
                    linesView.BlendingMode = BlendingMode.Multiply;
                    linesView.Outline.Width = 12;
                    linesView.Outline.Color = System.Drawing.Color.Yellow;
                    break;

                case AnnotationType.PolygonHighlight:
                    view = CreateAnnotationView(AnnotationType.Polygon);
                    WpfPolygonAnnotationView polygonView = (WpfPolygonAnnotationView)view;
                    polygonView.Border = false;
                    polygonView.BlendingMode = BlendingMode.Multiply;
                    polygonView.FillBrush = new AnnotationSolidBrush(System.Drawing.Color.Yellow);
                    break;

                case AnnotationType.FreehandPolygonHighlight:
                    view = CreateAnnotationView(AnnotationType.FreehandPolygon);
                    WpfPolygonAnnotationView freehandPolygonView = (WpfPolygonAnnotationView)view;
                    freehandPolygonView.Border = false;
                    freehandPolygonView.BlendingMode = BlendingMode.Multiply;
                    freehandPolygonView.FillBrush = new AnnotationSolidBrush(System.Drawing.Color.Yellow);
                    break;

                case AnnotationType.TextHighlight:
                    HighlightAnnotationData textHighlight = new HighlightAnnotationData();
                    textHighlight.Border = false;
                    textHighlight.Outline.Color = System.Drawing.Color.Yellow;
                    textHighlight.FillBrush = new AnnotationSolidBrush(System.Drawing.Color.FromArgb(255, 255, 128));
                    textHighlight.BlendingMode = BlendingMode.Multiply;
                    data = textHighlight;
                    break;

                case AnnotationType.ReferencedImage:
                    if (string.IsNullOrEmpty(_embeddedOrReferencedImageFileName))
                        _embeddedOrReferencedImageFileName = GetImageFilePath();

                    ReferencedImageAnnotationData referencedImage = new ReferencedImageAnnotationData();
                    referencedImage.Filename = _embeddedOrReferencedImageFileName;
                    data = referencedImage;
                    break;              

                case AnnotationType.EmbeddedImage:
                    if (string.IsNullOrEmpty(_embeddedOrReferencedImageFileName))
                        _embeddedOrReferencedImageFileName = GetImageFilePath();

                    if (string.IsNullOrEmpty(_embeddedOrReferencedImageFileName))
                        return null;

                    VintasoftImage embeddedImage;
                    try
                    {
                        embeddedImage = new VintasoftImage(_embeddedOrReferencedImageFileName, true);
                    }
                    catch (Exception ex)
                    {
                        _embeddedOrReferencedImageFileName = string.Empty;
                        DemosTools.ShowErrorMessage("Embedded Annotation", ex);
                        return null;
                    }

                    data = new EmbeddedImageAnnotationData(embeddedImage, true);
                    break;

#if !REMOVE_OFFICE_PLUGIN
                case AnnotationType.OfficeDocument:
                    try
                    {
                        Stream documentStream = OfficeDemosTools.SelectOfficeDocument();
                        if (documentStream == null)
                            return null;
                        data = new Vintasoft.Imaging.Annotation.Office.OfficeAnnotationData(documentStream, true);
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage("Office annotation", ex);
                        return null;
                    }
                    break;

                case AnnotationType.EmptyDocument:
                    officeAnnotation = new Vintasoft.Imaging.Annotation.Office.OfficeAnnotationData(DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx"), true);
                    officeAnnotation.AutoHeight = true;
                    data = officeAnnotation;
                    break;

                case AnnotationType.Chart:
                    try
                    {

                        Stream documentStream = OfficeDemosTools.SelectChartResource();
                        if (documentStream == null)
                            return null;
                        officeAnnotation = new Vintasoft.Imaging.Annotation.Office.OfficeAnnotationData(documentStream, true);
                        officeAnnotation.UseGraphicObjectRelativeSize = true;
                        data = officeAnnotation;
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage("Chart annotation", ex);
                        return null;
                    }
                    break;
#endif

                case AnnotationType.Text:
                    TextAnnotationData text = new TextAnnotationData();
                    text.Text = "Text";
                    data = text;
                    break;

                case AnnotationType.CloudText:
                    view = CreateAnnotationView(AnnotationType.Text);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.StickyNote:
                    StickyNoteAnnotationData stickyNote = new StickyNoteAnnotationData();
                    stickyNote.FillBrush = new AnnotationSolidBrush(System.Drawing.Color.Yellow);
                    data = stickyNote;
                    break;

                case AnnotationType.FreeText:
                    FreeTextAnnotationData freeText = new FreeTextAnnotationData();
                    freeText.Text = "Free Text";
                    data = freeText;
                    break;

                case AnnotationType.CloudFreeText:
                    view = CreateAnnotationView(AnnotationType.FreeText);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.RubberStamp:
                    StampAnnotationData stamp = new StampAnnotationData();
                    stamp.Text = "Rubber stamp";
                    data = stamp;
                    break;

                case AnnotationType.Link:
                    data = new LinkAnnotationData();
                    break;

                case AnnotationType.Arrow:
                    data = new ArrowAnnotationData();
                    break;

                case AnnotationType.DoubleArrow:
                    ArrowAnnotationData doubleArrow = new ArrowAnnotationData();
                    doubleArrow.BothCaps = true;
                    data = doubleArrow;
                    break;

                case AnnotationType.Line:
                    data = new LineAnnotationData();
                    break;

                case AnnotationType.Lines:
                    data = new LinesAnnotationData();
                    break;

                case AnnotationType.CloudLines:
                    view = CreateAnnotationView(AnnotationType.Lines);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.TriangleLines:
                    view = CreateAnnotationView(AnnotationType.Lines);
                    SetLineStyle(view, AnnotationLineStyle.Triangle);
                    break;

                case AnnotationType.LinesWithInterpolation:
                    LinesAnnotationData lines = new LinesAnnotationData();
                    lines.UseInterpolation = true;
                    data = lines;
                    break;

                case AnnotationType.CloudLinesWithInterpolation:
                    view = CreateAnnotationView(AnnotationType.LinesWithInterpolation);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.FreehandLines:
                    view = WpfAnnotationViewFactory.CreateView(new LinesAnnotationData());
                    WpfPointBasedAnnotationFreehandBuilder builder =
                        new WpfPointBasedAnnotationFreehandBuilder((IWpfPointBasedAnnotation)view, 1, 1);
                    builder.FinishBuildingByDoubleMouseClick = false;
                    view.Builder = builder;
                    break;

                case AnnotationType.Polygon:
                    data = new PolygonAnnotationData();
                    break;

                case AnnotationType.CloudPolygon:
                    view = CreateAnnotationView(AnnotationType.Polygon);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.TrianglePolygon:
                    view = CreateAnnotationView(AnnotationType.Polygon);
                    SetLineStyle(view, AnnotationLineStyle.Triangle);
                    break;

                case AnnotationType.PolygonWithInterpolation:
                    PolygonAnnotationData polygonWithInterpolation = new PolygonAnnotationData();
                    polygonWithInterpolation.UseInterpolation = true;
                    data = polygonWithInterpolation;
                    break;

                case AnnotationType.CloudPolygonWithInterpolation:
                    view = CreateAnnotationView(AnnotationType.PolygonWithInterpolation);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.FreehandPolygon:
                    view = WpfAnnotationViewFactory.CreateView(new PolygonAnnotationData());
                    view.Builder = new WpfPointBasedAnnotationFreehandBuilder((IWpfPointBasedAnnotation)view, 2, 1);
                    break;

                case AnnotationType.Ruler:
                    data = new RulerAnnotationData();
                    break;

                case AnnotationType.Rulers:
                    data = new RulersAnnotationData();
                    break;

                case AnnotationType.Angle:
                    data = new AngleAnnotationData();
                    break;

                case AnnotationType.Triangle:
                    data = new TriangleAnnotationData();
                    break;

                case AnnotationType.CloudTriangle:
                    view = CreateAnnotationView(AnnotationType.Triangle);
                    SetLineStyle(view, AnnotationLineStyle.Cloud);
                    break;

                case AnnotationType.Mark:
                    data = new MarkAnnotationData();
                    break;

                case AnnotationType.Arc:
                    data = new ArcAnnotationData();
                    break;

                case AnnotationType.ArcWithArrow:
                    data = new ArcAnnotationData();
                    data.Outline.StartCap.Style = LineCapStyles.Arrow;
                    break;

                case AnnotationType.ArcWithDoubleArrow:
                    data = new ArcAnnotationData();
                    data.Outline.StartCap.Style = LineCapStyles.Arrow;
                    data.Outline.EndCap.Style = LineCapStyles.Arrow;
                    break;

                case AnnotationType.Ink:
                    data = new InkAnnotationData();
                    break;

                default:
                    return null;
            }

            // if annotation view is created
            if (view != null)
                return view;

            // create the annotation view for specified annotation data
            return WpfAnnotationViewFactory.CreateView(data);
        }

        /// <summary>
        /// Subscribes to the link annotation view events.
        /// </summary>
        /// <param name="view">The view.</param>
        private void SubscribeToLinkAnnotationViewEvents(WpfAnnotationView view)
        {
            if (view != null)
            {
                if (view is WpfLinkAnnotationView)
                {
                    ((WpfLinkAnnotationView)view).LinkClicked += new EventHandler<AnnotationLinkClickedEventArgs>(OnLinkClicked);
                }
                else if (view is WpfCompositeAnnotationView)
                {
                    foreach (WpfAnnotationView child in (WpfCompositeAnnotationView)view)
                        SubscribeToLinkAnnotationViewEvents(child);
                }
            }
        }

        /// <summary>
        /// Unsubscribes from the link annotation view events.
        /// </summary>
        /// <param name="linkView">The view.</param>
        private void UnsubscribeFromLinkAnnotationViewEvents(WpfAnnotationView view)
        {
            if (view != null)
            {
                if (view is WpfLinkAnnotationView)
                {
                    ((WpfLinkAnnotationView)view).LinkClicked -= OnLinkClicked;
                }
                else if (view is WpfCompositeAnnotationView)
                {
                    foreach (WpfAnnotationView child in (WpfCompositeAnnotationView)view)
                        UnsubscribeFromLinkAnnotationViewEvents(child);
                }
            }
        }

        /// <summary> 
        /// Opens the link of link annotation.
        /// </summary>
        private void OnLinkClicked(object sender, AnnotationLinkClickedEventArgs e)
        {
            // open the link
            DemosTools.OpenBrowser(e.LinkText);
        }

        /// <summary>
        /// Ends the annotation building.
        /// </summary>
        private void EndAnnotationBuilding()
        {
            _buildingAnnotationType = AnnotationType.Unknown;

            SelectAnnotationButton(AnnotationType.Unknown);
        }

        #endregion


        #region Annotation viewer

        /// <summary>
        /// Subscribes to the annotation viewer events.
        /// </summary>
        /// <param name="annotationViewer">The annotation viewer.</param>
        private void SubscribeToAnnotationViewerEvents(WpfAnnotationViewer annotationViewer)
        {
            if (annotationViewer == null)
                return;

            annotationViewer.FocusedIndexChanging +=
                new PropertyChangedEventHandler<int>(viewer_FocusedIndexChanging);

            annotationViewer.AnnotationInteractionModeChanging +=
                new EventHandler<AnnotationInteractionModeChangingEventArgs>(viewer_AnnotationInteractionModeChanging);
            annotationViewer.AnnotationViewCollectionChanged +=
                new EventHandler<WpfAnnotationViewCollectionChangedEventArgs>(viewer_AnnotationViewCollectionChanged);

            annotationViewer.AnnotationVisualTool.AnnotationBuildingFinished +=
                new EventHandler<WpfAnnotationViewEventArgs>(viewer_AnnotationBuildingFinished);
            annotationViewer.AnnotationVisualTool.AnnotationBuildingCanceled +=
                new EventHandler<WpfAnnotationViewEventArgs>(viewer_AnnotationBuildingCanceled);

            annotationViewer.AnnotationVisualTool.Deactivated += AnnotationVisualTool_Deactivated;
        }

        /// <summary>
        /// Unsubscribes from the annotation viewer events.
        /// </summary>
        /// <param name="annotationViewer">The annotation viewer.</param>
        private void UnsubscribeFromAnnotationViewerEvents(WpfAnnotationViewer annotationViewer)
        {
            if (annotationViewer == null)
                return;

            annotationViewer.FocusedIndexChanging -= viewer_FocusedIndexChanging;

            annotationViewer.AnnotationInteractionModeChanging -= viewer_AnnotationInteractionModeChanging;
            annotationViewer.AnnotationViewCollectionChanged -= viewer_AnnotationViewCollectionChanged;

            annotationViewer.AnnotationVisualTool.AnnotationBuildingFinished -= viewer_AnnotationBuildingFinished;
            annotationViewer.AnnotationVisualTool.AnnotationBuildingCanceled -= viewer_AnnotationBuildingCanceled;

            annotationViewer.AnnotationVisualTool.Deactivated -= AnnotationVisualTool_Deactivated;
        }

        /// <summary>
        /// Annotation building is canceled.
        /// </summary>
        private void viewer_AnnotationBuildingCanceled(object sender, WpfAnnotationViewEventArgs e)
        {
            if (_isInteractionModeChanging)
                return;

            _embeddedOrReferencedImageFileName = string.Empty;
            EndAnnotationBuilding();
        }

        /// <summary>
        /// Annotaion visual tool is deactivated.
        /// </summary>
        private void AnnotationVisualTool_Deactivated(object sender, EventArgs e)
        {
            EndAnnotationBuilding();
        }

        /// <summary>
        /// Annotation building is finished.
        /// </summary>
        private void viewer_AnnotationBuildingFinished(object sender, WpfAnnotationViewEventArgs e)
        {
            if (AnnotationViewer.AnnotationViewCollection == null)
            {
                EndAnnotationBuilding();
                return;
            }

            if (!AnnotationViewer.AnnotationViewCollection.Contains(e.AnnotationView))
                return;

            // if buiding annotation type is specified
            if (_buildingAnnotationType != AnnotationType.Unknown)
            {
                // if building annotation is "Freehand lines"
                if (_buildingAnnotationType == AnnotationType.FreehandLines)
                {
                    // if annotation has less than 2 points
                    if (((LinesAnnotationData)e.AnnotationView.Data).Points.Count < 2)
                    {
                        // cancel the annotation building
                        AnnotationViewer.CancelAnnotationBuilding();
                        _buildingAnnotationType = AnnotationType.FreehandLines;
                    }
                }

                // if next annotation should be built
                if (AnnotationViewer.AnnotationInteractionMode == AnnotationInteractionMode.Author &&
                    NeedBuildAnnotationsContinuously)
                {
                    // if interaction controller of focused annotation must be changed
                    if (AnnotationViewer.FocusedAnnotationView != null)
                    {
                        // set transformer as interaction controller to the focused annotation view
                        AnnotationViewer.FocusedAnnotationView.InteractionController = AnnotationViewer.FocusedAnnotationView.Transformer;
                    }

                    // build next annotation
                    AddAndBuildAnnotation(_buildingAnnotationType);
                }
                else
                {
                    // clear file name of refereced image annotation
                    _embeddedOrReferencedImageFileName = string.Empty;

#if !REMOVE_OFFICE_PLUGIN
                    // if is chart annotation
                    if (_buildingAnnotationType == AnnotationType.Chart)
                    {
                        e.AnnotationView.InteractionController = e.AnnotationView.Transformer;
                        Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor visualEditor =
                            WpfCompositeInteractionController.FindInteractionController<Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor>(e.AnnotationView.InteractionController);
                        if (visualEditor != null)
                        {
                            OpenXmlDocumentChartDataWindow chartForm = new OpenXmlDocumentChartDataWindow();
                            chartForm.WindowStartupLocation = WindowStartupLocation.Manual;
                            chartForm.Left = Window.GetWindow(this).Left;
                            chartForm.Top = Window.GetWindow(this).Top;
                            chartForm.VisualEditor = visualEditor;
                            chartForm.ShowDialog();
                        }
                    }
#endif
                    // stop building
                    EndAnnotationBuilding();
                }
            }
        }

        /// <summary>
        /// Interaction mode of annotation is changing.
        /// </summary>
        private void viewer_AnnotationInteractionModeChanging(
            object sender,
            AnnotationInteractionModeChangingEventArgs e)
        {
            AnnotationViewer.CancelAnnotationBuilding();
        }

        /// <summary>
        /// Handles the AnnotationViewCollectionChanged event of the AnnotationViewer control.
        /// </summary>
        private void viewer_AnnotationViewCollectionChanged(
            object sender,
            WpfAnnotationViewCollectionChangedEventArgs e)
        {
            // is previous annotation collection exists
            if (e.OldValue != null)
            {
                // unsubscribe from annotation collection changed event
                e.OldValue.DataCollection.Changed -= AnnotationDataCollection_Changed;

                // for each annotation in previous annotation collection
                foreach (WpfAnnotationView annotationView in e.OldValue)
                {
                    // unsubscribe from the Link annotation events
                    UnsubscribeFromLinkAnnotationViewEvents(annotationView);
                }
            }

            // is new annotation collection exists
            if (e.NewValue != null)
            {
                // subscribe to annotation collection changed event
                e.NewValue.DataCollection.Changed +=
                    new CollectionChangeEventHandler<AnnotationData>(AnnotationDataCollection_Changed);

                // for each annotation in new annotation collection
                foreach (WpfAnnotationView annotationView in e.NewValue)
                {
                    // subscribe to the Link annotation events
                    SubscribeToLinkAnnotationViewEvents(annotationView);
                }
            }
        }

        /// <summary>
        /// Index, of focused image in viewer, is changing.
        /// </summary>
        private void viewer_FocusedIndexChanging(
            object sender,
            PropertyChangedEventArgs<int> e)
        {
            // get the annotation tool
            WpfAnnotationVisualTool annotationVisualTool = AnnotationViewer.AnnotationVisualTool;
            if (annotationVisualTool != null)
            {
                // if viewer has focused annotation
                if (annotationVisualTool.FocusedAnnotationView != null)
                {
                    // if focused annotation is building
                    if (annotationVisualTool.FocusedAnnotationView.IsBuilding)
                    {
                        // cancel the annotation building
                        annotationVisualTool.CancelAnnotationBuilding();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Changed event of the AnnotationDataCollection.
        /// </summary>
        private void AnnotationDataCollection_Changed(
            object sender,
            CollectionChangeEventArgs<AnnotationData> e)
        {
            if (e.Action == CollectionChangeActionType.Clear ||
                e.Action == CollectionChangeActionType.ClearAndAddItems)
            {
                foreach (WpfAnnotationView view in _annotationDataToAnnotationView.Values)
                {
                    // unsubscribe from the Link annotation events
                    UnsubscribeFromLinkAnnotationViewEvents(view);
                }

                _annotationDataToAnnotationView.Clear();
            }
            else
            {
                // if annotation was deleted
                if (e.OldValue != null)
                {
                    // if annotation dictionary contains annotation data
                    if (_annotationDataToAnnotationView.ContainsKey(e.OldValue))
                    {
                        // get annotation view
                        WpfAnnotationView annotationView = _annotationDataToAnnotationView[e.OldValue];

                        // unsubscribe from the link annotation events
                        UnsubscribeFromLinkAnnotationViewEvents(annotationView);

                        // remove annotation data from dictionary
                        _annotationDataToAnnotationView.Remove(e.OldValue);
                    }
                }
            }

            // if annotation was added
            if (e.NewValue != null)
            {
                // get annotation view
                WpfAnnotationView annotationView = _annotationViewer.AnnotationViewCollection.FindView(e.NewValue);

                // if annotation view found
                if (annotationView != null)
                {
                    // add annotation data in the annotation dictionary
                    _annotationDataToAnnotationView.Add(e.NewValue, annotationView);

                    // subscribe to the Link annotation events
                    SubscribeToLinkAnnotationViewEvents(annotationView);
                }
            }
        }

        #endregion


        #region Common

        /// <summary>
        /// Returns a path to an image file.
        /// </summary>
        /// <returns>
        /// A path to an image file.
        /// </returns>
        private string GetImageFilePath()
        {
            // if dialog is not created
            if (_openImageDialog == null)
            {
                // create dialog
                _openImageDialog = new OpenFileDialog();
                // set the available image formats
                CodecsFileFilters.SetFilters(_openImageDialog);
            }

            string result = null;
            // if image file is selected
            if (_openImageDialog.ShowDialog() == true)
                result = _openImageDialog.FileName;

            return result;
        }

        /// <summary>
        /// Selects the button of specified annotation type.
        /// </summary>
        /// <param name="annotationType">The annotation type.</param>
        private void SelectAnnotationButton(AnnotationType annotationType)
        {
            // if previous button must be unchecked
            if (_checkedAnnotationButton != null)
            {
                // uncheck the previous button
                SetBorderBrush(_checkedAnnotationButton, DemosTools.UNSELECTED_CONTROL_BRUSH);
                _checkedAnnotationButton = null;
            }

            // if button must be checked
            if (annotationType != AnnotationType.Unknown)
            {
                // get the button for check
                _checkedAnnotationButton = _annotationTypeToMenuItem[annotationType];
                // check the button
                SetBorderBrush(_checkedAnnotationButton, DemosTools.SELECTED_CONTROL_BRUSH);
            }
        }

        /// <summary>
        /// Sets the control border brush.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="borderBrush">The border brush.</param>
        private void SetBorderBrush(Control control, Brush borderBrush)
        {
            if (control == null)
                return;

            if (control is Button)
            {
                Button currentButton = (Button)control;

                currentButton.BorderBrush = borderBrush;
            }
            else if (control is ComboBox)
            {
                ComboBox comboBox = (ComboBox)control;

                SetBorderBrush(comboBox.Tag as Control, borderBrush);
            }
            else if (control is ComboBoxItem)
            {
                ComboBoxItem comboBoxItem = (ComboBoxItem)control;

                Panel panel = (DockPanel)comboBoxItem.Content;
                foreach (UIElement element in panel.Children)
                {
                    if (element is Border)
                    {
                        Border border = (Border)element;

                        border.BorderBrush = borderBrush;
                    }
                }

                SetBorderBrush(comboBoxItem.Parent as Control, borderBrush);
            }
            else if (control != null && control.Parent is Control)
            {
                SetBorderBrush((Control)control.Parent, borderBrush);
            }
        }

        /// <summary>
        /// Sets the annotation line style.
        /// </summary>
        /// <param name="view">The view of annotation.</param>
        /// <param name="lineStyle">The line style.</param>
        private void SetLineStyle(WpfAnnotationView view, AnnotationLineStyle lineStyle)
        {
            if (view == null)
                return;

            if (view is WpfRectangleAnnotationView)
                ((WpfRectangleAnnotationView)view).LineStyle = lineStyle;
            else if (view is WpfLinesAnnotationView)
                ((WpfLinesAnnotationView)view).LineStyle = lineStyle;
            else if (view is WpfPolygonAnnotationView)
                ((WpfPolygonAnnotationView)view).LineStyle = lineStyle;
            else if (view is WpfTextAnnotationView)
                ((WpfTextAnnotationView)view).LineStyle = lineStyle;
            else if (view is WpfFreeTextAnnotationView)
                ((WpfFreeTextAnnotationView)view).LineStyle = lineStyle;
        }

        /// <summary>
        /// Loads an image from resource.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        private Image LoadImageFromResource(string resourceName)
        {
            Image image = new Image();

            if (!Path.HasExtension(resourceName))
                resourceName += ".png";
            string pathToResource = "WpfDemosCommonCode.Annotation.Icons." + resourceName;
            image.Source = DemosResourcesManager.GetResourceAsBitmap(pathToResource);

            image.Stretch = Stretch.None;
            image.Width = 16;
            image.Height = 16;
            return image;
        }

        #endregion

        #endregion

        #endregion

    }
}
