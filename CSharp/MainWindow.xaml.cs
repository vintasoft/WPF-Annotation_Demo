using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using Vintasoft.Data;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.Formatters;
using Vintasoft.Imaging.Annotation.UI;
using Vintasoft.Imaging.Annotation.Wpf.Print;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Annotation.Wpf.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Codecs.Encoders;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.Print;
using Vintasoft.Imaging.UI;
using Vintasoft.Imaging.UIActions;
using Vintasoft.Imaging.Undo;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

using WpfDemosCommonCode;
using WpfDemosCommonCode.Annotation;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using WpfDemosCommonCode.Imaging.Codecs.Dialogs;

#if !REMOVE_PDF_PLUGIN
using WpfDemosCommonCode.Pdf;
#endif


namespace WpfAnnotationDemo
{
    /// <summary>
    /// A main window of WPF annotation demo.
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Constants

        /// <summary>
        /// Determines that application window must be per-monitor DPI-aware.
        /// </summary>
        const bool PER_MONITOR_DPI_ENABLED = true;

        /// <summary>
        /// The value, in screen pixels, that defines how annotation position will be changed when user pressed arrow key.
        /// </summary>
        const int ANNOTATION_KEYBOARD_MOVE_DELTA = 2;

        /// <summary>
        /// The value, in screen pixels, that defines how annotation size will be changed when user pressed "+/-" key.
        /// </summary>
        const int ANNOTATION_KEYBOARD_RESIZE_DELTA = 4;

        #endregion



        #region Fields

        public static RoutedCommand _openCommand = new RoutedCommand();
        public static RoutedCommand _addCommand = new RoutedCommand();
        public static RoutedCommand _saveAsCommand = new RoutedCommand();
        public static RoutedCommand _closeCommand = new RoutedCommand();
        public static RoutedCommand _printCommand = new RoutedCommand();
        public static RoutedCommand _exitCommand = new RoutedCommand();
        public static RoutedCommand _undoCommand = new RoutedCommand();
        public static RoutedCommand _redoCommand = new RoutedCommand();
        public static RoutedCommand _cutCommand = new RoutedCommand();
        public static RoutedCommand _copyCommand = new RoutedCommand();
        public static RoutedCommand _pasteCommand = new RoutedCommand();
        public static RoutedCommand _deleteCommand = new RoutedCommand();
        public static RoutedCommand _deleteAllCommand = new RoutedCommand();
        public static RoutedCommand _selectAllCommand = new RoutedCommand();
        public static RoutedCommand _groupCommand = new RoutedCommand();
        public static RoutedCommand _groupAllCommand = new RoutedCommand();
        public static RoutedCommand _rotateClockwiseCommand = new RoutedCommand();
        public static RoutedCommand _rotateCounterclockwiseCommand = new RoutedCommand();
        public static RoutedCommand _aboutCommand = new RoutedCommand();


        /// <summary>
        /// Template of application title.
        /// </summary>
        string _titlePrefix = "VintaSoft WPF Annotation Demo v{0} - {1}";

        /// <summary>
        /// Selected "View - Image scale mode" menu item.
        /// </summary>
        MenuItem _imageScaleModeSelectedMenuItem;

        /// <summary>
        /// Name of the first image file in image collection of image viewer.
        /// </summary>
        string _sourceFilename;
        bool _isFileReadOnlyMode = false;
        /// <summary>
        /// Start time of image loading.
        /// </summary>
        DateTime _imageLoadingStartTime;
        /// <summary>
        /// Time of image loading.
        /// </summary>
        TimeSpan _imageLoadingTime = TimeSpan.Zero;
        OpenFileDialog openFileDialog1 = new OpenFileDialog();

        /// <summary>
        /// Filename where image collection must be saved.
        /// </summary>
        string _saveFilename;
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();

        /// <summary>
        /// Print manager.
        /// </summary>
        WpfAnnotatedImagePrintManager _printManager;

        /// <summary>
        /// List of initialized annotations.
        /// </summary>
        List<AnnotationData> _initializedAnnotations = new List<AnnotationData>();

        /// <summary>
        /// Dictionary: the menu item => the annotation type.
        /// </summary>
        Dictionary<MenuItem, AnnotationType> _menuItemToAnnotationType = new Dictionary<MenuItem, AnnotationType>();

        /// <summary>
        /// Last focused annotation.
        /// </summary>
        AnnotationData _focusedAnnotationData = null;

        WpfAnnotationInteractionAreaAppearanceManager _annotationInteractionAreaSettings;

        /// <summary>
        /// Determines that transforming of annotation is started.
        /// </summary>
        bool _isAnnotationTransforming = false;

        /// <summary>
        /// A form with annotation history.
        /// </summary>
        WpfUndoManagerHistoryWindow _historyWindow;

        /// <summary>
        /// The data storage of undo monitor.
        /// </summary>
        IDataStorage _dataStorage = null;

        /// <summary>
        /// The undo manager.
        /// </summary>
        CompositeUndoManager _undoManager;

        /// <summary>
        /// The undo monitor of annotation viewer.
        /// </summary>
        CustomAnnotationViewerUndoMonitor _annotationViewerUndoMonitor;

        /// <summary>
        /// Logger of annotation's changes.
        /// </summary>
        WpfAnnotationsLogger _annotationLogger;

        /// <summary>
        /// Determines that application window is closing.
        /// </summary>
        bool _isFormClosing = false;

        /// <summary>
        /// The comment visual tool.
        /// </summary>
        WpfCommentVisualTool _commentVisualTool;

        /// <summary>
        /// The context menu position.
        /// </summary>
        Point _contextMenuPosition;

        /// <summary>
        /// The signature-annotations.
        /// </summary>
        AnnotationDataCollection _signatureAnnotations = new AnnotationDataCollection();

        /// <summary>
        /// The index of selected signature-annotation.
        /// </summary>
        int _selectedSignatureAnnotationIndex = -1;

#if !REMOVE_PDF_PLUGIN
        /// <summary>
        /// The PDF document that is opened in this demo.
        /// </summary>
        Vintasoft.Imaging.Pdf.PdfDocument _pdfDocument = null;
#endif

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // register the evaluation license for VintaSoft Imaging .NET SDK
            Vintasoft.Imaging.ImagingGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            InitializeComponent();

            InitializeAddAnnotationMenuItems();

            Jbig2AssemblyLoader.Load();
            Jpeg2000AssemblyLoader.Load();
            DicomAssemblyLoader.Load();
            PdfAnnotationsAssemblyLoader.Load();

            ImagingTypeEditorRegistrator.Register();
            AnnotationTypeEditorRegistrator.Register();

#if !REMOVE_OFFICE_PLUGIN
            AnnotationOfficeWpfUIAssembly.Init();
#endif

            annotationViewer1.AnnotationVisualTool.ChangeFocusedItemBeforeInteraction = true;

            // init "View => Image Display Mode" menu
            singlePageMenuItem.Tag = ImageViewerDisplayMode.SinglePage;
            twoColumnsMenuItem.Tag = ImageViewerDisplayMode.TwoColumns;
            singleContinuousRowMenuItem.Tag = ImageViewerDisplayMode.SingleContinuousRow;
            singleContinuousColumnMenuItem.Tag = ImageViewerDisplayMode.SingleContinuousColumn;
            twoContinuousRowsMenuItem.Tag = ImageViewerDisplayMode.TwoContinuousRows;
            twoContinuousColumnsMenuItem.Tag = ImageViewerDisplayMode.TwoContinuousColumns;

#if REMOVE_PDF_PLUGIN
            fillAndSignMenuItem.Visibility = Visibility.Collapsed;
#endif

            AnnotationCommentController annotationCommentController = new AnnotationCommentController(annotationViewer1.AnnotationDataController);
            WpfImageViewerCommentController imageViewerCommentsController = new WpfImageViewerCommentController(annotationCommentController);

            _commentVisualTool = new WpfCommentVisualTool(imageViewerCommentsController, new CommentControlFactory());
            annotationCommentsControl1.ImageViewer = annotationViewer1;
            annotationCommentsControl1.CommentTool = _commentVisualTool;
            annotationCommentsControl1.AnnotationTool = annotationViewer1.AnnotationVisualTool;


            annotationViewer1.VisualTool = new WpfCompositeVisualTool(
                _commentVisualTool,
#if !REMOVE_OFFICE_PLUGIN
               new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                annotationViewer1.VisualTool);
            visualToolsToolBar.MandatoryVisualTool = annotationViewer1.VisualTool;
            visualToolsToolBar.ImageViewer = annotationViewer1;
            visualToolsToolBar.VisualToolsMenuItem = visualToolsMenuItem;


            NoneAction noneAction = visualToolsToolBar.FindAction<NoneAction>();
            noneAction.Activated += NoneAction_Activated;
            noneAction.Deactivated += NoneAction_Deactivated;

            annotationToolBar.CommentBuilder = new AnnotationCommentBuilder(_commentVisualTool, annotationViewer1.AnnotationVisualTool);
            annotationToolBar.AnnotationViewer = annotationViewer1;
            annotationViewer1.PreviewMouseMove += new MouseEventHandler(annotationViewer1_PreviewMouseMove);

            _annotationInteractionAreaSettings = new WpfAnnotationInteractionAreaAppearanceManager();
            _annotationInteractionAreaSettings.VisualTool = annotationViewer1.AnnotationVisualTool;
            enableSpellCheckingMenuItem.IsChecked = _annotationInteractionAreaSettings.IsSpellCheckingEnabled;

            //
            CloseCurrentFile();

            // load XPS codec
            DemosTools.LoadXpsCodec();
            // set XPS rendering requirement
            DemosTools.SetXpsRenderingRequirement(annotationViewer1, 0f);

            //
            DemosTools.SetTestFilesFolder(openFileDialog1);

            thumbnailViewer.MasterViewer = annotationViewer1;
            MainToolbar.ImageViewer = annotationViewer1;
            MainToolbar.AssociatedZoomSlider = zoomSlider;

            annotationToolBar.AnnotationViewer = annotationViewer1;
            annotationToolBar.ViewerToolBar = MainToolbar;

            //
            annotationViewer1.FocusedAnnotationViewChanged += new EventHandler<WpfAnnotationViewChangedEventArgs>(annotationViewer1_SelectedAnnotationViewChanged);
            annotationViewer1.SelectedAnnotations.Changed += new EventHandler(SelectedAnnotations_Changed);
            annotationViewer1.AutoScrollPositionExChanging += new EventHandler<PropertyChangingEventArgs<Point>>(annotationViewer1_AutoScrollPositionExChanging);
            annotationViewer1.AnnotationBuildingStarted += new EventHandler<WpfAnnotationViewEventArgs>(annotationViewer1_AnnotationBuildingStarted);
            annotationViewer1.AnnotationBuildingFinished += new EventHandler<WpfAnnotationViewEventArgs>(annotationViewer1_AnnotationBuildingFinished);
            annotationViewer1.AnnotationBuildingCanceled += new EventHandler<WpfAnnotationViewEventArgs>(annotationViewer1_AnnotationBuildingCanceled);

            //
            annotationViewer1.Images.ImageCollectionChanged += new EventHandler<ImageCollectionChangeEventArgs>(annotationViewer1_Images_ImageCollectionChanged);
            annotationViewer1.Images.ImageCollectionSavingProgress += new EventHandler<ProgressEventArgs>(SavingProgress);
            annotationViewer1.Images.ImageCollectionSavingFinished += new EventHandler(Images_ImageCollectionSavingFinished);
            annotationViewer1.Images.ImageSavingException += new EventHandler<ExceptionEventArgs>(Images_ImageSavingException);

            annotationViewer1.PreviewKeyDown += annotationViewer1_PreviewKeyDown;

            // create the print manager
            _printManager = new WpfAnnotatedImagePrintManager(annotationViewer1.AnnotationDataController);
            _printManager.PrintScaleMode = PrintScaleMode.BestFit;

            // remember current image scale mode
            _imageScaleModeSelectedMenuItem = bestFitMenuItem;



            annotationViewer1.CatchVisualToolExceptions = true;
            annotationViewer1.VisualToolException += new EventHandler<Vintasoft.Imaging.ExceptionEventArgs>(annotationViewer1_VisualToolException);
            annotationViewer1.InputGestureDelete = null;

            _undoManager = new CompositeUndoManager();
            _undoManager.UndoLevel = 100;
            _undoManager.IsEnabled = false;
            _undoManager.Changed += new EventHandler<UndoManagerChangedEventArgs>(annotationUndoManager_Changed);
            _undoManager.Navigated += new EventHandler<UndoManagerNavigatedEventArgs>(annotationUndoManager_Navigated);

            _annotationViewerUndoMonitor = new CustomAnnotationViewerUndoMonitor(_undoManager, annotationViewer1);
            _annotationViewerUndoMonitor.ShowHistoryForDisplayedImages =
                showHistoryForDisplayedImagesMenuItem.IsChecked;

            // initialize color management
            ColorManagementHelper.EnableColorManagement(annotationViewer1);

            // update the UI
            UpdateUI();

            // register view for mark annotation data
            WpfAnnotationViewFactory.RegisterViewForAnnotationData(
               typeof(MarkAnnotationData),
               typeof(WpfMarkAnnotationView));
            // register view for triangle annotation data
            WpfAnnotationViewFactory.RegisterViewForAnnotationData(
                typeof(TriangleAnnotationData),
                typeof(WpfTriangleAnnotationView));

