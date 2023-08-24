using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Codecs;
#if !REMOVE_PDF_PLUGIN
using Vintasoft.Imaging.Pdf;
#endif
using Vintasoft.Imaging.Codecs.Encoders;
using Vintasoft.Imaging.ImageProcessing;


namespace WpfDemosCommonCode.Imaging.Codecs.Dialogs
{
    /// <summary>
    /// Represents the control for editing the PDF compression settings.
    /// </summary>
    public partial class PdfCompressionControl : UserControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCompressionControl"/> class.
        /// </summary>
        public PdfCompressionControl()
        {
            InitializeComponent();

            compressionNoneRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionNoneRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpegRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpegRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpegZipRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpegZipRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpeg2000RadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJpeg2000RadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionCcittRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionCcittRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionAutoRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionAutoRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionZipRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionZipRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionLzwRadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionLzwRadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJbig2RadioButton.Checked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            compressionJbig2RadioButton.Unchecked += new RoutedEventHandler(compressionRadioButton_CheckedChanged);
            binarizationModeComboBox.SelectionChanged += new SelectionChangedEventHandler(binarizationModeComboBox_SelectionChanged);
            thresholdNumericUpDown.ValueChanged += new EventHandler<EventArgs>(thresholdNumericUpDown_ValueChanged);

            jpegQualityNumericUpDown.Minimum = 5;
            jpegQualityNumericUpDown.Maximum = 100;
            jpegQualityNumericUpDown.Value = 90;
            jpegQualityNumericUpDown.ValueChanged += new EventHandler<EventArgs>(jpegQualityNumericUpDown_ValueChanged);

            jpegGrayscaleCheckBox.Checked += new RoutedEventHandler(jpegGrayscaleCheckBox_CheckedChanged);
            jpegGrayscaleCheckBox.Unchecked += new RoutedEventHandler(jpegGrayscaleCheckBox_CheckedChanged);

            jbig2UseGlobalsCheckBox.Checked += new RoutedEventHandler(jbig2UseGlobalsCheckBox_CheckedChanged);
            jbig2UseGlobalsCheckBox.Unchecked += new RoutedEventHandler(jbig2UseGlobalsCheckBox_CheckedChanged);

            zipLevelNumericUpDown.Minimum = 1;
            zipLevelNumericUpDown.Maximum = 9;
            zipLevelNumericUpDown.Value = 6;
            zipLevelNumericUpDown.ValueChanged += new EventHandler<EventArgs>(zipLevelNumericUpDown_ValueChanged);

            if (!AvailableEncoders.IsEncoderAvailable("Jbig2"))
                compressionJbig2RadioButton.IsEnabled = false;
            if (!AvailableEncoders.IsEncoderAvailable("Jpeg2000"))
                compressionJpeg2000RadioButton.IsEnabled = false;

            foreach (BinarizationMode mode in Enum.GetValues(typeof(BinarizationMode)))
                binarizationModeComboBox.Items.Add(mode);
        }

        #endregion



