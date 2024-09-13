using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs;
#if !REMOVE_PDF_PLUGIN
using Vintasoft.Imaging.Pdf;
#endif
using Vintasoft.Imaging.ImageProcessing.Info;
using Vintasoft.Imaging.Codecs.ImageFiles.Jpeg2000;
using Vintasoft.Imaging.Codecs.Encoders;


namespace WpfDemosCommonCode.Imaging.Codecs.Dialogs
{
    /// <summary>
    /// A form that allows to view and edit the PDF encoder settings.
    /// </summary>

    public partial class PdfEncoderSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// Indicates that the MRC compression profile is initializing.
        /// </summary>
        bool _isMrcCompressionProfileInitializing = false;

        #endregion



        #region Constructors

        public PdfEncoderSettingsWindow()
        {
            InitializeComponent();

            updateModeComboBox.Items.Add(PdfDocumentUpdateMode.Auto);
            updateModeComboBox.Items.Add(PdfDocumentUpdateMode.Incremental);
            updateModeComboBox.Items.Add(PdfDocumentUpdateMode.Pack);
            updateModeComboBox.Items.Add(PdfDocumentUpdateMode.CleanupAndPack);
            updateModeComboBox.SelectedIndex = 0;

#if !REMOVE_PDF_PLUGIN
            PdfDocumentConformance[] conformances = new PdfDocumentConformance[] {
                 PdfDocumentConformance.PdfA_1a,
                 PdfDocumentConformance.PdfA_1b,
                 PdfDocumentConformance.PdfA_2a,
                 PdfDocumentConformance.PdfA_2b,
                 PdfDocumentConformance.PdfA_2u,
                 PdfDocumentConformance.PdfA_3a,
                 PdfDocumentConformance.PdfA_3b,
                 PdfDocumentConformance.PdfA_3u,
                 PdfDocumentConformance.PdfA_4,
                 PdfDocumentConformance.PdfA_4e,
                 PdfDocumentConformance.PdfA_4f,
            };
            foreach (PdfDocumentConformance conformance in conformances)
                conformanceComboBox.Items.Add(DemosTools.ConvertToString(conformance));
            conformanceComboBox.SelectedIndex = 1;
#endif

            compressionImageRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionImageRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionMrcRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionMrcRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            mrcUseFrontCheckBox.Checked += new RoutedEventHandler(mrcUseFrontCheckBox_CheckedChanged);
            mrcUseFrontCheckBox.Unchecked += new RoutedEventHandler(mrcUseFrontCheckBox_CheckedChanged);
            mrcHiQualityMaskCheckBox.Checked += new RoutedEventHandler(mrcHiQualityMaskCheckBox_CheckedChanged);
            mrcHiQualityMaskCheckBox.Unchecked += new RoutedEventHandler(mrcHiQualityMaskCheckBox_CheckedChanged);
            mrcHiQualityFrontLayerCheckBox.Checked += new RoutedEventHandler(mrcHiQualityFrontLayerCheckBox_CheckedChanged);
            mrcHiQualityFrontLayerCheckBox.Unchecked += new RoutedEventHandler(mrcHiQualityFrontLayerCheckBox_CheckedChanged);
            mrcUseBackgroundLayerCheckBox.Checked += new RoutedEventHandler(mrcUseBackgroundLayerCheckBox_CheckedChanged);
            mrcUseBackgroundLayerCheckBox.Unchecked += new RoutedEventHandler(mrcUseBackgroundLayerCheckBox_CheckedChanged);
            mrcImageSegmentationCheckBox.Checked += new RoutedEventHandler(mrcImageSegmentationCheckBox_CheckedChanged);
            mrcImageSegmentationCheckBox.Unchecked += new RoutedEventHandler(mrcImageSegmentationCheckBox_CheckedChanged);
            mrcNotUseImagesLayerRadioButton.Checked += new RoutedEventHandler(mrcNotUseImagesLayerRadioButton_CheckedChanged);
            mrcNotUseImagesLayerRadioButton.Unchecked += new RoutedEventHandler(mrcNotUseImagesLayerRadioButton_CheckedChanged);
            mrcUseImagesLayerRadioButton.Checked += new RoutedEventHandler(mrcUseImagesLayerRadioButton_CheckedChanged);
            mrcUseImagesLayerRadioButton.Unchecked += new RoutedEventHandler(mrcUseImagesLayerRadioButton_CheckedChanged);
            appendCheckBox.Checked += new RoutedEventHandler(appendCheckBox_CheckedChanged);
            appendCheckBox.Unchecked += new RoutedEventHandler(appendCheckBox_CheckedChanged);

            mrcCompressionProfileComboBox.SelectionChanged += new SelectionChangedEventHandler(mrcCompressionProfileComboBox_SelectionChanged);

            CanEditAnnotationSettings = false;

#if !REMOVE_PDF_PLUGIN
            // if PDF encoder cannot generate annotation appearance in PDF document
            if (!PdfEncoder.CanGeneratePdfAnnotationAppearance)
            {
                // disable the ability to generate annotation appearance in PDF document
                annotationsPdfAppearanceCheckBox.IsEnabled = false;
            }
#endif
        }