            annotationViewer1.AnnotationDataController.AnnotationDataDeserializationException += new EventHandler<AnnotationDataDeserializationExceptionEventArgs>(AnnotationDataController_AnnotationDataDeserializationException);

            // set CustomFontProgramsController for all opened PDF documents
            CustomFontProgramsController.SetDefaultFontProgramsController();

            DocumentPasswordWindow.EnableAuthentication(annotationViewer1);

            // define custom serialization binder for correct deserialization of TriangleAnnotation v6.1 and earlier
            AnnotationSerializationBinder.Current = new CustomAnnotationSerializationBinder();

            moveAnnotationsBetweenImagesMenuItem.IsChecked = annotationViewer1.CanMoveAnnotationsBetweenImages;

            SelectionVisualToolActionFactory.CreateActions(visualToolsToolBar);
            MeasurementVisualToolActionFactory.CreateActions(visualToolsToolBar);
            ZoomVisualToolActionFactory.CreateActions(visualToolsToolBar);
            ImageProcessingVisualToolActionFactory.CreateActions(visualToolsToolBar);
            CustomVisualToolActionFactory.CreateActions(visualToolsToolBar);

            if (File.Exists(AnnotationTemplatesFilePath))
            {
                try
                {
                    using (Stream stream = File.OpenRead(AnnotationTemplatesFilePath))
                        _signatureAnnotations.AddFromStream(stream, new Resolution(96, 96));
                }
                catch
                {
                }
            }
        }

        #endregion



        #region Properties

        bool _isFileOpening = false;
        internal bool IsFileOpening
        {
            get
            {
                return _isFileOpening;
            }
            set
            {
                _isFileOpening = value;

                if (Dispatcher.Thread == Thread.CurrentThread)
                    UpdateUI();
                else
                    InvokeUpdateUI();
            }
        }

        bool _isFileSaving = false;
        internal bool IsFileSaving
        {
            get
            {
                return _isFileSaving;
            }
            set
            {
                _isFileSaving = value;

                if (Dispatcher.Thread == Thread.CurrentThread)
                    UpdateUI();
                else
                    InvokeUpdateUI();
            }
        }

        /// <summary>
        /// Gets a path to a file, where annotation templates must be saved.
        /// </summary>
        internal string AnnotationTemplatesFilePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AnnotationTemplates.xmp");
            }
        }

        #endregion



        #region Methods

        #region Window

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // if demo contains the annotation templates
                if (_signatureAnnotations.Count > 0)
                {
                    // create stream
                    using (Stream stream = File.Create(AnnotationTemplatesFilePath))
                    {
                        // create the annotation formatter
                        AnnotationVintasoftXmpFormatter annotationTemplatesFormatter = new AnnotationVintasoftXmpFormatter();
                        // serialize annotations to a file
                        annotationTemplatesFormatter.Serialize(stream, _signatureAnnotations);
                    }
                }
                // if demo does not contain the annotation templates
                else
                {
                    // remove file with annotation templates
                    File.Delete(AnnotationTemplatesFilePath);
                }
            }
            catch
            {
            }

            base.OnClosed(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.ContentRendered" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

#if !REMOVE_OFFICE_PLUGIN
            WpfDemosCommonCode.Office.OfficeDocumentVisualEditorWindow documentVisualEditorForm = new WpfDemosCommonCode.Office.OfficeDocumentVisualEditorWindow();
            documentVisualEditorForm.Owner = this;
            documentVisualEditorForm.AddVisualTool(annotationViewer1.AnnotationVisualTool);
#endif
        }


        /// <summary>
        /// Main form is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // process command line of the application
            string[] appArgs = Environment.GetCommandLineArgs();
            if (appArgs.Length > 1)
            {
                if (appArgs.Length == 2)
                {
                    try
                    {
                        OpenFile(appArgs[1]);
                    }
                    catch
                    {
                        CloseCurrentFile();
                    }
                }
                else
                {
                    for (int i = 1; i < appArgs.Length; i++)
                    {
                        try
                        {
                            annotationViewer1.Images.Add(appArgs[i]);
                        }
                        catch
                        {
                        }
                    }
                }

                // update the UI
                UpdateUI();
            }

            if (PER_MONITOR_DPI_ENABLED)
            {
                PresentationSource visual = PresentationSource.FromVisual(this);
                double xScale = 1 / visual.CompositionTarget.TransformToDevice.M11;
                double yScale = 1 / visual.CompositionTarget.TransformToDevice.M22;
                Width = Width * xScale;
                Height = Height * yScale;
                UpdateLayoutTransform(xScale, yScale);
            }
        }

        /// <summary>
        /// Updates the layout transform.
        /// </summary>
        private void UpdateLayoutTransform(double xScale, double yScale)
        {
            System.Windows.Media.Visual child = GetVisualChild(0);
            if (xScale != 1.0 && yScale != 1.0)
            {
                System.Windows.Media.ScaleTransform dpiScale = new System.Windows.Media.ScaleTransform(xScale, yScale);
                child.SetValue(Window.LayoutTransformProperty, dpiScale);
            }
            else
            {
                child.SetValue(Window.LayoutTransformProperty, null);
            }
        }

        /// <summary>
        /// Main form is closing.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            _isFormClosing = true;
            _printManager.Dispose();
        }

        /// <summary>
        /// Main form is closed.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            CloseCurrentFile();

            _annotationViewerUndoMonitor.Dispose();
            _undoManager.Dispose();

            if (_dataStorage != null)
                _dataStorage.Dispose();

            _annotationInteractionAreaSettings.Dispose();
        }

        #endregion


        #region UI state

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            // get the current status of application
            bool isFileOpening = IsFileOpening;
            bool isFileLoaded = _sourceFilename != null;
            bool isFileReadOnlyMode = _isFileReadOnlyMode;
            bool isFileEmpty = true;
            if (annotationViewer1.Images != null)
                isFileEmpty = annotationViewer1.Images.Count <= 0;
            bool isFileSaving = IsFileSaving;
            bool isImageSelected = annotationViewer1.Image != null;
            bool isAnnotationEmpty = true;
            if (isImageSelected)
                isAnnotationEmpty = annotationViewer1.AnnotationDataController[annotationViewer1.FocusedIndex].Count <= 0;
            bool isAnnotationFocused = annotationViewer1.FocusedAnnotationView != null;
            bool isAnnotationSelected = annotationViewer1.SelectedAnnotations.Count > 0;
            bool isInteractionModeAuthor = annotationViewer1.AnnotationInteractionMode == AnnotationInteractionMode.Author;
            bool isAnnotationBuilding = annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding;
            bool isCanUndo = _undoManager.UndoCount > 0 && !annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding;
            bool isCanRedo = _undoManager.RedoCount > 0 && !annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding;
#if !REMOVE_PDF_PLUGIN
            bool isPdfDocument = _pdfDocument != null && _pdfDocument.InteractiveForm != null;
#endif

            // "File" menu
            fileMenuItem.IsEnabled = !isFileOpening && !isFileSaving;
            saveCurrentFileMenuItem.IsEnabled = isFileLoaded && !isFileEmpty && !isFileReadOnlyMode;
            saveAsMenuItem.IsEnabled = !isFileEmpty;
            saveToMenuItem.IsEnabled = !isFileEmpty;
            closeMenuItem.IsEnabled = !isFileEmpty;
            printMenuItem.IsEnabled = !isFileEmpty;

            // "View" menu
            MainToolbar.IsEnabled = !isFileOpening && !isFileSaving;
            moveAnnotationsBetweenImagesMenuItem.IsEnabled =
                annotationViewer1.DisplayMode != ImageViewerDisplayMode.SinglePage;

            // update "View => Image Display Mode" menu
            singlePageMenuItem.IsChecked = false;
            twoColumnsMenuItem.IsChecked = false;
            singleContinuousRowMenuItem.IsChecked = false;
            singleContinuousColumnMenuItem.IsChecked = false;
            twoContinuousRowsMenuItem.IsChecked = false;
            twoContinuousColumnsMenuItem.IsChecked = false;
            switch (annotationViewer1.DisplayMode)
            {
                case ImageViewerDisplayMode.SinglePage:
                    singlePageMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoColumns:
                    twoColumnsMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.SingleContinuousRow:
                    singleContinuousRowMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.SingleContinuousColumn:
                    singleContinuousColumnMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoContinuousRows:
                    twoContinuousRowsMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoContinuousColumns:
                    twoContinuousColumnsMenuItem.IsChecked = true;
                    break;
            }

            // "Edit" menu
            editMenuItem.IsEnabled = !isFileEmpty;
            if (!editMenuItem.IsEnabled)
                CloseHistoryWindow();
            enableUndoRedoMenuItem.IsChecked = _undoManager.IsEnabled;
            undoMenuItem.IsEnabled = _undoManager.IsEnabled && !isFileOpening && !isFileSaving && isCanUndo;
            redoMenuItem.IsEnabled = _undoManager.IsEnabled && !isFileOpening && !isFileSaving && isCanRedo;
            undoRedoSettingsMenuItem.IsEnabled = _undoManager.IsEnabled && !isFileOpening && !isFileSaving &&
                !annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding;
            historyDialogMenuItem.IsEnabled = _undoManager.IsEnabled && !isFileOpening && !isFileSaving;

            // "Annotations" menu
            //
            annotationsInfoMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            interactionModeMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            loadFromFileMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            addAnnotationMenuItem.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor;
            buildAnnotationsContinuouslyMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            bringToBackMenuItem1.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor && !isAnnotationBuilding;
            bringToFrontMenuItem1.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor && !isAnnotationBuilding;
            //
            multiSelectMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            groupSelectedMenuItem.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor && !isAnnotationBuilding;
            groupAllMenuItem.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor && !isAnnotationBuilding;
            //
            rotateImageWithAnnotationsMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            burnAnnotationsOnImageMenuItem.IsEnabled = !isAnnotationEmpty;
            cloneImageWithAnnotationsMenuItem.IsEnabled = !isFileOpening && !isFileEmpty;
            //
            saveToFileMenuItem.IsEnabled = !isAnnotationEmpty;

            // "FillAndSign" menu
            fillAndSignMenuItem.IsEnabled = !isFileOpening && !isFileEmpty && isInteractionModeAuthor;
#if !REMOVE_PDF_PLUGIN
            verifySignaturesMenuItem.IsEnabled = isPdfDocument;