        #region Properties

#if !REMOVE_PDF_PLUGIN
        PdfCompression _compression = PdfCompression.Auto;
        /// <summary>
        /// Gets or sets the PDF compression.
        /// </summary>
        [DefaultValue(PdfCompression.Auto)]
        public PdfCompression Compression
        {
            get
            {
                return _compression;
            }
            set
            {
                _compression = value;
                UpdateUI();
            }
        }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether the 'Auto' compression can be used.
        /// </summary>
        [DefaultValue(true)]
        public bool CanUseAutoCompression
        {
            get
            {
                return compressionAutoRadioButton.Visibility == Visibility.Visible;
            }
            set
            {
                compressionAutoRadioButton.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

#if !REMOVE_PDF_PLUGIN
        PdfCompressionSettings _compressionSettings = null;
        /// <summary>
        /// Gets or sets the PDF compression settings.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue((object)null)]
        public PdfCompressionSettings CompressionSettings
        {
            get
            {
                return _compressionSettings;
            }
            set
            {
                _compressionSettings = value;
                UpdateUI();
            }
        }
#endif

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            jpeg2000CompressionSettingsGroupBox.Visibility = Visibility.Hidden;
            jpegCompressionSettingsGroupBox.Visibility = Visibility.Hidden;
            jbig2CompressionSettingsGroupBox.Visibility = Visibility.Hidden;
            zipCompressionSettingsGroupBox.Visibility = Visibility.Hidden;
            binarizationGroupBox.Visibility = Visibility.Hidden;
#if !REMOVE_PDF_PLUGIN
            if (_compressionSettings != null)
            {
                switch (_compression)
                {
                    case PdfCompression.Auto:
                        zipCompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.Jpeg:
                        jpegCompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.Jpeg2000:
                        jpeg2000CompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.Jbig2:
                        jbig2CompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.CcittFax:
                        binarizationGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.Zip:
                        zipCompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                    case PdfCompression.Zip | PdfCompression.Jpeg:
                        zipCompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        jpegCompressionSettingsGroupBox.Visibility = Visibility.Visible;
                        break;
                }
                jpegQualityNumericUpDown.Value = _compressionSettings.JpegQuality;
                jpegGrayscaleCheckBox.IsChecked = _compressionSettings.JpegSaveAsGrayscale;
                jbig2UseGlobalsCheckBox.IsChecked = _compressionSettings.Jbig2UseGlobals;
                zipLevelNumericUpDown.Value = _compressionSettings.ZipCompressionLevel;
                binarizationModeComboBox.SelectedItem = _compressionSettings.BinarizationCommand.BinarizationMode;
                thresholdNumericUpDown.Value = _compressionSettings.BinarizationCommand.Threshold;
            }
            if (_compression == PdfCompression.Auto)
                compressionAutoRadioButton.IsChecked = true;
            else if (_compression == PdfCompression.CcittFax)
                compressionCcittRadioButton.IsChecked = true;
            else if (_compression == PdfCompression.Jbig2)
                compressionJbig2RadioButton.IsChecked = true;
            else if (_compression == PdfCompression.Jpeg2000)
                compressionJpeg2000RadioButton.IsChecked = true;
            else if (_compression == PdfCompression.Jpeg)
                compressionJpegRadioButton.IsChecked = true;
            else if (_compression == PdfCompression.Lzw)
                compressionLzwRadioButton.IsChecked = true;
            else if (_compression == PdfCompression.None)
                compressionNoneRadioButton.IsChecked = true;
            else if (_compression == PdfCompression.Zip)
                compressionZipRadioButton.IsChecked = true;
            else if (_compression == (PdfCompression.Zip | PdfCompression.Jpeg) ||
                _compression == (PdfCompression.Zip | PdfCompression.Jpeg | PdfCompression.Predictor))
                compressionJpegZipRadioButton.IsChecked = true;
#endif
        }

        /// <summary>
        /// Compression radio button is checked.
        /// </summary>
        private void compressionRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            if (compressionAutoRadioButton.IsChecked.Value)
                _compression = PdfCompression.Auto;
            else if (compressionCcittRadioButton.IsChecked.Value)
                _compression = PdfCompression.CcittFax;
            else if (compressionJbig2RadioButton.IsChecked.Value)
                _compression = PdfCompression.Jbig2;
            else if (compressionJpeg2000RadioButton.IsChecked.Value)
                _compression = PdfCompression.Jpeg2000;
            else if (compressionJpegRadioButton.IsChecked.Value)
                _compression = PdfCompression.Jpeg;
            else if (compressionLzwRadioButton.IsChecked.Value)
                _compression = PdfCompression.Lzw;
            else if (compressionNoneRadioButton.IsChecked.Value)
                _compression = PdfCompression.None;
            else if (compressionZipRadioButton.IsChecked.Value)
                _compression = PdfCompression.Zip;
            else if (compressionJpegZipRadioButton.IsChecked.Value)
                _compression = PdfCompression.Jpeg | PdfCompression.Zip;
#endif
            UpdateUI();
        }

        /// <summary>
        /// JPEG quality is changed.
        /// </summary>
        private void jpegQualityNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.JpegQuality = (int)jpegQualityNumericUpDown.Value;
#endif
        }

        /// <summary>
        /// JPEG grayscale flag is changed.
        /// </summary>
        private void jpegGrayscaleCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.JpegSaveAsGrayscale = jpegGrayscaleCheckBox.IsChecked.Value;
#endif
        }

        /// <summary>
        /// JBIG2 UseGlobals flag is changed.
        /// </summary>
        private void jbig2UseGlobalsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.Jbig2UseGlobals = jbig2UseGlobalsCheckBox.IsChecked.Value;
#endif
        }

        /// <summary>
        /// Zip compression level is changed.
        /// </summary>
        private void zipLevelNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.ZipCompressionLevel = (int)zipLevelNumericUpDown.Value;
#endif
        }

        /// <summary>
        /// Shows the JPEG2000 settings dialog.
        /// </summary>
        private void jpeg2000SettingsButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            Jpeg2000EncoderSettingsWindow jpeg2000SettingsDialog = new Jpeg2000EncoderSettingsWindow();
            jpeg2000SettingsDialog.EncoderSettings = _compressionSettings.Jpeg2000Settings;
            jpeg2000SettingsDialog.ShowDialog();
#endif
        }

        /// <summary>
        /// Shows the JBIG2 settings dialog.
        /// </summary>
        private void jbig2SettingsButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            Jbig2EncoderSettingsWindow jbig2SettingsDialog = new Jbig2EncoderSettingsWindow();
            jbig2SettingsDialog.EncoderSettings = _compressionSettings.Jbig2Settings;
            jbig2SettingsDialog.AppendExistingDocumentEnabled = false;
            jbig2SettingsDialog.ShowDialog();
#endif
        }

        /// <summary>
        /// Binarization mode of image is changed.
        /// </summary>
        void binarizationModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BinarizationMode mode = (BinarizationMode)binarizationModeComboBox.SelectedItem;
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.BinarizationCommand.BinarizationMode = mode;
#endif
            if (mode == BinarizationMode.Threshold)
            {
                thresholdNumericUpDown.Visibility = Visibility.Visible;
                thresholdLabel.Visibility = Visibility.Visible;
            }
            else
            {
                thresholdNumericUpDown.Visibility = Visibility.Hidden;
                thresholdLabel.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Threshold of binarization is changed.
        /// </summary>
        void thresholdNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
#if !REMOVE_PDF_PLUGIN
            _compressionSettings.BinarizationCommand.Threshold = (int)thresholdNumericUpDown.Value;
#endif
        }

        #endregion

    }
}
