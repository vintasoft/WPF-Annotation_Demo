#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System;
using System.Windows;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OpenXmlParagraphPropertiesWindow.xaml
    /// </summary>
    public partial class OpenXmlParagraphPropertiesWindow : Window
    {

        #region Fields

        /// <summary>
        /// The initial paragraph properties.
        /// </summary>
        OpenXmlParagraphProperties _intalParagraphProperties;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenXmlParagraphPropertiesWindow"/> class.
        /// </summary>
        public OpenXmlParagraphPropertiesWindow()
        {
            InitializeComponent();

            textJustificationComboBox.Items.Add(OpenXmlParagraphJustification.Left);
            textJustificationComboBox.Items.Add(OpenXmlParagraphJustification.Center);
            textJustificationComboBox.Items.Add(OpenXmlParagraphJustification.Right);
            textJustificationComboBox.Items.Add(OpenXmlParagraphJustification.Both);
        }

        #endregion



        #region Properties

        OpenXmlParagraphProperties _paragraphProperties;
        /// <summary>
        /// Gets or sets the paragraph properties.
        /// </summary>
        public OpenXmlParagraphProperties ParagraphProperties
        {
            get
            {
                return _paragraphProperties;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _paragraphProperties = value;
                _intalParagraphProperties = value.Clone();

                UpdateUI();
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Returns the paragraph properties, which contain changed properties.
        /// </summary>
        public OpenXmlParagraphProperties GetChangedParagraphProperties()
        {
            OpenXmlParagraphProperties result = OpenXmlParagraphProperties.GetChanges(_intalParagraphProperties, _paragraphProperties);
            if (result.IsEmpty)
                return null;
            return result;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateParagraphProperties())
                DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Updates the User Interface from <see cref="ParagraphProperties"/>.
        /// </summary>
        private void UpdateUI()
        {
            textJustificationComboBox.SelectedItem = _paragraphProperties.Justification;

            fillColorPanel.Color = WpfObjectConverter.CreateWindowsColor(_paragraphProperties.FillColor.Value);

            firstLineIndentationComboBox.Text = DemosTools.ToString(_paragraphProperties.FirstLineIndentation);

            leftIndentationComboBox.Text = DemosTools.ToString(_paragraphProperties.LeftIndentation);

            rightIndentationComboBox.Text = DemosTools.ToString(_paragraphProperties.RightIndentation);

            lineHeightComboBox.Text = DemosTools.ToString(_paragraphProperties.LineHeightFactor);

            spacingBeforeComboBox.Text = DemosTools.ToString(_paragraphProperties.SpacingBeforeParagraph);

            spacingAfterComboBox.Text = DemosTools.ToString(_paragraphProperties.SpacingAfterParagraph);

            keepLinesCheckBox.IsChecked = _paragraphProperties.KeepLines.Value;

            keepNextCheckBox.IsChecked = _paragraphProperties.KeepNext.Value;

            pageBreakBeforeCheckBox.IsChecked = _paragraphProperties.PageBreakBefore.Value;

            widowControlCheckBox.IsChecked = _paragraphProperties.WidowControl.Value;
        }

        /// <summary>
        /// Updates the <see cref="ParagraphProperties"/> from User Interface.
        /// </summary>
        /// <returns></returns>
        private bool UpdateParagraphProperties()
        {
            _paragraphProperties.Justification = (OpenXmlParagraphJustification)textJustificationComboBox.SelectedItem;

            _paragraphProperties.FillColor = WpfObjectConverter.CreateDrawingColor(fillColorPanel.Color);

            float value;

            if (!DemosTools.ParseFloat(firstLineIndentationComboBox.Text, "First Line Indentation", out value))
                return false;
            _paragraphProperties.FirstLineIndentation = value;

            if (!DemosTools.ParseFloat(leftIndentationComboBox.Text, "Left Indentation", out value))
                return false;
            _paragraphProperties.LeftIndentation = value;

            if (!DemosTools.ParseFloat(rightIndentationComboBox.Text, "Right Indentation", out value))
                return false;
            _paragraphProperties.RightIndentation = value;

            if (!DemosTools.ParseFloat(lineHeightComboBox.Text, "Line Height Factor", out value))
                return false;
            _paragraphProperties.LineHeightFactor = value;

            if (!DemosTools.ParseFloat(spacingBeforeComboBox.Text, "Spacing Before Paragraph", out value))
                return false;
            _paragraphProperties.SpacingBeforeParagraph = value;

            if (!DemosTools.ParseFloat(spacingAfterComboBox.Text, "Spacing After Paragraph", out value))
                return false;
            _paragraphProperties.SpacingAfterParagraph = value;

            _paragraphProperties.KeepLines = keepLinesCheckBox.IsChecked.Value;

            _paragraphProperties.KeepNext = keepNextCheckBox.IsChecked.Value;

            _paragraphProperties.PageBreakBefore = pageBreakBeforeCheckBox.IsChecked.Value;

            _paragraphProperties.WidowControl = widowControlCheckBox.IsChecked.Value;

            return true;
        }

        #endregion

        #endregion

    }
}