        #endregion



        #region Properties

        bool _isMrcCompressionOnly = false;
        /// <summary>
        /// Gets or sets a value that indicates whether PDF document can be compressed
        /// with MRC compression only.
        /// </summary>
        /// <value>
        /// <b>true</b> - PDF document can be compressed with MRC compression only;
        /// <b>false</b> - PDF document can be compressed with or without MRC compression.
        /// </value>
        public bool IsMrcCompressionOnly
        {
            get
            {
                return _isMrcCompressionOnly;
            }
            set
            {
                compressionImageRadioButton.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                compressionMrcRadioButton.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                compressionMrcRadioButton.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                UpdateUI();

                _isMrcCompressionOnly = value;
            }
        }

        bool _allowMrcCompression = true;
        /// <summary>
        /// Gets or sets a value that indicates whether PDF document can be compressed with MRC.
        /// </summary>
        /// <value>
        /// <b>True</b> - PDF document can be compressed with or without MRC compression;
        /// <b>false</b> - PDF document can be compressed with image compression only.
        /// </value>
        public bool AllowMrcCompression
        {
            get
            {
                return _allowMrcCompression;
            }
            set
            {
                if (value)
                {
                    compressionImageRadioButton.Visibility = Visibility.Visible;
                    compressionMrcRadioButton.Visibility = Visibility.Visible;
                }
                else
                {
                    compressionImageRadioButton.Visibility = Visibility.Collapsed;
                    compressionMrcRadioButton.Visibility = Visibility.Collapsed;
                }

                if (!value)
                    compressionImageRadioButton.IsChecked = true;

                UpdateUI();

                _allowMrcCompression = value;
            }
        }