#endif

            // annotation viewer context menu
            annotationViewerMenu.IsEnabled = !isFileOpening && !isFileEmpty;
            saveImageWithAnnotationsMenuItem.IsEnabled = !isAnnotationEmpty;
            burnAnnotationsMenuItem.IsEnabled = !isAnnotationEmpty;
            copyImageToClipboardMenuItem.IsEnabled = isImageSelected;
            deleteImageMenuItem.IsEnabled = isImageSelected && !isFileSaving;
            bringToBackMenuItem.IsEnabled = isAnnotationFocused && isAnnotationSelected;
            bringToFrontMenuItem.IsEnabled = isAnnotationFocused && isAnnotationSelected;

            // annotation tool strip 
            annotationToolBar.IsEnabled = !isFileOpening && !isFileEmpty;

            // selection mode
            selectionModeToolBar.IsEnabled = !isFileOpening && !isFileEmpty;

            // zoom
            zoomSlider.IsEnabled = !isFileOpening && !isFileEmpty;

            // thumbnailViewer1 & annotationViewer1 & propertyGrid1 & annotationComboBox
            panel5.IsEnabled = !IsFileOpening && !isFileEmpty;
            if (annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                annotationComboBox.IsEnabled = false;
            else
                annotationComboBox.IsEnabled = true;

            // viewer tool strip
            MainToolbar.IsEnabled = !isFileOpening;
            MainToolbar.SaveButtonEnabled = !isFileEmpty && !IsFileSaving;
            MainToolbar.PrintButtonEnabled = !isFileEmpty && !IsFileSaving;

            //
            string str = Path.GetFileName(_sourceFilename);
            if (_isFileReadOnlyMode)
                str += " [Read Only]";
            Title = string.Format(_titlePrefix, ImagingGlobalSettings.ProductVersion, str);
        }

        /// <summary>
        /// Updates the UI safely.
        /// </summary>
        private void InvokeUpdateUI()
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
                UpdateUI();
            else
                Dispatcher.Invoke(new UpdateUIDelegate(UpdateUI));
        }

        #endregion


        #region UI

        #region 'File' menu

        /// <summary>
        /// Clears image collection of image viewer and
        /// adds image(s) to an image collection of image viewer.
        /// </summary>
        private void openImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsFileOpening = true;

            CodecsFileFilters.SetFilters(openFileDialog1);

            // select image file
            if (openFileDialog1.ShowDialog().Value)
            {
                // clear image collection of the image viewer if necessary
                if (annotationViewer1.Images.Count > 0)
                {
                    annotationViewer1.Images.ClearAndDisposeItems();
                }

                // add image file to image collection of the image viewer
                try
                {
                    OpenFile(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }

            IsFileOpening = false;
        }

        /// <summary>
        /// Clears image collection of image viewer and
        /// adds image(s) to an image collection of image viewer.
        /// </summary>
        private void MainToolbar_OpenFile(object sender, EventArgs e)
        {
            openImageMenuItem_Click(sender, null);
        }

        /// <summary>
        /// Adds image(s) to an image collection of image viewer.
        /// </summary>
        private void addImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsFileOpening = true;

            CodecsFileFilters.SetFilters(openFileDialog1);

            openFileDialog1.Multiselect = true;

            // select image file(s)
            if (openFileDialog1.ShowDialog().Value)
            {
                // add image file(s) to image collection of the image viewer
                try
                {
                    foreach (string fileName in openFileDialog1.FileNames)
                        annotationViewer1.Images.Add(fileName);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }

            openFileDialog1.Multiselect = false;

            IsFileOpening = false;
        }

        /// <summary>
        /// Saves image collection with annotations of image viewer to new source and
        /// switches to the new source.
        /// </summary>
        private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImageCollectionToMultipageImageFile(true);
        }

        /// <summary>
        /// Saves image collection with annotations of image viewer to new source and
        /// do NOT switch to the new source.
        /// </summary>
        private void MainToolbar_SaveFile(object sender, EventArgs e)
        {
            saveAsMenuItem_Click(sender, null);
        }

        /// <summary>
        /// Saves image collection with annotations to the first source of image collection,
        /// i.e. saves modified image collection with annotations back to the source file.
        /// </summary>
        private void saveCurrentFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImageCollectionToSourceFile();
        }

        /// <summary>
        /// Saves image collection with annotations of image viewer to new source and
        /// do NOT switch to the new source.
        /// </summary>
        private void saveToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImageCollectionToMultipageImageFile(false);
        }

        /// <summary>
        /// Closes the current image file.
        /// </summary>
        private void closeImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentFile();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Prints images with annotations.
        /// </summary>
        private void MainToolbar_Print(object sender, EventArgs e)
        {
            printMenuItem_Click(sender, null);
        }

        /// <summary>
        /// Prints images with annotations.
        /// </summary>
        private void printMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = _printManager.PrintDialog;
            printDialog.MinPage = 1;
            printDialog.MaxPage = (uint)_printManager.Images.Count;
            printDialog.UserPageRangeEnabled = true;

            // show print dialog and
            // start print if dialog results is OK
            if (printDialog.ShowDialog().Value)
            {
                try
                {
                    _printManager.Print(this.Title);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion


        #region 'Edit' menu

        /// <summary>
        /// Enables/disables the image/annotations undo manager.
        /// </summary>
        private void enableUndoRedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool isUndoManagerEnabled = _undoManager.IsEnabled ^ true;


            if (!isUndoManagerEnabled)
            {
                CloseHistoryWindow();

                _undoManager.Clear();
            }

            _undoManager.IsEnabled = isUndoManagerEnabled;

            UpdateUndoRedoMenu(_undoManager);

            // update UI
            UpdateUI();
        }

        /// <summary>
        /// Undoes changes in image/annotations.
        /// </summary>
        private void undoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                return;

            _undoManager.Undo(1);
            UpdateUI();
        }

        /// <summary>
        /// Redoes changes in image/annotations.
        /// </summary>
        private void redoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                return;

            _undoManager.Redo(1);
            UpdateUI();
        }

        /// <summary>
        /// Edits the undo manager settings.
        /// </summary>
        private void undoRedoSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IDataStorage dataStorage = _dataStorage;

            if (dataStorage is CompositeDataStorage)
            {
                CompositeDataStorage compositeStorage = (CompositeDataStorage)dataStorage;
                dataStorage = compositeStorage.Storages[0];
            }

            WpfUndoManagerSettingsWindow dlg = new WpfUndoManagerSettingsWindow(_undoManager, dataStorage);
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                if (dlg.DataStorage != dataStorage)
                {
                    IDataStorage prevDataStorage = _dataStorage;

                    if (dlg.DataStorage is CompressedImageStorage)
                    {
                        _dataStorage = new CompositeDataStorage(
                            dlg.DataStorage,
                            new CloneableObjectStorageInMemory());
                    }
                    else
                    {
                        _dataStorage = dlg.DataStorage;
                    }

                    _undoManager.Clear();
                    _undoManager.DataStorage = _dataStorage;

                    _annotationViewerUndoMonitor.DataStorage = _dataStorage;

                    if (prevDataStorage != null)
                        prevDataStorage.Dispose();
                }
                UpdateUndoRedoMenu(_undoManager);
            }
        }

        /// <summary>
        /// Shows/hides the history dialog.
        /// </summary>
        private void historyDialogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            historyDialogMenuItem.IsChecked ^= true;

            if (historyDialogMenuItem.IsChecked)
                // show the image processing history form
                ShowHistoryWindow();
            else
                // close the image processing history form
                CloseHistoryWindow();
        }

        /// <summary>
        /// Enables/disables showing history for the displayed images.
        /// </summary>
        private void showHistoryForDisplayedImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            showHistoryForDisplayedImagesMenuItem.IsChecked ^= true;

            _annotationViewerUndoMonitor.ShowHistoryForDisplayedImages =
                showHistoryForDisplayedImagesMenuItem.IsChecked;
        }

        #endregion


        #region 'View' menu

        /// <summary>
        /// Changes settings of thumbanil viewer.
        /// </summary>
        private void thumbnailViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailViewerSettingsWindow viewerSettingsDialog = new ThumbnailViewerSettingsWindow(thumbnailViewer, (Style)Resources["ThumbnailItemStyle"]);
            viewerSettingsDialog.Owner = this;
            viewerSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewerSettingsDialog.ShowDialog();
        }

        /// <summary>
        /// Enables/disables usage of bounding box during creation/transformation of annotation.
        /// </summary>
        private void boundAnnotationsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.IsAnnotationBoundingRectEnabled = boundAnnotationsToolStripMenuItem.IsChecked;
        }

        /// <summary>
        /// Enables/disables the moving of annotations between images.
        /// </summary>
        private void moveAnnotationsBetweenImagesMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (moveAnnotationsBetweenImagesMenuItem.IsChecked)
                annotationViewer1.CanMoveAnnotationsBetweenImages = true;
            else
                annotationViewer1.CanMoveAnnotationsBetweenImages = false;
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void rotateClockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewClockwise();
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void rotateCounterclockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewCounterClockwise();
        }

        /// <summary>
        /// Changes settings of annotation viewer.
        /// </summary>
        private void annotationViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageViewerSettingsWindow viewerSettingsDialog = new ImageViewerSettingsWindow(annotationViewer1);
            viewerSettingsDialog.Owner = this;
            viewerSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewerSettingsDialog.ShowDialog();
            UpdateUI();
        }

        /// <summary>
        /// Changes settings of annotation interaction points.
        /// </summary>
        private void annotationInteractionPointsSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WpfInteractionAreaAppearanceManagerWindow window = new WpfInteractionAreaAppearanceManagerWindow();
            window.InteractionAreaSettings = _annotationInteractionAreaSettings;
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        /// <summary>
        /// Changes image display mode of image viewer.
        /// </summary>
        private void ImageDisplayMode_Click(object sender, RoutedEventArgs e)
        {
            MenuItem imageDisplayModeMenuItem = (MenuItem)sender;
            annotationViewer1.DisplayMode = (ImageViewerDisplayMode)imageDisplayModeMenuItem.Tag;
            UpdateUI();
        }

        /// <summary>
        /// Sets an image size mode.
        /// </summary>
        private void imageSizeMode_Click(object sender, RoutedEventArgs e)
        {
            // disable previously checked menu
            _imageScaleModeSelectedMenuItem.IsChecked = false;

            //
            MenuItem item = (MenuItem)sender;
            switch (item.Header.ToString())
            {
                case "Normal":
                    annotationViewer1.SizeMode = ImageSizeMode.Normal;
                    break;
                case "Best fit":
                    annotationViewer1.SizeMode = ImageSizeMode.BestFit;
                    break;
                case "Fit to width":
                    annotationViewer1.SizeMode = ImageSizeMode.FitToWidth;
                    break;
                case "Fit to height":
                    annotationViewer1.SizeMode = ImageSizeMode.FitToHeight;
                    break;
                case "Pixel to Pixel":
                    annotationViewer1.SizeMode = ImageSizeMode.PixelToPixel;
                    break;
                case "Scale":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    break;
                case "25%":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    annotationViewer1.Zoom = 25;
                    break;
                case "50%":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    annotationViewer1.Zoom = 50;
                    break;
                case "100%":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    annotationViewer1.Zoom = 100;
                    break;
                case "200%":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    annotationViewer1.Zoom = 200;
                    break;
                case "400%":
                    annotationViewer1.SizeMode = ImageSizeMode.Zoom;
                    annotationViewer1.Zoom = 400;
                    break;
            }

            _imageScaleModeSelectedMenuItem = item;
            _imageScaleModeSelectedMenuItem.IsChecked = true;
        }

        /// <summary>
        /// Enables/disables logging of annotation's changes.
        /// </summary>
        private void showEventsLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (showEventsLogMenuItem.IsChecked)
            {
                annotationEventsLogRow.Height = new GridLength(100);
                annotationEventsLogGridSplitter.Visibility = Visibility.Visible;
            }
            else
            {
                annotationEventsLogRow.Height = new GridLength(0);
                annotationEventsLogGridSplitter.Visibility = Visibility.Collapsed;
            }

            if (_annotationLogger == null)
                _annotationLogger = new WpfAnnotationsLogger(annotationViewer1, annotationEventsLog);

            _annotationLogger.IsEnabled = showEventsLogMenuItem.IsChecked;
        }

        /// <summary>
        /// Handles the TextChanged event of annotationEventsLog object.
        /// </summary>
        private void annotationEventsLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isFormClosing)
                annotationEventsLog.ScrollToLine(annotationEventsLog.LineCount - 1);
        }

        /// <summary>
        /// Edits the color management settings.
        /// </summary>
        private void colorManagementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ColorManagementSettingsWindow.EditColorManagement(annotationViewer1);
        }

        /// <summary>
        /// Enables/disables the spell cheking of text annotations.
        /// </summary>
        private void enableSpellCheckingMenuItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
                _annotationInteractionAreaSettings.IsSpellCheckingEnabled = enableSpellCheckingMenuItem.IsChecked;
        }

        #endregion


        #region 'Annotation' menu

        /// <summary>
        /// "Annotations" menu is opening.
        /// </summary>
        private void annotationsMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (annotationViewer1.FocusedAnnotationView != null && annotationViewer1.FocusedAnnotationView is WpfLineAnnotationViewBase)
            {
                transformationModeMenuItem.IsEnabled = true;
                UpdateAnnotationsTransformationModeMenu();
            }
            else
            {
                transformationModeMenuItem.IsEnabled = false;
            }

            UpdateEditMenuItems();
        }

        /// <summary>
        /// "Annotations" menu is closed.
        /// </summary>
        private void annotationsMenuItem_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)e.Source).Name != "annotationsMenuItem")
                return;

            EnableEditMenuItems();
        }

        /// <summary>
        /// Shows information about annotation collections of all images.
        /// </summary>
        private void annotationsInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WpfAnnotationsInfoWindow ai = new WpfAnnotationsInfoWindow(annotationViewer1.AnnotationDataController);
            ai.Owner = this;
            ai.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ai.ShowDialog();
        }


        #region Interaction Mode

        /// <summary>
        /// Changes the annotation interaction mode to None.
        /// </summary>
        private void annotationInteractionModeNoneMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.None;
        }

        /// <summary>
        /// Changes the annotation interaction mode to View.
        /// </summary>
        private void annotationInteractionModeViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.View;
        }

        /// <summary>
        /// Changes the annotation interaction mode to Author.
        /// </summary>
        private void annotationInteractionModeAuthorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.Author;
        }

        #endregion


        #region Transformation Mode

        /// <summary>
        /// Sets "rectangular" transformation mode for focused annotation.
        /// </summary>
        private void transformationModeRectangularMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((WpfLineAnnotationViewBase)annotationViewer1.FocusedAnnotationView).GripMode = GripMode.Rectangular;
            UpdateAnnotationsTransformationModeMenu();
        }

        /// <summary>
        /// Sets "points" transformation mode for focused annotation. 
        /// </summary>
        private void transformationModePointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((WpfLineAnnotationViewBase)annotationViewer1.FocusedAnnotationView).GripMode = GripMode.Points;
            UpdateAnnotationsTransformationModeMenu();
        }

        /// <summary>
        /// Sets "rectangular and points" transformation mode for focused annotation.
        /// </summary>
        private void transformationModeRectangularAndPointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((WpfLineAnnotationViewBase)annotationViewer1.FocusedAnnotationView).GripMode = GripMode.RectangularAndPoints;
            UpdateAnnotationsTransformationModeMenu();
        }

        #endregion


        #region Load and Save annotations

        /// <summary>
        /// Loads annotation collection from file.
        /// </summary>
        private void loadFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsFileOpening = true;

            AnnotationDemosTools.LoadAnnotationsFromFile(annotationViewer1, openFileDialog1, _undoManager);

            IsFileOpening = false;
        }

        /// <summary>
        /// Saves annotation collection to a file.
        /// </summary>
        private void saveToFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsFileSaving = true;

            AnnotationDemosTools.SaveAnnotationsToFile(annotationViewer1, saveFileDialog1);

            IsFileSaving = false;
        }

        #endregion


        /// <summary>
        /// Starts building of annotation.
        /// </summary>
        private void addAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationType annotationType = _menuItemToAnnotationType[(MenuItem)sender];

            // start new annotation building process and specify that this is the first process
            annotationToolBar.AddAndBuildAnnotation(annotationType);
        }

        /// <summary>
        /// Enables/disables the continuous building of annotations.
        /// </summary>
        private void buildAnnotationsContinuouslyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            buildAnnotationsContinuouslyMenuItem.IsChecked ^= true;
            annotationToolBar.NeedBuildAnnotationsContinuously = buildAnnotationsContinuouslyMenuItem.IsChecked;
        }


        #region UI actions

        /// <summary>
        /// Cuts selected annotation.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get UI action
            CutItemUIAction cutUIAction = GetUIAction<CutItemUIAction>(annotationViewer1.VisualTool);

            // if UI action is not empty AND UI action is enabled
            if (cutUIAction != null && cutUIAction.IsEnabled)
            {
                _undoManager.BeginCompositeAction("WpfAnnotationViewCollection: Cut");

                try
                {
                    cutUIAction.Execute();
                }
                finally
                {
                    _undoManager.EndCompositeAction();
                }
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Copies selected annotation.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get UI action
            CopyItemUIAction copyUIAction = GetUIAction<CopyItemUIAction>(annotationViewer1.VisualTool);
            // if UI action is not empty AND UI action is enabled
            if (copyUIAction != null && copyUIAction.IsEnabled)
            {
                // execute UI action
                copyUIAction.Execute();
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Pastes annotations from "internal" buffer and makes them active.
        /// </summary>
        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get UI action
            PasteItemWithOffsetUIAction pasteUIAction = GetUIAction<PasteItemWithOffsetUIAction>(annotationViewer1.VisualTool);
            // if UI action is not empty AND UI action is enabled
            if (pasteUIAction != null && pasteUIAction.IsEnabled)
            {
                pasteUIAction.OffsetX = 20;
                pasteUIAction.OffsetY = 20;

                _undoManager.BeginCompositeAction("WpfAnnotationViewCollection: Paste");

                try
                {
                    pasteUIAction.Execute();
                }
                finally
                {
                    _undoManager.EndCompositeAction();
                }
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Pastes annotations from "internal" buffer to mouse position and makes them active.
        /// </summary>
        private void pasteAnnotationInMousePositionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get mouse position on image in DIP
            Point mousePositionOnImageInDip = annotationViewer1.PointFromControlToDip(_contextMenuPosition);

            annotationViewer1.PasteAnnotationsFromClipboard(mousePositionOnImageInDip);
        }

        /// <summary>
        /// Removes selected annotation from annotation collection.
        /// </summary>
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if thumbnail viewer is focused
            if (thumbnailViewer.IsFocused)
            {
                thumbnailViewer.DoDelete();
            }
            else
            {
                // delete the selected annotation from image
                DeleteAnnotation(false);
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Removes all annotations from annotation collection.
        /// </summary>
        private void deleteAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // delete all annotations from image
            DeleteAnnotation(true);

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Brings the selected annotation to the first position in annotation collection.
        /// </summary>
        private void bringToBackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            annotationViewer1.BringSelectedAnnotationToBack();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Brings the selected annotation to the last position in annotation collection.
        /// </summary>
        private void bringToFrontMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            annotationViewer1.BringSelectedAnnotationToFront();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Enables/disables multi selection of annotations in viewer.
        /// </summary>
        private void multiSelectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.AnnotationMultiSelect = multiSelectMenuItem.IsChecked;
            UpdateUI();
        }

        /// <summary>
        /// Selects all annotations of annotation collection.
        /// </summary>
        private void selectAllAnnotations_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            // if thumbnail viewer is focused
            if (thumbnailViewer.IsFocused)
            {
                thumbnailViewer.DoSelectAll();
            }
            else
            {
                // get UI action
                SelectAllItemsUIAction selectAllUIAction = GetUIAction<SelectAllItemsUIAction>(annotationViewer1.VisualTool);
                // if UI action is not empty AND UI action is enabled
                if (selectAllUIAction != null && selectAllUIAction.IsEnabled)
                {
                    // execute action
                    selectAllUIAction.Execute();
                }
            }
            UpdateUI();
        }

        /// <summary>
        /// Deselects all annotations of annotation collection.
        /// </summary>
        private void deselectAllAnnotations_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            // if thumbnail viewer is not focused
            if (!thumbnailViewer.IsFocused)
            {
                // get UI action
                DeselectAllItemsUIAction deselectAllUIAction = GetUIAction<DeselectAllItemsUIAction>(annotationViewer1.VisualTool);
                // if UI action is not empty AND UI action is enabled
                if (deselectAllUIAction != null && deselectAllUIAction.IsEnabled)
                {
                    // execute UI action
                    deselectAllUIAction.Execute();
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// Groups/ungroups selected annotations of annotation collection.
        /// </summary>
        private void groupSelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationDemosTools.GroupUngroupSelectedAnnotations(annotationViewer1, _undoManager);
        }

        /// <summary>
        /// Groups all annotations of annotation collection.
        /// </summary>
        private void groupAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationDemosTools.GroupAllAnnotations(annotationViewer1, _undoManager);
        }

        #endregion


        #region Rotate, Burn, Clone

        /// <summary>
        /// Rotates image with annotations.
        /// </summary>
        private void rotateImageWithAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationDemosTools.RotateImageWithAnnotations(annotationViewer1, _undoManager, this);
        }

        /// <summary>
        /// Burns annotations on focused image.
        /// </summary>
        private void burnAnnotationsOnImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!AnnotationDemosTools.CheckImage(annotationViewer1))
                return;

            Cursor currentCursor = Cursor;

            try
            {
                Cursor = Cursors.Wait;
                AnnotationDemosTools.BurnAnnotationsOnImage(annotationViewer1, _undoManager, _dataStorage);
                UpdateUI();
            }
            catch (ImageProcessingException ex)
            {
                Cursor = currentCursor;
                MessageBox.Show(ex.Message, "Burn annotations on image", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exc)
            {
                Cursor = currentCursor;
                DemosTools.ShowErrorMessage(exc);
            }
            Cursor = currentCursor;
        }

        /// <summary>
        /// Clones image with annotations.
        /// </summary>
        private void cloneImageWithAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            annotationViewer1.CancelAnnotationBuilding();

            annotationViewer1.AnnotationDataController.CloneImageWithAnnotations(annotationViewer1.FocusedIndex, annotationViewer1.Images.Count);
        }

        #endregion

        #endregion


        #region 'Fill and Sign'

        /// <summary>
        /// Handles the Click event of fillSignatureMenuItem object.
        /// </summary>
        private void fillSignatureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // create dialog that allows to create and select the signature-annotation
            FillSignatureWindow dlg = new FillSignatureWindow((AnnotationDataCollection)_signatureAnnotations.Clone());
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;
            if (_selectedSignatureAnnotationIndex != -1)
                dlg.SelectedSignatureAnnotationIndex = _selectedSignatureAnnotationIndex;

            // show the dialog
            if (dlg.ShowDialog() == true)
            {
                // if signature-annotation is selected
                if (dlg.SelectedSignatureAnnotation != null)
                {
                    // build the signature-annotation on image in viewer
                    annotationViewer1.AddAndBuildAnnotation(dlg.SelectedSignatureAnnotation);
                }

                // save index of selected signature-annotation
                _selectedSignatureAnnotationIndex = dlg.SelectedSignatureAnnotationIndex;

                // remove old signature-annotations
                _signatureAnnotations.ClearAndDisposeItems();

                // save new signature-annotations
                _signatureAnnotations.AddRange(dlg.GetSignatureAnnotations());
            }
        }

        /// <summary>
        /// Handles the Click event of signDocumentMenuItem object.
        /// </summary>
        private void signDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            AnnotationDataController annotationController = annotationViewer1.AnnotationDataController;

            // if signature-annotation does not exist
            if (!IsSignatureAnnotationExist(annotationController))
            {
                MessageBox.Show(
                    "The signature-annotation is not found." + Environment.NewLine +
                    "Please select \"Fill and Sign => Fill Signature...\" menu and create a signature-annotation.",
                    "Sign PDF document", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // create dialog that allows to select the signature-annotation
            WpfAnnotationsInfoWindow annotationsInfoDialog =
                new WpfAnnotationsInfoWindow(annotationController, "Signature", true, annotationViewer1.FocusedAnnotationData, false, true, "Select the signature-annotation");

            annotationsInfoDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            annotationsInfoDialog.Owner = this;

            // show the dialog
            if (annotationsInfoDialog.ShowDialog() == true)
            {
                // set file filters in file saving dialog
                saveFileDialog1.Filter = "PDF Files|*.pdf";

                // show the save file dialog
                if (saveFileDialog1.ShowDialog() == true)
                {
                    // create PDF encoder
                    using (PdfEncoder encoder = new PdfEncoder(true))
                    {
                        // if source file is not PDF document
                        if (string.IsNullOrEmpty(_sourceFilename) || Path.GetExtension(_sourceFilename).ToUpperInvariant() != ".PDF")
                        {
                            encoder.Settings.AnnotationsFormat = AnnotationsFormat.VintasoftBinary;

                            // create dialog that allows to set the PDF encoder settings
                            PdfEncoderSettingsWindow pdfEncoderSettingsDlg = new PdfEncoderSettingsWindow();

                            pdfEncoderSettingsDlg.AppendExistingDocumentEnabled = File.Exists(saveFileDialog1.FileName);
                            pdfEncoderSettingsDlg.CanEditAnnotationSettings = true;
                            pdfEncoderSettingsDlg.AllowMrcCompression = false;
                            pdfEncoderSettingsDlg.EncoderSettings = encoder.Settings;
                            // show the dialog
                            if (pdfEncoderSettingsDlg.ShowDialog() != true)
                                return;
                        }

                        // create PDF page signature manager
                        using (Vintasoft.Imaging.Annotation.Pdf.PdfPageSignatureManager manager =
                            new Vintasoft.Imaging.Annotation.Pdf.PdfPageSignatureManager(annotationViewer1.AnnotationDataController, encoder))
                        {
                            manager.SignatureRequest += PdfPageSignatureManager_SignatureRequest;

                            try
                            {
                                // for each selected annotation
                                foreach (AnnotationData signatureAnnotation in annotationsInfoDialog.SelectedAnnotations.Keys)
                                {
                                    // get image that contains annotation
                                    VintasoftImage image = annotationsInfoDialog.SelectedAnnotations[signatureAnnotation];

                                    // registers signature on image
                                    manager.RegisterSignatureOnImage(image, signatureAnnotation);
                                }

                                // save PDF document
                                annotationViewer1.Images.SaveSync(saveFileDialog1.FileName, encoder);

                                MessageBox.Show("Document is signed successfully.");
                            }
                            catch (Exception exc)
                            {
                                DemosTools.ShowErrorMessage(exc);
                            }
                            finally
                            {
                                manager.SignatureRequest -= PdfPageSignatureManager_SignatureRequest;

                                _sourceFilename = saveFileDialog1.FileName;

                                UpdateUI();
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
#endif
        }

        /// <summary>
        /// Returns a value indicating whether image collection has the signature-annotation.
        /// </summary>
        /// <param name="annotationController">The annotation controller.</param>
        /// <returns>A value indicating whether image collection has the signature-annotation.</returns>
        private static bool IsSignatureAnnotationExist(AnnotationDataController annotationController)
        {
            // for each image in annotation data controller
            for (int i = 0; i < annotationController.Images.Count; i++)
            {
                // get annotation collection for image
                AnnotationDataCollection annotations = annotationController[i];
                // for each annotation in annotation collection
                for (int j = 0; j < annotations.Count; j++)
                {
                    AnnotationData annotation = annotations[j];
                    if (annotation.Intent == "Signature")
                        return true;
                }
            }

            return false;
        }

#if !REMOVE_PDF_PLUGIN
        /// <summary>
        /// Handles the SignatureRequest event of PdfPageSignatureManager object.
        /// </summary>
        private void PdfPageSignatureManager_SignatureRequest(object sender, SignatureRequestEventArgs e)
        {
            // create dialog that allows to perform signing of PDF document
            WpfDemosCommonCode.Pdf.Security.CreateSignatureFieldWindow dlg =
                new WpfDemosCommonCode.Pdf.Security.CreateSignatureFieldWindow(e.SignatureField);

            dlg.CanChangeSignatureAppearance = false;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            // show the dialog
            dlg.ShowDialog();
        }
#endif

        /// <summary>
        /// Handles the Click event of verifySignaturesMenuItem object.
        /// </summary>
        private void verifySignaturesMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            // create dialog that allows to view information about document signatures and verify signature
            WpfDemosCommonCode.Pdf.Security.DocumentSignaturesWindow dlg =
                new WpfDemosCommonCode.Pdf.Security.DocumentSignaturesWindow(_pdfDocument);

            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            // show the dialog
            dlg.ShowDialog();
#endif
        }

        #endregion


        #region 'Help' menu

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder description = new StringBuilder();

            description.AppendLine("This project demonstrates the following SDK possibilities:");
            description.AppendLine();
            description.AppendLine("- Interactively add 20+ predefined annotation types onto JPEG, PNG, TIFF image or PDF page.");
            description.AppendLine();
            description.AppendLine("- Use source codes of custom annotations as an example of custom annotation development.");
            description.AppendLine();
            description.AppendLine("- Manipulate annotations: copy/paste, cut, delete, burn on image, resize, rotate.");
            description.AppendLine();
            description.AppendLine("- Load/save annotations from/to XML, JPEG, PNG, TIFF or PDF file in VintasoftBinary, XMP or WANG format.");
            description.AppendLine();
            description.AppendLine("- Display, save and print images with annotations.");
            description.AppendLine();
            description.AppendLine("- Navigate images: first, previous, next, last.");
            description.AppendLine();
            description.AppendLine("- Change settings of image preview.");
            description.AppendLine();
            description.AppendLine("- Use visual tools for interactive image processing: selection, magnifier, crop, drag-n-drop, zoom, pan, scroll.");

            description.AppendLine();
            description.AppendLine();
            description.AppendLine("The project is available in C# and VB.NET for Visual Studio .NET.");

            WpfAboutBoxBaseWindow dlg = new WpfAboutBoxBaseWindow("vsannotation-dotnet");
            dlg.Description = description.ToString();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.ShowDialog();
        }

        #endregion


        #region Menu, context menu

        /// <summary>
        /// Saves focused image with annotations to a file.
        /// </summary>
        private void saveImageWithAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveSingleImageToNewImageFile();
        }

        /// <summary>
        /// Copies focused image with annotations to clipboard.
        /// </summary>
        private void copyImageToClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationDemosTools.CopyImageToClipboard(annotationViewer1);
        }

        /// <summary>
        /// Deletes focused image.
        /// </summary>
        private void deleteImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteImages();

            // update the UI
            UpdateUI();
        }

        #endregion

        #endregion


        #region Viewers

        #region Annotation viewer

        /// <summary>
        /// The mouse is moved in annotation viewer.
        /// </summary>
        private void annotationViewer1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // if viewer must be scrolled when annotation is moved
            if (scrollViewerWhenAnnotationIsMovedMenuItem.IsChecked)
            {
                // if left mouse button is pressed
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // get the interaction controller of annotation viewer
                    IWpfInteractionController interactionController =
                        annotationViewer1.AnnotationVisualTool.ActiveInteractionController;
                    // if user interacts with annotation
                    if (interactionController != null && interactionController.IsInteracting)
                    {
                        const int delta = 20;

                        // get the "visible area" of annotation viewer
                        Rect rect = new Rect(0, 0,
                                             annotationViewer1.ViewportWidth,
                                             annotationViewer1.ViewportHeight);
                        // remove "border" from the "visible area"
                        rect.Inflate(-delta, -delta);

                        // get the mouse location
                        Point mousePosition = e.GetPosition(annotationViewer1);
                        // if mouse is located in "border"
                        if (!rect.Contains(mousePosition))
                        {
                            // calculate how to scrool the annotation viewer

                            double deltaX = 0;
                            if (mousePosition.X < delta)
                                deltaX = -(delta - mousePosition.X);
                            if (mousePosition.X > delta + rect.Width)
                                deltaX = -(delta + rect.Width - mousePosition.X);
                            double deltaY = 0;
                            if (mousePosition.Y < delta)
                                deltaY = -(delta - mousePosition.Y);
                            if (mousePosition.Y > delta + rect.Height)
                                deltaY = -(delta + rect.Height - mousePosition.Y);

                            // get the auto scroll position of annotation viewer
                            Point autoScrollPosition = new Point(
                                Math.Abs(annotationViewer1.AutoScrollPositionEx.X),
                                Math.Abs(annotationViewer1.AutoScrollPositionEx.Y));

                            // calculate new auto scroll position

                            if ((!annotationViewer1.AutoScroll || annotationViewer1.ViewerState.AutoScrollSize.Width > 0) && deltaX != 0)
                                autoScrollPosition.X += deltaX;
                            if ((!annotationViewer1.AutoScroll || annotationViewer1.ViewerState.AutoScrollSize.Height > 0) && deltaY != 0)
                                autoScrollPosition.Y += deltaY;

                            // if auto scroll position is changed
                            if (autoScrollPosition != annotationViewer1.ViewerState.AutoScrollPosition)
                                // set new auto scroll position
                                annotationViewer1.ViewerState.AutoScrollPosition = autoScrollPosition;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The scroll position of the annotation viewer is changing.
        /// </summary>
        private void annotationViewer1_AutoScrollPositionExChanging(object sender, PropertyChangingEventArgs<Point> e)
        {
            // if viewer must be scrolled when annotation is moved
            if (scrollViewerWhenAnnotationIsMovedMenuItem.IsChecked)
            {
                // get the interaction controller of annotation viewer
                IWpfInteractionController interactionController =
                    annotationViewer1.AnnotationVisualTool.ActiveInteractionController;
                // if user interacts with annotation
                if (interactionController != null && interactionController.IsInteracting)
                {
                    // get bounding box of displayed images
                    Rect displayedImagesBBox = annotationViewer1.GetDisplayedImagesBoundingBox();

                    // get the scroll position
                    Point scrollPosition = e.NewValue;

                    // cut the coordinates for getting coordinates inside the focused image
                    scrollPosition.X = Math.Max(displayedImagesBBox.X, Math.Min(scrollPosition.X, displayedImagesBBox.Right));
                    scrollPosition.Y = Math.Max(displayedImagesBBox.Y, Math.Min(scrollPosition.Y, displayedImagesBBox.Bottom));

                    // update the scroll position
                    e.NewValue = scrollPosition;
                }
            }
        }

        /// <summary>
        /// Zoom is changed in annotation viewer.
        /// </summary>
        private void annotationViewer1_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            if (Math.Round(zoomSlider.Value) != Math.Round(annotationViewer1.Zoom))
                zoomSlider.Value = annotationViewer1.Zoom;
        }

        /// <summary>
        /// Occurs when visual tool throws an exception.
        /// </summary>
        private void annotationViewer1_VisualToolException(
            object sender,
            Vintasoft.Imaging.ExceptionEventArgs e)
        {
            DemosTools.ShowErrorMessage(e.Exception);
        }

        /// <summary>
        /// Image loading in viewer is started.
        /// </summary>
        private void annotationViewer1_ImageLoading(object sender, ImageLoadingEventArgs e)
        {
            imageLoadingProgerssBar.Visibility = Visibility.Visible;
            statusLabelLoadingImage.Visibility = Visibility.Visible;
            _imageLoadingStartTime = DateTime.Now;
        }

        /// <summary>
        /// Image loading in viewer is in progress.
        /// </summary>
        private void annotationViewer1_ImageLoadingProgress(object sender, ProgressEventArgs e)
        {
            if (_isFormClosing)
            {
                e.Cancel = true;
                return;
            }
            imageLoadingProgerssBar.Value = e.Progress;
        }

        /// <summary>
        /// Image loading in viewer is finished.
        /// </summary>
        private void annotationViewer1_ImageLoaded(object sender, ImageLoadedEventArgs e)
        {
            _imageLoadingTime = DateTime.Now.Subtract(_imageLoadingStartTime);

            imageLoadingProgerssBar.Visibility = Visibility.Collapsed;
            statusLabelLoadingImage.Visibility = Visibility.Collapsed;


            //
            VintasoftImage image = annotationViewer1.Image;

            // show error message if not critical error occurs during image loading
            string imageLoadingErrorString = "";
            if (image.LoadingError)
                imageLoadingErrorString = string.Format("[{0}] ", image.LoadingErrorString);
            // show information about the image
            imageInfoStatusLabel.Text = string.Format("{0} Width={1}; Height={2}; PixelFormat={3}; Resolution={4}",
            imageLoadingErrorString, image.Width, image.Height, image.PixelFormat, image.Resolution);

            // if image loading time more than 0
            if (_imageLoadingTime != TimeSpan.Zero)
                // show information about image loading time
                imageInfoStatusLabel.Text = string.Format("[Loading time: {0}ms] {1}",
            _imageLoadingTime.TotalMilliseconds, imageInfoStatusLabel.Text);

            // if image has annotations
            if (image.Metadata.AnnotationsFormat != AnnotationsFormat.None)
                // show information about format of annotations
                imageInfoStatusLabel.Text = string.Format("[AnnotationsFormat: {0}] {1}",
            image.Metadata.AnnotationsFormat, imageInfoStatusLabel.Text);


            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Key is pressed in annotation viewer.
        /// </summary>
        private void annotationViewer1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (annotationViewer1.IsAnnotationBuilding)
                    annotationViewer1.FinishAnnotationBuilding();
            }
            else if (e.Key == Key.Escape)
            {
                if (annotationViewer1.IsAnnotationBuilding)
                    annotationViewer1.CancelAnnotationBuilding();
                else
                    annotationViewer1.AnnotationVisualTool.CancelActiveInteraction();
            }
            else if (
                CanInteractWithFocusedAnnotationUseKeyboard() &&
                annotationViewer1.IsFocused &&
                annotationViewer1.FocusedAnnotationView != null)
            {
                // get transform from AnnotationViewer space to DIP space
                AffineMatrix matrix = annotationViewer1.GetTransformFromVisualToolToDip();
                System.Drawing.PointF deltaVector = PointFAffineTransform.TransformVector(
                    matrix,
                    new System.Drawing.PointF(ANNOTATION_KEYBOARD_MOVE_DELTA, ANNOTATION_KEYBOARD_MOVE_DELTA));

                System.Drawing.PointF resizeVector = PointFAffineTransform.TransformVector(
                    matrix,
                    new System.Drawing.PointF(ANNOTATION_KEYBOARD_RESIZE_DELTA, ANNOTATION_KEYBOARD_RESIZE_DELTA));

                // current annotation location 
                Point location = annotationViewer1.FocusedAnnotationView.Location;
                Size size = annotationViewer1.FocusedAnnotationView.Size;

                switch (e.Key)
                {
                    case Key.Up:
                        annotationViewer1.FocusedAnnotationView.Location = new Point(location.X, location.Y - deltaVector.Y);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        annotationViewer1.FocusedAnnotationView.Location = new Point(location.X, location.Y + deltaVector.Y);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        annotationViewer1.FocusedAnnotationView.Location = new Point(location.X + deltaVector.X, location.Y);
                        e.Handled = true;
                        break;
                    case Key.Left:
                        annotationViewer1.FocusedAnnotationView.Location = new Point(location.X - deltaVector.X, location.Y);
                        e.Handled = true;
                        break;
                    case Key.Add:
                        annotationViewer1.FocusedAnnotationView.Size = new Size(size.Width + resizeVector.X, size.Height + resizeVector.Y);
                        e.Handled = true;
                        break;
                    case Key.Subtract:
                        if (size.Width > resizeVector.X)
                            annotationViewer1.FocusedAnnotationView.Size = new Size(size.Width - resizeVector.X, size.Height);

                        size = annotationViewer1.FocusedAnnotationView.Size;

                        if (size.Height > resizeVector.Y)
                            annotationViewer1.FocusedAnnotationView.Size = new Size(size.Width, size.Height - resizeVector.Y);
                        e.Handled = true;
                        break;
                }
                annotationDataPropertyGrid.Refresh();
            }
        }

        /// <summary>
        /// Determines whether can move focused annotation use keyboard.
        /// </summary>
        private bool CanInteractWithFocusedAnnotationUseKeyboard()
        {
            if (annotationViewer1.FocusedAnnotationView == null)
                return false;

#if !REMOVE_OFFICE_PLUGIN
            Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor documentEditor =
                WpfUserInteractionVisualTool.GetActiveInteractionController<Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor>(annotationViewer1.VisualTool);
            if (documentEditor != null && documentEditor.IsEditingEnabled)
            {
                return false;
            }
#endif
            return true;
        }

        /// <summary>
        /// Annotation interaction mode of viewer is changed.
        /// </summary>
        private void annotationViewer1_AnnotationInteractionModeChanged(
            object sender,
            AnnotationInteractionModeChangedEventArgs e)
        {
            annotationInteractionModeNoneMenuItem.IsChecked = false;
            annotationInteractionModeViewMenuItem.IsChecked = false;
            annotationInteractionModeAuthorMenuItem.IsChecked = false;

            AnnotationInteractionMode annotationInteractionMode = e.NewValue;
            switch (annotationInteractionMode)
            {
                case AnnotationInteractionMode.None:
                    annotationInteractionModeNoneMenuItem.IsChecked = true;
                    annotationInteractionModeToolStripComboBox.SelectedIndex = 0;
                    break;

                case AnnotationInteractionMode.View:
                    annotationInteractionModeViewMenuItem.IsChecked = true;
                    annotationInteractionModeToolStripComboBox.SelectedIndex = 1;
                    break;

                case AnnotationInteractionMode.Author:
                    annotationInteractionModeAuthorMenuItem.IsChecked = true;
                    annotationInteractionModeToolStripComboBox.SelectedIndex = 2;
                    break;
            }

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// The annotation deserialization error occurs.
        /// </summary>
        private void AnnotationDataController_AnnotationDataDeserializationException(
            object sender,
            Vintasoft.Imaging.Annotation.AnnotationDataDeserializationExceptionEventArgs e)
        {
            DemosTools.ShowErrorMessage("AnnotationData deserialization exception", e.Exception);
        }


        /// <summary>
        /// Handles the Images_ImageCollectionChanged event of annotationViewer1 object.
        /// </summary>
        private void annotationViewer1_Images_ImageCollectionChanged(object sender, ImageCollectionChangeEventArgs e)
        {
            // update the UI
            InvokeUpdateUI();
        }

        #endregion


        #region Thumbnail viewer

        /// <summary>
        /// Loading of thumbnails is in progress.
        /// </summary>
        private void thumbnailViewer_ThumbnailLoadingProgress(object sender, ProgressEventArgs e)
        {
            actionLabel.Content = "Creating thumbnails:";
            thumbnailLoadingProgerssBar.Value = e.Progress;
            thumbnailLoadingProgerssBar.Visibility = Visibility.Visible;
            actionLabel.Visibility = Visibility.Visible;
            if (thumbnailLoadingProgerssBar.Value == 100)
            {
                thumbnailLoadingProgerssBar.Visibility = Visibility.Collapsed;
                actionLabel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Thumbnail is added in thumbnail viewer.
        /// </summary>
        private void thumbnailViewer_ThumbnailAdded(object sender, ThumbnailImageItemEventArgs e)
        {
            e.Thumbnail.MouseEnter += Thumbnail_MouseEnter;
        }

        /// <summary>
        /// Handles the MouseEnter event of the ThumbnailImageItem control.
        /// </summary>
        private void Thumbnail_MouseEnter(object sender, MouseEventArgs e)
        {
            ThumbnailImageItem thumbnail = (ThumbnailImageItem)sender;
            if (thumbnail.ContextMenu == null)
            {
                ContextMenu thumbnailContextMenu = new ContextMenu();

                MenuItem saveImageWithAnnotationsItem = new MenuItem();
                saveImageWithAnnotationsItem.Header = "Save image with annotations...";
                saveImageWithAnnotationsItem.Click += new RoutedEventHandler(saveImageWithAnnotationsMenuItem_Click);
                thumbnailContextMenu.Items.Add(saveImageWithAnnotationsItem);

                MenuItem burnAnnotationsItem = new MenuItem();
                burnAnnotationsItem.Header = "Burn annotations on image";
                burnAnnotationsItem.Click += new RoutedEventHandler(burnAnnotationsOnImageMenuItem_Click);
                thumbnailContextMenu.Items.Add(burnAnnotationsItem);

                MenuItem copyImageItem = new MenuItem();
                copyImageItem.Header = "Copy image to clipboard";
                copyImageItem.Click += new RoutedEventHandler(copyImageToClipboardMenuItem_Click);
                thumbnailContextMenu.Items.Add(copyImageItem);

                MenuItem deleteImageItem = new MenuItem();
                deleteImageItem.Header = "Delete image(s)";
                deleteImageItem.Click += new RoutedEventHandler(deleteImageMenuItem_Click);
                thumbnailContextMenu.Items.Add(deleteImageItem);

                thumbnail.ContextMenu = thumbnailContextMenu;
            }
        }

        #endregion


        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void RotateViewClockwise()
        {
            annotationViewer1.RotateViewClockwise();
            thumbnailViewer.RotateViewClockwise();
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void RotateViewCounterClockwise()
        {
            annotationViewer1.RotateViewCounterClockwise();
            thumbnailViewer.RotateViewCounterClockwise();
        }

        #endregion


        #region Menu, context menu

        /// <summary>
        /// Initializes the "Annotation" -> "Menu" menu items.
        /// </summary>
        private void InitializeAddAnnotationMenuItems()
        {
            _menuItemToAnnotationType.Clear();

            _menuItemToAnnotationType.Add(rectangleMenuItem, AnnotationType.Rectangle);
            _menuItemToAnnotationType.Add(ellipseMenuItem, AnnotationType.Ellipse);
            _menuItemToAnnotationType.Add(highlightMenuItem, AnnotationType.Highlight);
            _menuItemToAnnotationType.Add(textHighlightMenuItem, AnnotationType.TextHighlight);
            _menuItemToAnnotationType.Add(embeddedImageMenuItem, AnnotationType.EmbeddedImage);
            _menuItemToAnnotationType.Add(referencedImageMenuItem, AnnotationType.ReferencedImage);
            _menuItemToAnnotationType.Add(textMenuItem, AnnotationType.Text);
            _menuItemToAnnotationType.Add(stickyNoteMenuItem, AnnotationType.StickyNote);
            _menuItemToAnnotationType.Add(freeTextMenuItem, AnnotationType.FreeText);
            _menuItemToAnnotationType.Add(rubberStampMenuItem, AnnotationType.RubberStamp);
            _menuItemToAnnotationType.Add(linkMenuItem, AnnotationType.Link);
            _menuItemToAnnotationType.Add(lineMenuItem, AnnotationType.Line);
            _menuItemToAnnotationType.Add(linesMenuItem, AnnotationType.Lines);
            _menuItemToAnnotationType.Add(linesWithInterpolationMenuItem, AnnotationType.LinesWithInterpolation);
            _menuItemToAnnotationType.Add(freehandLinesMenuItem, AnnotationType.FreehandLines);
            _menuItemToAnnotationType.Add(polygonMenuItem, AnnotationType.Polygon);
            _menuItemToAnnotationType.Add(polygonWithInterpolationMenuItem, AnnotationType.PolygonWithInterpolation);
            _menuItemToAnnotationType.Add(freehandPolygonMenuItem, AnnotationType.FreehandPolygon);
            _menuItemToAnnotationType.Add(rulerMenuItem, AnnotationType.Ruler);
            _menuItemToAnnotationType.Add(rulersMenuItem, AnnotationType.Rulers);
            _menuItemToAnnotationType.Add(angleMenuItem, AnnotationType.Angle);
            _menuItemToAnnotationType.Add(triangleCustomAnnotationMenuItem, AnnotationType.Triangle);
            _menuItemToAnnotationType.Add(markCustomAnnotationMenuItem, AnnotationType.Mark);
        }

        /// <summary>
        /// Updates "Annotations -> Transformation Mode" menu. 
        /// </summary>
        private void UpdateAnnotationsTransformationModeMenu()
        {
            // transformation mode for focused annotation
            GripMode gripMode = ((WpfLineAnnotationViewBase)annotationViewer1.FocusedAnnotationView).GripMode;
            // update menus
            transformationModeRectangularMenuItem.IsChecked = gripMode == GripMode.Rectangular;
            transformationModePointsMenuItem.IsChecked = gripMode == GripMode.Points;
            transformationModeRectangularAndPointsMenuItem.IsChecked = gripMode == GripMode.RectangularAndPoints;
        }

        /// <summary>
        /// Enables the UI action items in "Edit" menu.
        /// </summary>
        private void EnableEditMenuItems()
        {
            cutMenuItem.IsEnabled = true;
            copyMenuItem.IsEnabled = true;
            pasteMenuItem.IsEnabled = true;
            deleteMenuItem.IsEnabled = true;
            deleteAllMenuItem.IsEnabled = true;
            selectAllMenuItem.IsEnabled = true;
            deselectAllMenuItem.IsEnabled = true;
        }

        /// <summary>
        /// The annotation viewer context menu is opened.
        /// </summary>
        private void annotationViewerMenu_Opened(object sender, RoutedEventArgs e)
        {
            _contextMenuPosition = Mouse.GetPosition(annotationViewer1);

            UpdateContextMenuItem(pasteAnnotationMenuItem1, GetUIAction<PasteItemUIAction>(annotationViewer1.VisualTool));
        }

        /// <summary>
        /// The annotation context menu is opened.
        /// </summary>
        private void annotationMenu_Opened(object sender, RoutedEventArgs e)
        {
            _contextMenuPosition = Mouse.GetPosition(annotationViewer1);

            UpdateContextMenuItem(cutAnnotationMenuItem, GetUIAction<CutItemUIAction>(annotationViewer1.VisualTool));
            UpdateContextMenuItem(copyAnnotationMenuItem, GetUIAction<CopyItemUIAction>(annotationViewer1.VisualTool));
            UpdateContextMenuItem(pasteAnnotationMenuItem, GetUIAction<PasteItemUIAction>(annotationViewer1.VisualTool));
            UpdateContextMenuItem(deleteAnnotationMenuItem, GetUIAction<DeleteItemUIAction>(annotationViewer1.VisualTool));
        }

        /// <summary>
        /// Updates the UI action items in "Edit" menu.
        /// </summary>
        private void UpdateEditMenuItems()
        {
            // if thumbnail viewer is focused
            if (thumbnailViewer.IsFocused)
            {
                UpdateEditMenuItem(cutMenuItem, null, "Cut");
                UpdateEditMenuItem(copyMenuItem, null, "Copy");
                UpdateEditMenuItem(pasteMenuItem, null, "Paste");

                deleteMenuItem.IsEnabled = true;
                deleteMenuItem.Header = "Delete Page(s)";

                deleteAllMenuItem.IsEnabled = false;
                deleteAllMenuItem.Header = "Delete All";

                bool isFileEmpty = true;
                if (annotationViewer1.Images != null)
                    isFileEmpty = annotationViewer1.Images.Count <= 0;
                selectAllMenuItem.IsEnabled = !isFileEmpty && !IsFileOpening;
                selectAllMenuItem.Header = "Select All Pages";

                UpdateEditMenuItem(deselectAllMenuItem, null, "Deselect All");
            }
            else
            {
                UpdateEditMenuItem(cutMenuItem, GetUIAction<CutItemUIAction>(annotationViewer1.VisualTool), "Cut");
                UpdateEditMenuItem(copyMenuItem, GetUIAction<CopyItemUIAction>(annotationViewer1.VisualTool), "Copy");
                UpdateEditMenuItem(pasteMenuItem, GetUIAction<PasteItemUIAction>(annotationViewer1.VisualTool), "Paste");
                UpdateEditMenuItem(deleteMenuItem, GetUIAction<DeleteItemUIAction>(annotationViewer1.VisualTool), "Delete");
                UpdateEditMenuItem(deleteAllMenuItem, GetUIAction<DeleteAllItemsUIAction>(annotationViewer1.VisualTool), "Delete All");
                UpdateEditMenuItem(selectAllMenuItem, GetUIAction<SelectAllItemsUIAction>(annotationViewer1.VisualTool), "Select All");
                UpdateEditMenuItem(deselectAllMenuItem, GetUIAction<DeselectAllItemsUIAction>(annotationViewer1.VisualTool), "Deselect All");
            }
        }

        /// <summary>
        /// Updates the UI action item in "Edit" menu.
        /// </summary>
        /// <param name="menuItem">The "Edit" menu item.</param>
        /// <param name="uiAction">The UI action, which is associated with the "Edit" menu item.</param>
        /// <param name="defaultText">The default text for the "Edit" menu item.</param>
        private void UpdateEditMenuItem(MenuItem menuItem, UIAction uiAction, string defaultText)
        {
            // if UI action is not empty AND UI action is enabled
            if (uiAction != null && uiAction.IsEnabled)
            {
                // enable menu item
                menuItem.IsEnabled = true;
                // set text to menu item
                menuItem.Header = uiAction.Name;
            }
            else
            {
                // disable menu item
                menuItem.IsEnabled = false;
                // set default text to menu item
                menuItem.Header = defaultText;
            }
        }

        /// <summary>
        /// Updates the UI action item in context menu.
        /// </summary>
        /// <param name="menuItem">The menu item.</param>
        /// <param name="uiAction">The UI action, which is associated with the "Edit" menu item.</param>
        private void UpdateContextMenuItem(MenuItem menuItem, UIAction uiAction)
        {
            // if UI action is specified AND UI action is enabled
            if (uiAction != null && uiAction.IsEnabled)
            {
                // enable the menu item
                menuItem.IsEnabled = true;
            }
            else
            {
                // disable the menu item
                menuItem.IsEnabled = false;
            }
        }

        /// <summary>
        /// Returns the UI action of the visual tool.
        /// </summary>
        /// <param name="visualTool">Visual tool.</param>
        /// <returns>The UI action of the visual tool.</returns>
        private T GetUIAction<T>(WpfVisualTool visualTool)
            where T : UIAction
        {
            IList<UIAction> uiActions = null;
            // if visual tool has actions
            if (TryGetCurrentToolActions(visualTool, out uiActions))
            {
                // for each action in list
                foreach (UIAction uiAction in uiActions)
                {
                    if (uiAction is T)
                        return (T)uiAction;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns the UI actions of visual tool.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        /// <param name="uiActions">The list of UI actions supported by the current visual tool.</param>
        /// <returns>
        /// <b>true</b> - UI actions are found; otherwise, <b>false</b>.
        /// </returns>
        private bool TryGetCurrentToolActions(
            WpfVisualTool visualTool,
            out IList<UIAction> uiActions)
        {
            uiActions = null;
            ISupportUIActions currentToolWithUIActions = visualTool as ISupportUIActions;
            if (currentToolWithUIActions != null)
                uiActions = currentToolWithUIActions.GetSupportedUIActions();

            return uiActions != null;
        }

        #endregion


        #region Annotations's combobox AND annotation's property grid

        /// <summary>
        /// Fills combobox with information about annotations of image.
        /// </summary>
        private void FillAnnotationComboBox()
        {
            annotationComboBox.Items.Clear();

            if (annotationViewer1.FocusedIndex >= 0)
            {
                AnnotationDataCollection annotations = annotationViewer1.AnnotationDataController[annotationViewer1.FocusedIndex];
                for (int i = 0; i < annotations.Count; i++)
                {
                    annotationComboBox.Items.Add(string.Format("[{0}] {1}", i, annotations[i].GetType().Name));
                    if (annotationViewer1.FocusedAnnotationData == annotations[i])
                        annotationComboBox.SelectedIndex = i;
                }
            }
        }

        /// <summary>
        /// Shows information about annotation in property grid.
        /// </summary>
        private void ShowAnnotationProperties(WpfAnnotationView annotation)
        {
            AnnotationData data = null;
            if (annotation != null)
                data = annotation.Data;
            if (annotationDataPropertyGrid.SelectedObject != data)
                annotationDataPropertyGrid.SelectedObject = data;
            else if (!_isAnnotationTransforming)
                annotationDataPropertyGrid.Refresh();
        }

        /// <summary>
        /// Handler of the DropDown event of the ComboBox of annotations.
        /// </summary>
        private void annotationComboBox_DropDownOpened(object sender, EventArgs e)
        {
            FillAnnotationComboBox();
        }

        /// <summary>
        /// Selected annotation is changed using annotation's combobox.
        /// </summary>
        private void annotationComboBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (annotationViewer1.FocusedIndex != -1 && annotationComboBox.SelectedIndex != -1)
            {
                annotationViewer1.FocusedAnnotationData = annotationViewer1.AnnotationDataCollection[annotationComboBox.SelectedIndex];
            }
        }

        /// <summary>
        /// Selected annotation is changed in annotation viewer.
        /// </summary>
        private void annotationViewer1_SelectedAnnotationViewChanged(object sender, WpfAnnotationViewChangedEventArgs e)
        {
            FillAnnotationComboBox();
            ShowAnnotationProperties(annotationViewer1.FocusedAnnotationView);

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Collection with selected annotations is changed.
        /// </summary>
        private void SelectedAnnotations_Changed(object sender, EventArgs e)
        {
            // update the UI
            UpdateUI();
        }

        #endregion


        #region File manipulation

        /// <summary>
        /// Opens stream of the image file and
        /// adds stream of image file to the image collection of image viewer - this allows
        /// to save modified multipage image files back to the source.
        /// </summary>
        private void OpenFile(string filename)
        {
            CloseSource();
            OpenSourceStream(filename);

            // add images of new file to image collection of image viewer asynchronously
            Thread openFileThread = new Thread(OpenFileAsynchronously);
            openFileThread.SetApartmentState(ApartmentState.STA);
            openFileThread.IsBackground = true;
            openFileThread.Start();
        }

        /// <summary>
        /// Adds images of new file to image collection of image viewer asynchronously.
        /// </summary>
        private void OpenFileAsynchronously()
        {
            try
            {
                Dispatcher.Invoke(new SetCursorDelegate(SetCursor), Cursors.Wait);
                annotationViewer1.Images.Add(_sourceFilename, false);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                Dispatcher.Invoke(new CloseCurrentFileDelegate(CloseCurrentFile));
            }

#if !REMOVE_PDF_PLUGIN
            if (annotationViewer1.Images.Count > 0)
            {
                Vintasoft.Imaging.Pdf.Tree.PdfPage page = Vintasoft.Imaging.Pdf.PdfDocumentController.GetPageAssociatedWithImage(annotationViewer1.Images[0]);
                if (page != null)
                    _pdfDocument = page.Document;
            }
#endif

            Dispatcher.Invoke(new SetCursorDelegate(SetCursor), Cursors.Arrow);

            // update the UI
            InvokeUpdateUI();
        }

        /// <summary>
        /// Closes current image file.
        /// </summary>
        private void CloseCurrentFile()
        {
            this.Title = string.Format(_titlePrefix, ImagingGlobalSettings.ProductVersion, "(Untitled)");
            annotationViewer1.Images.ClearAndDisposeItems();
            CloseSource();
        }

        /// <summary>
        /// Opens stream of the image file.
        /// </summary>
        private void OpenSourceStream(string filename)
        {
            _sourceFilename = Path.GetFullPath(filename);
            _isFileReadOnlyMode = false;
            Stream stream = null;
            try
            {
                stream = new FileStream(_sourceFilename, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            if (stream == null)
            {
                _isFileReadOnlyMode = true;
            }
            else
            {
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// Closes stream of the image file.
        /// </summary>
        private void CloseSource()
        {
            _sourceFilename = null;
#if !REMOVE_PDF_PLUGIN
            _pdfDocument = null;
#endif
        }

        /// <summary>
        /// Sets the specified cursor to the main application window.
        /// </summary>
        /// <param name="cursor">Cursor object.</param>
        private void SetCursor(Cursor cursor)
        {
            Cursor = cursor;
        }

        #endregion


        #region Image manipulation

        /// <summary>
        /// Deletes selected images or focused image.
        /// </summary>
        private void DeleteImages()
        {
            // get an array of selected images
            VintasoftImage[] selectedImages = new VintasoftImage[thumbnailViewer.SelectedThumbnails.Count];

            // if selection is present
            if (selectedImages.Length > 0)
            {
                for (int i = 0; i < selectedImages.Length; i++)
                    selectedImages[i] = thumbnailViewer.SelectedThumbnails[i].Source;
            }
            // if selection is not present
            else
            {
                int focusedIndex = thumbnailViewer.FocusedIndex;
                // if there is no focused image
                if (focusedIndex == -1)
                    return;
                // if there is focused image
                selectedImages = new VintasoftImage[1];
                selectedImages[0] = thumbnailViewer.Thumbnails[focusedIndex].Source;
            }

            // remove selected images from the image collection
            for (int i = 0; i < selectedImages.Length; i++)
                thumbnailViewer.Images.Remove(selectedImages[i]);

            // dispose selected images
            for (int i = 0; i < selectedImages.Length; i++)
                selectedImages[i].Dispose();

            imageInfoStatusLabel.Text = "";
        }

        #endregion


        #region Annotation interaction mode

        /// <summary>
        /// Annotation interaction mode is changed using combobox.
        /// </summary>
        private void annotationInteractionModeToolStripComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (annotationInteractionModeToolStripComboBox.SelectedIndex)
            {
                case 0:
                    annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.None;
                    break;

                case 1:
                    annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.View;
                    break;

                case 2:
                    annotationViewer1.AnnotationInteractionMode = AnnotationInteractionMode.Author;
                    break;
            }

            EnableUndoRedoMenu();
            if (_historyWindow != null)
                _historyWindow.CanNavigateOnHistory = true;
        }

        #endregion


        #region Annotation

        /// <summary>
        /// Begins initialization of specified annotation.
        /// </summary>
        private void BeginInit(AnnotationData annotation)
        {
            if (!_initializedAnnotations.Contains(annotation))
            {
                _initializedAnnotations.Add(annotation);
                annotation.BeginInit();
            }
        }

        /// <summary>
        /// Ends initialization of specified annotation.
        /// </summary>
        private void EndInit(AnnotationData annotation)
        {
            if (_initializedAnnotations.Contains(annotation))
            {
                _initializedAnnotations.Remove(annotation);
                annotation.EndInit();
            }
        }

        /// <summary>
        /// Annotation transforming is started.
        /// </summary>
        private void annotationViewer1_AnnotationTransformingStarted(
            object sender,
            WpfAnnotationViewEventArgs e)
        {
            _isAnnotationTransforming = true;

            BeginInit(e.AnnotationView.Data);
            foreach (WpfAnnotationView view in annotationViewer1.SelectedAnnotations)
                BeginInit(view.Data);
        }

        /// <summary>
        /// Annotation transforming is finished.
        /// </summary>
        private void annotationViewer1_AnnotationTransformingFinished(
            object sender,
            WpfAnnotationViewEventArgs e)
        {
            _isAnnotationTransforming = false;

            EndInit(e.AnnotationView.Data);
            foreach (WpfAnnotationView view in annotationViewer1.SelectedAnnotations)
                EndInit(view.Data);

            annotationDataPropertyGrid.Refresh();
        }

        /// <summary>
        /// Annotation building is started.
        /// </summary>
        private void annotationViewer1_AnnotationBuildingStarted(object sender, WpfAnnotationViewEventArgs e)
        {
            annotationComboBox.IsEnabled = false;

            DisableUndoRedoMenu();
            if (_historyWindow != null)
                _historyWindow.CanNavigateOnHistory = false;
        }

        /// <summary>
        /// Annotation building is canceled.
        /// </summary>
        private void annotationViewer1_AnnotationBuildingCanceled(object sender, WpfAnnotationViewEventArgs e)
        {
            annotationComboBox.IsEnabled = true;

            EnableUndoRedoMenu();
            if (_historyWindow != null)
                _historyWindow.CanNavigateOnHistory = true;
        }

        /// <summary>
        /// Annotation building is finished.
        /// </summary>
        private void annotationViewer1_AnnotationBuildingFinished(
            object sender,
            WpfAnnotationViewEventArgs e)
        {
            bool isBuildingFinished = true;

            if (annotationToolBar.NeedBuildAnnotationsContinuously)
            {
                if (annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                    isBuildingFinished = false;
            }

            if (isBuildingFinished)
            {
                annotationComboBox.IsEnabled = true;

                EnableUndoRedoMenu();
                if (_historyWindow != null)
                    _historyWindow.CanNavigateOnHistory = true;
            }

            ShowAnnotationProperties(annotationViewer1.FocusedAnnotationView);
        }

        /// <summary>
        /// Disables the comment visual tool.
        /// </summary>
        private void NoneAction_Deactivated(object sender, EventArgs e)
        {
            _commentVisualTool.Enabled = false;
        }

        /// <summary>
        /// Enables the comment visual tool.
        /// </summary>
        private void NoneAction_Activated(object sender, EventArgs e)
        {
            _commentVisualTool.Enabled = true;
        }

        #endregion


        #region Selected annotations (Drag)

        /// <summary>
        /// Selected thumbnail collection is changed in thumbnail viewer.
        /// </summary>
        private void thumbnailViewer_SelectedThumbnailsChanged(object sender, RoutedEventArgs e)
        {
            if (thumbnailViewer.SelectedThumbnails.Count > 0)
                imageInfoStatusLabel.Text = string.Format("Selected {0} thumbnails", thumbnailViewer.SelectedThumbnails.Count);
            else
                imageInfoStatusLabel.Text = "";
        }

        /// <summary>
        /// Focused annotation is changed in annotation viewer.
        /// </summary>
        private void annotationViewer1_FocusedAnnotationViewChanged(
            object sender,
            WpfAnnotationViewChangedEventArgs e)
        {
            if (e.OldValue != null)
                e.OldValue.Data.PropertyChanged -= new EventHandler<ObjectPropertyChangedEventArgs>(FocusedAnnotationData_PropertyChanged);

            if (e.NewValue != null)
                e.NewValue.Data.PropertyChanged += new EventHandler<ObjectPropertyChangedEventArgs>(FocusedAnnotationData_PropertyChanged);
        }

        /// <summary>
        /// Focused annotation property is changed.
        /// </summary>
        private void FocusedAnnotationData_PropertyChanged(
            object sender,
            ObjectPropertyChangedEventArgs e)
        {            
            if (e.PropertyName == "Comment")
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Deletes the selected annotation or all annotations from image.
        /// </summary>
        /// <param name="deleteAll">Determines that all annotations must be deleted from image.</param>
        private void DeleteAnnotation(bool deleteAll)
        {
            annotationViewer1.CancelAnnotationBuilding();

            // get UI action
            UIAction deleteUIAction = null;
            if (deleteAll)
                deleteUIAction = GetUIAction<DeleteAllItemsUIAction>(annotationViewer1.VisualTool);
            else
                deleteUIAction = GetUIAction<DeleteItemUIAction>(annotationViewer1.VisualTool);

            // if action is not empty  AND action is enabled
            if (deleteUIAction != null && deleteUIAction.IsEnabled)
            {
                string actionName = "WpfAnnotationViewCollection: Delete";
                if (deleteAll)
                    actionName = actionName + " All";
                _undoManager.BeginCompositeAction(actionName);
                try
                {
                    deleteUIAction.Execute();
                }
                finally
                {
                    _undoManager.EndCompositeAction();
                }
            }

            UpdateUI();
        }

        #endregion


        #region Image/annotations undo manager

        /// <summary>
        /// Updates the "Undo/Redo" menu.
        /// </summary>
        private void UpdateUndoRedoMenu(UndoManager undoManager)
        {
            bool canUndo = false;
            bool canRedo = false;

            if (undoManager != null && undoManager.IsEnabled)
            {
                if (!annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding)
                {
                    canUndo = undoManager.UndoCount > 0;
                    canRedo = undoManager.RedoCount > 0;
                }
            }

            string undoMenuItemText = "Undo";
            if (canUndo && !string.IsNullOrEmpty(undoManager.UndoDescription))
                undoMenuItemText = string.Format("Undo {0}", undoManager.UndoDescription).Trim();

            undoMenuItem.IsEnabled = canUndo;
            undoMenuItem.Header = undoMenuItemText;


            string redoMenuItemText = "Redo";
            if (canRedo && !string.IsNullOrEmpty(undoManager.RedoDescription))
                redoMenuItemText = string.Format("Redo {0}", undoManager.RedoDescription).Trim();

            redoMenuItem.IsEnabled = canRedo;
            redoMenuItem.Header = redoMenuItemText;
        }

        /// <summary>
        /// Enables the undo redo menu.
        /// </summary>
        private void EnableUndoRedoMenu()
        {
            UpdateUndoRedoMenu(_undoManager);
            undoRedoSettingsMenuItem.IsEnabled = true;
        }

        /// <summary>
        /// Disables the undo redo menu.
        /// </summary>
        private void DisableUndoRedoMenu()
        {
            undoMenuItem.IsEnabled = false;
            redoMenuItem.IsEnabled = false;
            undoRedoSettingsMenuItem.IsEnabled = false;
        }

        /// <summary>
        /// Image/annotations undo manager is changed.
        /// </summary>
        private void annotationUndoManager_Changed(object sender, UndoManagerChangedEventArgs e)
        {
            UpdateUndoRedoMenu((UndoManager)sender);
        }

        /// <summary>
        /// Image/annotations undo manager is navigated.
        /// </summary>
        private void annotationUndoManager_Navigated(object sender, UndoManagerNavigatedEventArgs e)
        {
            UpdateUndoRedoMenu((UndoManager)sender);
        }

        /// <summary>
        /// Shows the history window.
        /// </summary>
        private void ShowHistoryWindow()
        {
            if (annotationViewer1.Image == null)
                return;

            _historyWindow = new WpfUndoManagerHistoryWindow(this, _undoManager);
            _historyWindow.CanNavigateOnHistory = !annotationViewer1.AnnotationVisualTool.IsFocusedAnnotationBuilding;
            _historyWindow.Closed += new EventHandler(historyWindow_Closed);
            _historyWindow.Show();
        }

        /// <summary>
        /// Closes the history window.
        /// </summary>
        private void CloseHistoryWindow()
        {
            if (_historyWindow != null)
                _historyWindow.Close();
        }

        /// <summary>
        /// History window is closed.
        /// </summary>
        private void historyWindow_Closed(object sender, EventArgs e)
        {
            historyDialogMenuItem.IsChecked = false;
            _historyWindow = null;
        }

        #endregion


        #region Save image(s)

        /// <summary>
        /// Saves image collection with annotations to the first source of image collection,
        /// i.e. saves modified image collection with annotations back to the source file.
        /// </summary>
        private void SaveImageCollectionToSourceFile()
        {
            // cancel annotation building
            annotationViewer1.CancelAnnotationBuilding();

            // if focused image is NOT correct
            if (!AnnotationDemosTools.CheckImage(annotationViewer1))
                return;

            EncoderBase encoder = null;
            try
            {
                // specify that image file saving is started
                IsFileSaving = true;

                // if image collection contains several images
                if (annotationViewer1.Images.Count > 1)
                    // get multipage encoder
                    encoder = GetMultipageEncoder(_sourceFilename, true, false);
                // if image collection contains single image
                else
                    // get single- or multipage encoder
                    encoder = GetEncoder(_sourceFilename, true);
                // if encoder is found
                if (encoder != null)
                {
                    encoder.SaveAndSwitchSource = true;

                    // save image collection to a file
                    annotationViewer1.Images.SaveAsync(_sourceFilename, encoder);
                }
                // if encoder is NOT found
                else
                    // open save file dialog and save image collection to the new multipage image file
                    SaveImageCollectionToMultipageImageFile(true);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
            finally
            {
                // specify that image file saving is finished
                IsFileSaving = false;
            }
        }

        /// <summary>
        /// Opens the save file dialog and saves image collection to the new multipage image file.
        /// </summary>
        private void SaveImageCollectionToMultipageImageFile(bool saveAs)
        {
            // cancel annotation building
            annotationViewer1.CancelAnnotationBuilding();

            // if focused image is NOT correct
            if (!AnnotationDemosTools.CheckImage(annotationViewer1))
                return;

            // specify that image file saving is started
            IsFileSaving = true;

            bool multipage = annotationViewer1.Images.Count > 1;

            // set file filters in file saving dialog
            CodecsFileFilters.SetFiltersWithAnnotations(saveFileDialog1, multipage);
            // show the file saving dialog
            if (saveFileDialog1.ShowDialog().Value)
            {
                EncoderBase encoder = null;
                try
                {
                    string saveFilename = Path.GetFullPath(saveFileDialog1.FileName);
                    // if multiple images must be saved
                    if (multipage)
                        // get image encoder for multi page image file
                        encoder = GetMultipageEncoder(saveFilename, true, saveAs);
                    // if single image must be saved
                    else
                        // get image encoder for single page image file
                        encoder = GetEncoder(saveFilename, true);
                    // if encoder is found
                    if (encoder != null)
                    {
                        if (saveAs)
                            _saveFilename = saveFilename;
                        encoder.SaveAndSwitchSource = saveAs;

                        // save images to an image file
                        annotationViewer1.Images.SaveAsync(saveFilename, encoder);
                    }
                    else
                        DemosTools.ShowErrorMessage("Image encoder is not found.");
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                    // specify that image file saving is finished
                    IsFileSaving = false;
                }

                if (!saveAs)
                    // specify that image file saving is finished
                    IsFileSaving = false;
            }
            else
            {
                // specify that image file saving is finished
                IsFileSaving = false;
            }
        }

        /// <summary>
        /// Opens the save file dialog and saves focused image to the new image file.
        /// </summary>
        private void SaveSingleImageToNewImageFile()
        {
            // cancel annotation building
            annotationViewer1.CancelAnnotationBuilding();

            // if focused image is NOT correct
            if (!AnnotationDemosTools.CheckImage(annotationViewer1))
                return;

            // specify that image file saving is started
            IsFileSaving = true;

            // set file filters in file saving dialog
            CodecsFileFilters.SetFiltersWithAnnotations(saveFileDialog1, false);
            saveFileDialog1.FileName = "";
            // show the file saving dialog
            if (saveFileDialog1.ShowDialog().Value)
            {
                try
                {
                    string fileName = Path.GetFullPath(saveFileDialog1.FileName);
                    // set encoder parameters, if necessary
                    EncoderBase encoder = GetEncoder(fileName, true);
                    // if encoder is found
                    if (encoder != null)
                    {
                        encoder.SaveAndSwitchSource = false;

                        // save images to an image file
                        annotationViewer1.Image.Save(fileName, encoder, SavingProgress);
                    }
                    else
                        DemosTools.ShowErrorMessage("Image encoder is not found.");
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }

            // specify that image file saving is finished
            IsFileSaving = false;
        }

        /// <summary>
        /// Returns the encoder for saving of single image.
        /// </summary>
        private EncoderBase GetEncoder(string filename, bool showSettingsDialog)
        {
            // get multipage encoder
            MultipageEncoderBase multipageEncoder = GetMultipageEncoder(filename, showSettingsDialog, false);
            // if multipage encoder is found
            if (multipageEncoder != null)
                // return multipage encoder
                return multipageEncoder;

            switch (Path.GetExtension(filename).ToUpperInvariant())
            {
                case ".JPG":
                case ".JPEG":
                    JpegEncoder jpegEncoder = new JpegEncoder();

                    if (showSettingsDialog)
                    {
                        jpegEncoder.Settings.AnnotationsFormat = AnnotationsFormat.VintasoftBinary;

                        JpegEncoderSettingsWindow jpegEncoderSettingsDlg = new JpegEncoderSettingsWindow();
                        jpegEncoderSettingsDlg.EditAnnotationSettings = true;
                        jpegEncoderSettingsDlg.EncoderSettings = jpegEncoder.Settings;
                        jpegEncoderSettingsDlg.Owner = this;
                        if (jpegEncoderSettingsDlg.ShowDialog() == false)
                            throw new Exception("Saving canceled.");
                    }

                    return jpegEncoder;

                case ".PNG":
                    PngEncoder pngEncoder = new PngEncoder();

                    if (showSettingsDialog)
                    {
                        pngEncoder.Settings.AnnotationsFormat = AnnotationsFormat.VintasoftBinary;

                        PngEncoderSettingsWindow pngEncoderSettingsDlg = new PngEncoderSettingsWindow();
                        pngEncoderSettingsDlg.EditAnnotationSettings = true;
                        pngEncoderSettingsDlg.EncoderSettings = pngEncoder.Settings;
                        pngEncoderSettingsDlg.Owner = this;
                        if (pngEncoderSettingsDlg.ShowDialog() == false)
                            throw new Exception("Saving canceled.");
                    }

                    return pngEncoder;

                // if annotations are not supported
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the multipage encoder for saving of image collection.
        /// </summary>
        private MultipageEncoderBase GetMultipageEncoder(
            string filename,
            bool showSettingsDialog,
            bool switchTo)
        {
            bool isFileExist = File.Exists(filename) && !switchTo;
            switch (Path.GetExtension(filename).ToUpperInvariant())
            {
#if !REMOVE_PDF_PLUGIN
                case ".PDF":
                    IPdfEncoder pdfEncoder = (IPdfEncoder)AvailableEncoders.CreateEncoderByName("Pdf");

                    if (showSettingsDialog)
                    {
                        pdfEncoder.Settings.AnnotationsFormat = AnnotationsFormat.VintasoftBinary;

                        PdfEncoderSettingsWindow pdfEncoderSettingsDlg = new PdfEncoderSettingsWindow();
                        pdfEncoderSettingsDlg.AppendExistingDocumentEnabled = isFileExist;
                        pdfEncoderSettingsDlg.CanEditAnnotationSettings = true;
                        pdfEncoderSettingsDlg.EncoderSettings = pdfEncoder.Settings;
                        pdfEncoderSettingsDlg.Owner = this;
                        if (pdfEncoderSettingsDlg.ShowDialog() == false)
                            throw new Exception("Saving canceled.");
                    }

                    return (MultipageEncoderBase)pdfEncoder;
#endif

                case ".TIF":
                case ".TIFF":
                    TiffEncoder tiffEncoder = new TiffEncoder();

                    if (showSettingsDialog)
                    {
                        tiffEncoder.Settings.AnnotationsFormat = AnnotationsFormat.VintasoftBinary;

                        TiffEncoderSettingsWindow tiffEncoderSettingsDlg = new TiffEncoderSettingsWindow();
                        tiffEncoderSettingsDlg.CanAddImagesToExistingFile = isFileExist;
                        tiffEncoderSettingsDlg.EditAnnotationSettings = true;
                        tiffEncoderSettingsDlg.EncoderSettings = tiffEncoder.Settings;
                        tiffEncoderSettingsDlg.Owner = this;
                        if (tiffEncoderSettingsDlg.ShowDialog() == false)
                            throw new Exception("Saving canceled.");
                    }

                    return tiffEncoder;
            }

            return null;
        }

        /// <summary>
        /// Image collection saving is in progress.
        /// </summary>
        private void SavingProgress(object sender, ProgressEventArgs e)
        {
            Dispatcher.Invoke(new UpdateSavingProgressDelegate(UpdateSavingProgress), e.Progress);
        }

        private void UpdateSavingProgress(int progress)
        {
            actionLabel.Content = "Saving:";
            thumbnailLoadingProgerssBar.Value = progress;
            if (progress != 100)
                thumbnailLoadingProgerssBar.Visibility = Visibility.Visible;
            else
                thumbnailLoadingProgerssBar.Visibility = Visibility.Collapsed;
            actionLabel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Image collection is saved.
        /// </summary>
        private void Images_ImageCollectionSavingFinished(object sender, EventArgs e)
        {
            if (_saveFilename != null)
            {
                CloseSource();
                _sourceFilename = _saveFilename;
                _saveFilename = null;
                _isFileReadOnlyMode = false;
            }

#if !REMOVE_PDF_PLUGIN
            _pdfDocument = null;
            Vintasoft.Imaging.Pdf.Tree.PdfPage page = Vintasoft.Imaging.Pdf.PdfDocumentController.GetPageAssociatedWithImage(annotationViewer1.Images[0]);
            if (page != null)
                _pdfDocument = page.Document;
#endif

            IsFileSaving = false;
        }

        /// <summary>
        /// Handles the ImageSavingException event of Images object.
        /// </summary>
        private void Images_ImageSavingException(object sender, ExceptionEventArgs e)
        {
            DemosTools.ShowErrorMessage(e.Exception);
        }

        #endregion


        #region Hot keys

        /// <summary>
        /// Handles the CanExecute event of openCommandBinding object.
        /// </summary>
        private void openCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = openImageMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of addCommandBinding object.
        /// </summary>
        private void addCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = addImagesMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of saveAsCommandBinding object.
        /// </summary>
        private void saveAsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = saveAsMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of closeCommandBinding object.
        /// </summary>
        private void closeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = closeMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of printCommandBinding object.
        /// </summary>
        private void printCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = printMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of exitCommandBinding object.
        /// </summary>
        private void exitCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = exitMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of cutCommandBinding object.
        /// </summary>
        private void cutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = cutMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of copyCommandBinding object.
        /// </summary>
        private void copyCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = copyMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of pasteCommandBinding object.
        /// </summary>
        private void pasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = pasteMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of deleteCommandBinding object.
        /// </summary>
        private void deleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = deleteMenuItem.IsEnabled && annotationViewer1.IsMouseOver;
            e.ContinueRouting = !e.CanExecute;
        }

        /// <summary>
        /// Handles the CanExecute event of deleteAllCommandBinding object.
        /// </summary>
        private void deleteAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = deleteAllMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of selectAllCommandBinding object.
        /// </summary>
        private void selectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = selectAllMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of groupCommandBinding object.
        /// </summary>
        private void groupCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = groupSelectedMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of groupAllCommandBinding object.
        /// </summary>
        private void groupAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = groupAllMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateClockwiseCommandBinding object.
        /// </summary>
        private void rotateClockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateClockwiseMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateCounterclockwiseCommandBinding object.
        /// </summary>
        private void rotateCounterclockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateCounterclockwiseMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of undoCommandBinding object.
        /// </summary>
        private void undoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undoMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of redoCommandBinding object.
        /// </summary>
        private void redoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redoMenuItem.IsEnabled;
        }

        #endregion

        #endregion



        #region Delegates

        private delegate void UpdateUIDelegate();

        private delegate void SetCursorDelegate(Cursor cursor);

        private delegate void CloseCurrentFileDelegate();

        private delegate void UpdateSavingProgressDelegate(int progress);

        #endregion

    }
}
