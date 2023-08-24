using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Codecs;
using Vintasoft.Imaging.Codecs.Encoders;


namespace WpfDemosCommonCode.Imaging.Codecs.Dialogs
{
    /// <summary>
    /// A form that allows to view and edit the JBIG2 encoder settings.
    /// </summary>
    public partial class Jbig2EncoderSettingsWindow : Window
    {

        #region Constructors

        public Jbig2EncoderSettingsWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Determines that existing document should be append.
        /// </summary>
        public bool AppendExistingDocument
        {
            get
            {
                return appendCheckBox.IsChecked.Value == true;
            }
            set
            {
                appendCheckBox.IsChecked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="AppendExistingDocument"/> is enabled.
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

        Jbig2EncoderSettings _encoderSettings;
        /// <summary>
        /// Gets or sets JBIG 2 encoder settings.
        /// </summary>
        public Jbig2EncoderSettings EncoderSettings
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
                    UpdateUI();
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Loaded event of Window object.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (EncoderSettings == null)
                EncoderSettings = new Jbig2EncoderSettings();
        }

        /// <summary>
        /// Handles the Click event of UseLossyCheckBox object.
        /// </summary>
        private void useLossyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            lossyGroupBox.IsEnabled = useLossyCheckBox.IsChecked.Value == true;
        }

        /// <summary>
        /// Handles the Click event of UseSymbolDictionaryCheckBox object.
        /// </summary>
        private void useSymbolDictionaryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            symbolDictionaryGroupBox.IsEnabled = useSymbolDictionaryCheckBox.IsChecked.Value == true;
        }

        private void UpdateUI()
        {
            if (EncoderSettings.UseMmr)
                mmrRadioButton.IsChecked = true;
            else
                arithmeticRadioButton.IsChecked = true;
            useLossyCheckBox.IsChecked = EncoderSettings.Lossy;
            useLossyCheckBox_Click(useLossyCheckBox, null);
            inaccuracyPercentNumericUpDown.Value = EncoderSettings.Inaccuracy;
            useSymbolDictionaryCheckBox.IsChecked = EncoderSettings.UseSymbolDictionary;
            useSymbolDictionaryCheckBox_Click(useSymbolDictionaryCheckBox, null);
            string symbolDictionarySize = EncoderSettings.SymbolDictionarySize.ToString();

            bool contains = false;
            foreach (ComboBoxItem item in sdSizeComboBox.Items)
                if (item.Content.ToString() == symbolDictionarySize)
                {
                    contains = true;
                    break;
                }
            if (!contains)
                sdSizeComboBox.Items.Add(symbolDictionarySize);
            sdSizeComboBox.SelectedItem = symbolDictionarySize;
            lossyGroupBox.IsEnabled = useLossyCheckBox.IsChecked.Value == true;
            symbolDictionaryGroupBox.IsEnabled = useSymbolDictionaryCheckBox.IsChecked.Value == true;
        }

        private void SetEncoderSettings()
        {
            EncoderSettings.UseMmr = mmrRadioButton.IsChecked.Value == true;
            EncoderSettings.Lossy = useLossyCheckBox.IsChecked.Value == true;
            EncoderSettings.Inaccuracy = (int)inaccuracyPercentNumericUpDown.Value;
            EncoderSettings.UseSymbolDictionary = useSymbolDictionaryCheckBox.IsChecked.Value == true;
            EncoderSettings.SymbolDictionarySize = Int32.Parse(sdSizeComboBox.Text);
        }

        /// <summary>
        /// Handles the Click event of ButtonOk object.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            SetEncoderSettings();

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of ButtonCancel object.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