        PdfEncoderSettings _encoderSettings;
        /// <summary>
        /// Gets or sets the PDF encoder settings.
        /// </summary>
        public PdfEncoderSettings EncoderSettings
        {
            get
            {
                return _encoderSettings;
            }
            set
            {
                if (value == null)
                    throw new ArgumentOutOfRangeException();
                if (_encoderSettings != value)
                {
                    _encoderSettings = value;
#if !REMOVE_PDF_PLUGIN
                    pdfImageCompressionControl.Compression = ConvertToPdfCompression(_encoderSettings.Compression);
                    PdfCompressionSettings settings = new PdfCompressionSettings();
                    settings.Jbig2Settings = _encoderSettings.Jbig2Settings;
                    settings.Jbig2UseGlobals = _encoderSettings.Jbig2UseGlobals;
                    settings.Jpeg2000Settings = _encoderSettings.Jpeg2000Settings;
                    settings.JpegSettings = _encoderSettings.JpegSettings;
                    settings.BinarizationCommand = _encoderSettings.BinarizationCommand;
                    pdfImageCompressionControl.CompressionSettings = settings;
                    if (CanEditImageTilesSettings)
                    {
                        tileWidthCheckBox.IsChecked = _encoderSettings.ImageTileWidth > 0;
                        tileHeightCheckBox.IsChecked = _encoderSettings.ImageTileHeight > 0;
                        if (tileHeightCheckBox.IsChecked.Value)
                            tileHeightNumericUpDown.Value = _encoderSettings.ImageTileHeight;
                        if (tileWidthCheckBox.IsChecked.Value)
                            tileWidthNumericUpDown.Value = _encoderSettings.ImageTileWidth;
                    }
#endif
                    UpdateUI();
                }
            }
        }

#if !REMOVE_PDF_PLUGIN
        PdfMrcCompressionSettings _mrcCompressionSettings = null;
        /// <summary>
        /// Gets or sets the PDF MRC compression settings.
        /// </summary>
        public PdfMrcCompressionSettings MrcCompressionSettings
        {
            get
            {
                return _mrcCompressionSettings;
            }
            set
            {
                _mrcCompressionSettings = value;
                if (value != null)
                {
                    mrcBackgroundCompressionControl.Compression = value.BackgroundLayerCompression;
                    mrcBackgroundCompressionControl.CompressionSettings = value.BackgroundLayerCompressionSettings;
                    mrcImagesCompressionControl.Compression = value.ImagesLayerCompression;
                    mrcImagesCompressionControl.CompressionSettings = value.ImagesLayerCompressionSettings;
                    mrcMaskCompressionControl.Compression = value.MaskCompression;
                    mrcMaskCompressionControl.CompressionSettings = value.MaskCompressionSettings;
                    mrcFrontCompressionControl.Compression = value.FrontLayerCompression;
                    mrcFrontCompressionControl.CompressionSettings = value.FrontLayerCompressionSettings;
                }
            }
        }
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether the images can be added
        /// to an existing PDF document.
        /// </summary>
        public bool AppendExistingDocument
        {
            get
            {
                return appendCheckBox.IsChecked.Value;
            }
            set
            {
                appendCheckBox.IsChecked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether <see cref="AppendExistingDocument"/> is enabled.
        /// </summary>
        public bool AppendExistingDocumentEnabled
        {
            get
            {
                return appendCheckBox.IsEnabled;
            }
            set
            {
                appendCheckBox.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the annotation settings can be edited.
        /// </summary>
        public bool CanEditAnnotationSettings
        {
            get
            {
                return tabControl1.Items.Contains(annotationsTabItem);
            }
            set
            {
                if (CanEditAnnotationSettings != value)
                {
                    if (value)
                        tabControl1.Items.Add(annotationsTabItem);
                    else
                        tabControl1.Items.Remove(annotationsTabItem);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the image tiles settings can be edited.
        /// </summary>
        public bool CanEditImageTilesSettings
        {
            get
            {
                return imageTilesGroupBox.IsEnabled;
            }
            set
            {
                if (CanEditImageTilesSettings != value)
                {
                    imageTilesGroupBox.IsEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether PDF annotation appearance should be generated.
        /// </summary>
        public bool GeneratePdfAnnotationsAppearence
        {
            get
            {
                return annotationsPdfAppearanceCheckBox.IsChecked.Value;
            }
            set
            {
                annotationsPdfAppearanceCheckBox.IsChecked = value;
            }
        }

        #endregion



        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (EncoderSettings == null)
                EncoderSettings = new PdfEncoderSettings();


            if (CanEditAnnotationSettings)
            {
                tabControl1.Items.Remove(annotationsTabItem);
                tabControl1.Items.Insert(1, annotationsTabItem);
            }
        }

        /// <summary>
        /// Compression type is changed.
        /// </summary>
        void compressionRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            if (compressionMrcRadioButton.IsChecked.Value)
            {
                if (MrcCompressionSettings != null)
                    MrcCompressionSettings.EnableMrcCompression = true;
                else
                    mrcCompressionProfileComboBox.SelectedIndex = 2;
            }
            else
            {
                if (MrcCompressionSettings != null)
                    MrcCompressionSettings.EnableMrcCompression = false;
            }
#endif
            UpdateUI();
        }

        /// <summary>
        /// 'Append to existing document' checkbox state is changed.
        /// </summary>
        void appendCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            pdfaCheckBox.IsEnabled = !appendCheckBox.IsChecked.Value;
            if (appendCheckBox.IsChecked.Value)
                pdfaCheckBox.IsChecked = false;
        }

        /// <summary>
        /// 'OK' button is pressed.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            SyncEncoderSettingsWithUI();

            DialogResult = true;
        }

        /// <summary>
        /// 'Cancel' button is pressed.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            pdfaCheckBox.IsChecked = _encoderSettings.PdfACompatible;
            if (_encoderSettings.PdfACompatible)
                conformanceComboBox.SelectedItem = DemosTools.ConvertToString(_encoderSettings.Conformance);

            tileWidthNumericUpDown.IsEnabled = tileWidthCheckBox.IsChecked.Value;
            tileHeightNumericUpDown.IsEnabled = tileHeightCheckBox.IsChecked.Value;

            titleTextBox.Text = _encoderSettings.DocumentTitle;
            authorTextBox.Text = _encoderSettings.DocumentAuthor;
            creatorTextBox.Text = _encoderSettings.DocumentCreator;
            keywordsTextBox.Text = _encoderSettings.DocumentKeywords;
            producerTextBox.Text = _encoderSettings.DocumentProducer;
            subjectTextBox.Text = _encoderSettings.DocumentSubject;

            annotationsBinaryCheckBox.IsChecked = (EncoderSettings.AnnotationsFormat & AnnotationsFormat.VintasoftBinary) != 0;
            annotationXmpCheckBox.IsChecked = (EncoderSettings.AnnotationsFormat & AnnotationsFormat.VintasoftXmp) != 0;

            createThumbnailsCheckBox.IsChecked = EncoderSettings.CreateThumbnails;

            if (annotationsPdfAppearanceCheckBox.IsEnabled)
                annotationsPdfAppearanceCheckBox.IsChecked = _encoderSettings.GenerateAnnotationAppearance;

#if !REMOVE_PDF_PLUGIN
            if (_mrcCompressionSettings != null && _mrcCompressionSettings.EnableMrcCompression)
            {
                compressionMrcRadioButton.IsChecked = true;
                compressionImageRadioButton.IsChecked = false;
                mrcUseImagesLayerRadioButton.IsChecked = _mrcCompressionSettings.CreateImagesLayer;
                mrcNotUseImagesLayerRadioButton.IsChecked = !_mrcCompressionSettings.CreateImagesLayer;
                mrcUseFrontCheckBox.IsChecked = _mrcCompressionSettings.CreateFrontLayer;
                mrcHiQualityMaskCheckBox.IsChecked = _mrcCompressionSettings.HiQualityMask;
                mrcHiQualityFrontLayerCheckBox.IsChecked = _mrcCompressionSettings.HiQualityFrontLayer;
                mrcHiQualityFrontLayerCheckBox.IsEnabled = _mrcCompressionSettings.CreateFrontLayer;
                mrcUseBackgroundLayerCheckBox.IsChecked = _mrcCompressionSettings.CreateBackgroundLayer;
                mrcAddPdfLayersCheckBox.IsChecked = _mrcCompressionSettings.AddPdfLayers;
#if !REMOVE_DOCCLEANUP_PLUGIN
                mrcImageSegmentationCheckBox.IsChecked = _mrcCompressionSettings.ImageSegmentation != null;
                mrcImageSegmentationSettingsButton.IsEnabled = _mrcCompressionSettings.ImageSegmentation != null;
#endif

                mrcBackgroundCompressionControl.IsEnabled = _mrcCompressionSettings.CreateBackgroundLayer;
                mrcImagesCompressionControl.IsEnabled = _mrcCompressionSettings.CreateImagesLayer;
                mrcFrontCompressionControl.IsEnabled = _mrcCompressionSettings.CreateFrontLayer;
            }
            else
            {
                compressionMrcRadioButton.IsChecked = false;
                compressionImageRadioButton.IsChecked = true;
            }
#endif

            Visibility visibility = compressionMrcRadioButton.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            mrcCompressionSettingsGroupBox.Visibility = visibility;
            mrcCompressionProfileComboBox.Visibility = visibility;
            pdfImageCompressionControl.Visibility = compressionImageRadioButton.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            updateModeComboBox.SelectedItem = _encoderSettings.UpdateMode;
        }

        /// <summary>
        /// Synchronizes the <see cref="EncoderSettings"/> property with UI.
        /// </summary>
        private void SyncEncoderSettingsWithUI()
        {
#if !REMOVE_PDF_PLUGIN
            if (pdfaCheckBox.IsChecked.Value == true)
                EncoderSettings.Conformance = DemosTools.ConvertFromString(conformanceComboBox.SelectedItem.ToString());
            else
                EncoderSettings.Conformance = PdfDocumentConformance.Undefined;
#endif

            if (CanEditImageTilesSettings)
            {
                if (tileWidthCheckBox.IsChecked.Value)
                    EncoderSettings.ImageTileWidth = (int)tileWidthNumericUpDown.Value;
                else
                    EncoderSettings.ImageTileWidth = 0;

                if (tileHeightCheckBox.IsChecked.Value)
                    EncoderSettings.ImageTileHeight = (int)tileHeightNumericUpDown.Value;
                else
                    EncoderSettings.ImageTileHeight = 0;
            }

            EncoderSettings.DocumentTitle = titleTextBox.Text;
            EncoderSettings.DocumentAuthor = authorTextBox.Text;
            EncoderSettings.DocumentCreator = creatorTextBox.Text;
            EncoderSettings.DocumentKeywords = keywordsTextBox.Text;
            EncoderSettings.DocumentProducer = producerTextBox.Text;
            EncoderSettings.DocumentSubject = subjectTextBox.Text;

#if !REMOVE_PDF_PLUGIN
            EncoderSettings.Compression = ConvertToPdfImageCompression(pdfImageCompressionControl.Compression);

            EncoderSettings.Jbig2Settings = pdfImageCompressionControl.CompressionSettings.Jbig2Settings;
            EncoderSettings.Jbig2UseGlobals = pdfImageCompressionControl.CompressionSettings.Jbig2UseGlobals;
            EncoderSettings.Jpeg2000Settings = pdfImageCompressionControl.CompressionSettings.Jpeg2000Settings;
            EncoderSettings.JpegQuality = pdfImageCompressionControl.CompressionSettings.JpegQuality;
            EncoderSettings.JpegSaveAsGrayscale = pdfImageCompressionControl.CompressionSettings.JpegSaveAsGrayscale;
            EncoderSettings.ZipCompressionLevel = pdfImageCompressionControl.CompressionSettings.ZipCompressionLevel;
#endif

            if (CanEditAnnotationSettings)
            {
                EncoderSettings.AnnotationsFormat = AnnotationsFormat.None;
                if (annotationsBinaryCheckBox.IsChecked.Value)
                    EncoderSettings.AnnotationsFormat |= AnnotationsFormat.VintasoftBinary;
                if (annotationXmpCheckBox.IsChecked.Value)
                    EncoderSettings.AnnotationsFormat |= AnnotationsFormat.VintasoftXmp;
            }

            EncoderSettings.CreateThumbnails = createThumbnailsCheckBox.IsChecked.Value;

            EncoderSettings.GenerateAnnotationAppearance = annotationsPdfAppearanceCheckBox.IsChecked.Value;

            EncoderSettings.UpdateMode = (PdfDocumentUpdateMode)updateModeComboBox.SelectedItem;

#if !REMOVE_PDF_PLUGIN
            if (compressionMrcRadioButton.IsChecked.Value)
            {
                if (_mrcCompressionSettings == null)
                    _mrcCompressionSettings = new PdfMrcCompressionSettings();

                _mrcCompressionSettings.EnableMrcCompression = true;
                _mrcCompressionSettings.CreateImagesLayer = mrcUseImagesLayerRadioButton.IsChecked.Value;
                _mrcCompressionSettings.CreateBackgroundLayer = mrcUseBackgroundLayerCheckBox.IsChecked.Value;
                _mrcCompressionSettings.HiQualityMask = mrcHiQualityMaskCheckBox.IsChecked.Value;
                _mrcCompressionSettings.HiQualityFrontLayer = mrcHiQualityFrontLayerCheckBox.IsChecked.Value;
                _mrcCompressionSettings.CreateFrontLayer = mrcUseFrontCheckBox.IsChecked.Value;
                _mrcCompressionSettings.BackgroundLayerCompression = mrcBackgroundCompressionControl.Compression;
                _mrcCompressionSettings.BackgroundLayerCompressionSettings = mrcBackgroundCompressionControl.CompressionSettings;
                _mrcCompressionSettings.ImagesLayerCompression = mrcImagesCompressionControl.Compression;
                _mrcCompressionSettings.ImagesLayerCompressionSettings = mrcImagesCompressionControl.CompressionSettings;
                _mrcCompressionSettings.MaskCompression = mrcMaskCompressionControl.Compression;
                _mrcCompressionSettings.MaskCompressionSettings = mrcMaskCompressionControl.CompressionSettings;
                _mrcCompressionSettings.FrontLayerCompression = mrcFrontCompressionControl.Compression;
                _mrcCompressionSettings.FrontLayerCompressionSettings = mrcFrontCompressionControl.CompressionSettings;
                _mrcCompressionSettings.AddPdfLayers = mrcAddPdfLayersCheckBox.IsChecked.Value;
#if !REMOVE_DOCCLEANUP_PLUGIN
                if (mrcImageSegmentationCheckBox.IsChecked.Value)
                {
                    if (_mrcCompressionSettings.ImageSegmentation == null)
                        _mrcCompressionSettings.ImageSegmentation = new Vintasoft.Imaging.ImageProcessing.Info.ImageSegmentationCommand();
                }
                else
                {
                    _mrcCompressionSettings.ImageSegmentation = null;
                }
#endif
            }
            else
            {
                if (_mrcCompressionSettings != null)
                    _mrcCompressionSettings.EnableMrcCompression = false;
            }
#endif
        }

#if !REMOVE_PDF_PLUGIN
        /// <summary>
        /// Converts <see cref="PdfCompression"/> enum to <see cref="PdfImageCompression"/> enum.
        /// </summary>
        private PdfImageCompression ConvertToPdfImageCompression(PdfCompression compression)
        {
            switch (compression)
            {
                case PdfCompression.CcittFax:
                    return PdfImageCompression.CcittFax;

                case PdfCompression.Jbig2:
                    return PdfImageCompression.Jbig2;

                case PdfCompression.Jpeg:
                    return PdfImageCompression.Jpeg;

                case PdfCompression.Jpeg2000:
                    return PdfImageCompression.Jpeg2000;

                case PdfCompression.Lzw:
                    return PdfImageCompression.Lzw;

                case PdfCompression.None:
                    return PdfImageCompression.None;

                case PdfCompression.Zip:
                    return PdfImageCompression.Zip;

                case PdfCompression.Zip | PdfCompression.Jpeg:
                    return PdfImageCompression.Zip | PdfImageCompression.Jpeg;
            }
            return PdfImageCompression.Auto;
        }

        /// <summary>
        /// Converts <see cref="PdfImageCompression"/> enum to <see cref="PdfCompression"/> enum.
        /// </summary>
        private PdfCompression ConvertToPdfCompression(PdfImageCompression compression)
        {
            switch (compression)
            {
                case PdfImageCompression.CcittFax:
                    return PdfCompression.CcittFax;

                case PdfImageCompression.Jbig2:
                    return PdfCompression.Jbig2;

                case PdfImageCompression.Jpeg:
                    return PdfCompression.Jpeg;

                case PdfImageCompression.Jpeg2000:
                    return PdfCompression.Jpeg2000;

                case PdfImageCompression.Lzw:
                    return PdfCompression.Lzw;

                case PdfImageCompression.None:
                    return PdfCompression.None;

                case PdfImageCompression.Zip:
                    return PdfCompression.Zip;

                case PdfImageCompression.Jpeg | PdfImageCompression.Zip:
                    return PdfCompression.Jpeg | PdfCompression.Zip;
            }
            return PdfCompression.Auto;
        }
#endif

        /// <summary>
        /// "PDF/A Compatible" checkbox state is changed.
        /// </summary>
        private void pdfaCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (pdfaCheckBox.IsChecked.Value == true)
                conformanceComboBox.IsEnabled = true;
            else
                conformanceComboBox.IsEnabled = false;
        }

        /// <summary>
        /// "Use front layer" checkbox state is changed.
        /// </summary>
        void mrcUseFrontCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.CreateFrontLayer = mrcUseFrontCheckBox.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// "High quality mask" checkbox state is changed.
        /// </summary>
        void mrcHiQualityMaskCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.HiQualityMask = mrcHiQualityMaskCheckBox.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// "High quality front layer" checkbox state is changed.
        /// </summary>
        void mrcHiQualityFrontLayerCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.HiQualityFrontLayer = mrcHiQualityFrontLayerCheckBox.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// "Use background layer" checkbox state is changed.
        /// </summary>
        void mrcUseBackgroundLayerCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.CreateBackgroundLayer = mrcUseBackgroundLayerCheckBox.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// "Detect images" checkbox state is changed.
        /// </summary>
        void mrcImageSegmentationCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (mrcImageSegmentationCheckBox.IsChecked.Value)
            {
#if !REMOVE_PDF_PLUGIN && !REMOVE_DOCCLEANUP_PLUGIN
                _mrcCompressionSettings.ImageSegmentation = new Vintasoft.Imaging.ImageProcessing.Info.ImageSegmentationCommand();
#endif
                mrcUseImagesLayerRadioButton.IsEnabled = true;
                mrcNotUseImagesLayerRadioButton.IsEnabled = true;
            }
            else
            {
#if !REMOVE_PDF_PLUGIN && !REMOVE_DOCCLEANUP_PLUGIN
                _mrcCompressionSettings.ImageSegmentation = null;
#endif
                mrcUseImagesLayerRadioButton.IsEnabled = false;
                mrcNotUseImagesLayerRadioButton.IsEnabled = false;
            }
            UpdateUI();
        }

        /// <summary>
        /// "Detect images -> background layer" radio button state is changed.
        /// </summary>
        void mrcNotUseImagesLayerRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.CreateImagesLayer = !mrcNotUseImagesLayerRadioButton.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// "Detect images -> images layer (each image as resource)" radio button state is changed.
        /// </summary>
        void mrcUseImagesLayerRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _mrcCompressionSettings.CreateImagesLayer = mrcUseImagesLayerRadioButton.IsChecked.Value;
#endif
            OnMrcCompressionChanged();
            UpdateUI();
        }

        /// <summary>
        /// MRC compression settings is changed.
        /// </summary>
        private void OnMrcCompressionChanged()
        {
            if (!_isMrcCompressionProfileInitializing)
                mrcCompressionProfileComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Sets the MRC compression settings from predefined profile.
        /// </summary>
        void mrcCompressionProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mrcCompressionProfileComboBox.SelectedIndex == 0)
                return;

#if !REMOVE_PDF_PLUGIN && !REMOVE_DOCCLEANUP_PLUGIN
            PdfMrcCompressionSettings settings = new PdfMrcCompressionSettings();

            _isMrcCompressionProfileInitializing = true;

            switch (mrcCompressionProfileComboBox.SelectedIndex)
            {
                // Document with images, best quality
                case 1:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 60;

                    settings.ImageSegmentation = new ImageSegmentationCommand();
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = true;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = true;
                    settings.FrontLayerCompression = PdfCompression.Jpeg2000;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionRatio = 300 * 3;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionType = Jpeg2000CompressionType.Lossy;
                    break;

                // Document with images, optimal
                case 2:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 35;

                    settings.ImageSegmentation = new ImageSegmentationCommand();
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = true;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = true;
                    settings.FrontLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.FrontLayerCompressionSettings.JpegQuality = 25;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionRatio = 400 * 3;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionType = Jpeg2000CompressionType.Lossy;
                    break;

                // Document with images, best compression
                case 3:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 20;

                    settings.ImageSegmentation = new ImageSegmentationCommand();
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = false;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = false;
                    settings.FrontLayerCompression = PdfCompression.Zip;
                    break;

                // Document, best quality
                case 4:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 30;

                    settings.ImageSegmentation = null;
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = true;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = true;
                    settings.FrontLayerCompression = PdfCompression.Jpeg2000;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionRatio = 300 * 3;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionType = Jpeg2000CompressionType.Lossy;
                    break;

                // Document, optimal
                case 5:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 25;


                    settings.ImageSegmentation = null;
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = true;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = false;
                    settings.FrontLayerCompression = PdfCompression.Jpeg2000;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionRatio = 350 * 3;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionType = Jpeg2000CompressionType.Lossy;
                    break;

                // Document, best compression
                case 6:
                    settings.CreateBackgroundLayer = true;
                    settings.BackgroundLayerCompression = PdfCompression.Jpeg | PdfCompression.Zip;
                    settings.BackgroundLayerCompressionSettings.JpegQuality = 20;


                    settings.ImageSegmentation = null;
                    settings.CreateImagesLayer = false;

                    settings.HiQualityMask = false;

                    settings.CreateFrontLayer = true;
                    settings.HiQualityFrontLayer = false;
                    settings.FrontLayerCompression = PdfCompression.Jpeg2000;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionRatio = 450 * 3;
                    settings.FrontLayerCompressionSettings.Jpeg2000Settings.CompressionType = Jpeg2000CompressionType.Lossy;
                    break;
            }
            MrcCompressionSettings = settings;
#endif
            UpdateUI();

            _isMrcCompressionProfileInitializing = false;
        }

        /// <summary>
        /// Shows the dialog for editing the image segmentation settings.
        /// </summary>
        private void mrcImageSegmentationSettingsButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN && !REMOVE_DOCCLEANUP_PLUGIN
            PropertyGridWindow form = new PropertyGridWindow(MrcCompressionSettings.ImageSegmentation, "Image segmentation settings");
            form.ShowDialog();
#endif
        }

        /// <summary>
        /// Updates User Interface.
        /// </summary>
        private void UpdateUI_Handler(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        #endregion

    }
}
