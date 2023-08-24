#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System.ComponentModel;
using System.IO;
using System.Windows;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Editor.Docx;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for SetOpenXmlParagraphNumerationWindow.xaml
    /// </summary>
    public partial class SetOpenXmlParagraphNumerationWindow : Window
    {

        #region Fields

        /// <summary>
        /// The visual editor for Office document.
        /// </summary>
        WpfOfficeDocumentVisualEditor _documentVisualEditor;

        /// <summary>
        /// The DOCX document editor.
        /// </summary>
        DocxDocumentEditor _documentEditor;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetOpenXmlParagraphNumerationWindow"/> class.
        /// </summary>
        public SetOpenXmlParagraphNumerationWindow()
        {
            InitializeComponent();

            externalNumerationsComboBox.Items.Add("SimpleList");
            externalNumerationsComboBox.Items.Add("NumberedList");
            externalNumerationsComboBox.Items.Add("CheckList");
            externalNumerationsComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetOpenXmlParagraphNumerationWindow"/> class.
        /// </summary>
        /// <param name="documentVisualEditor">The Office document visual editor.</param>
        public SetOpenXmlParagraphNumerationWindow(WpfOfficeDocumentVisualEditor documentVisualEditor)
            : this()
        {
            _documentVisualEditor = documentVisualEditor;
            _documentEditor = documentVisualEditor.CreateDocumentEditor();
            UpdateUI();
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of RestartButton object.
        /// </summary>
        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            if (numerationDefinitionsListBox.SelectedItem != null)
            {
                DocxDocumentNumbering numbering = (DocxDocumentNumbering)numerationDefinitionsListBox.SelectedItem;
                numbering = _documentEditor.Numbering.CreateCopy(numbering);

                numerationDefinitionsListBox.Items.Add(numbering);
                numerationDefinitionsListBox.SelectedItem = numbering;
            }
        }

        /// <summary>
        /// Handles the Click event of ImportButton object.
        /// </summary>
        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            using (Stream numberingTemplateStream = DemosResourcesManager.GetResourceAsStream(externalNumerationsComboBox.Text + ".docx"))
            {
                using (DocxDocumentEditor numberingTemplateDocumentEditor = new DocxDocumentEditor(numberingTemplateStream))
                {
                    DocxDocumentNumbering importedNumbering = _documentEditor.Numbering.Import(
                        numberingTemplateDocumentEditor,
                        numberingTemplateDocumentEditor.Numbering.Items[0]);

                    numerationDefinitionsListBox.Items.Add(importedNumbering);
                    numerationDefinitionsListBox.SelectedItem = importedNumbering;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (numerationDefinitionsListBox.SelectedItem != null)
            {
                OpenXmlDocumentNumbering numbering = (OpenXmlDocumentNumbering)numerationDefinitionsListBox.SelectedItem;
                if (_documentVisualEditor.SetParagraphNumeration(_documentEditor, numbering, 0))
                {
                    _documentEditor.Dispose();
                    _documentEditor = null;
                    _documentVisualEditor.OnDocumentChanged();
                }
            }
            DialogResult = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of NumerationDefinitionsListBox object.
        /// </summary>
        private void numerationDefinitionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            okButton.IsEnabled = numerationDefinitionsListBox.SelectedItem != null;
            restartButton.IsEnabled = numerationDefinitionsListBox.SelectedItem != null;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            numerationDefinitionsListBox.Items.Clear();
            if (_documentEditor.Numbering.Items != null)
            {
                foreach (DocxDocumentNumbering numbering in _documentEditor.Numbering.Items)
                {
                    numerationDefinitionsListBox.Items.Add(numbering);
                }
                OpenXmlParagraphProperties paragraphProperties = _documentVisualEditor.GetParagraphProperties();
                if (paragraphProperties.Numeration != null)
                {
                    numerationDefinitionsListBox.SelectedItem = paragraphProperties.Numeration;
                }
            }
        }

        /// <summary>
        /// Called when window is closing.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_documentEditor != null)
            {
                _documentEditor.Dispose();
                _documentEditor = null;
            }
        }

        #endregion

    }
}
