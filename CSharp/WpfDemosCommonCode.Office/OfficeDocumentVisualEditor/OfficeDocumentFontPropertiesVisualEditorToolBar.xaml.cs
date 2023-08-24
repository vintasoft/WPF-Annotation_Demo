#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OfficeDocumentFontPropertiesVisualEditorToolBar.xaml
    /// </summary>
    public partial class OfficeDocumentFontPropertiesVisualEditorToolBar : ToolBar
    {

        #region Fields

        /// <summary>
        /// A value indicating whether UI is updating.
        /// </summary>
        bool _isUiUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficeDocumentFontPropertiesVisualEditorToolBar"/> class.
        /// </summary>
        public OfficeDocumentFontPropertiesVisualEditorToolBar()
        {
            InitializeComponent();
            fontSizeComboBox.Items.Add(8);
            fontSizeComboBox.Items.Add(9);
            fontSizeComboBox.Items.Add(10);
            fontSizeComboBox.Items.Add(11);
            fontSizeComboBox.Items.Add(12);
            fontSizeComboBox.Items.Add(14);
            fontSizeComboBox.Items.Add(16);
            fontSizeComboBox.Items.Add(18);
            fontSizeComboBox.Items.Add(20);
            fontSizeComboBox.Items.Add(22);
            fontSizeComboBox.Items.Add(24);
            fontSizeComboBox.Items.Add(28);
            fontSizeComboBox.Items.Add(36);
            fontSizeComboBox.Items.Add(48);
            fontSizeComboBox.Items.Add(72);
            fontNameComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(fontNameComboBox_TextChanged));
            fontSizeComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(fontSizeComboBox_TextChanged));
        }

        #endregion



        #region Properties

        WpfOfficeDocumentVisualEditor _visualEditor;
        /// <summary>
        /// Gets or sets the visual editor for Office document.
        /// </summary>
        private WpfOfficeDocumentVisualEditor VisualEditor
        {
            get
            {
                return _visualEditor;
            }
            set
            {
                if (_visualEditor != value)
                {
                    if (_visualEditor != null)
                    {
                        _visualEditor.EditingEnabled -= VisualEditor_EditEnabled;
                        _visualEditor.EditingDisabled -= VisualEditor_EditDisabled;
                        _visualEditor.DocumentChanged -= VisualEditor_DocumentChanged;
                    }

                    _visualEditor = value;

                    if (_visualEditor != null)
                    {
                        _visualEditor.EditingEnabled += VisualEditor_EditEnabled;
                        _visualEditor.EditingDisabled += VisualEditor_EditDisabled;
                        _visualEditor.DocumentChanged += VisualEditor_DocumentChanged;
                    }
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Returns the visual tool that uses the Office document visual editor.
        /// </summary>
        public void AddVisualTool(WpfUserInteractionVisualTool visualTool)
        {
            visualTool.ActiveInteractionControllerChanged += VisualTool_ActiveInteractionControllerChanged;
        }

        /// <summary>
        /// Handles the FocusedTextSymbolChanged event of TextTool object.
        /// </summary>
        private void TextTool_FocusedTextSymbolChanged(object sender, Vintasoft.Imaging.PropertyChangedEventArgs<Vintasoft.Imaging.Text.TextRegionSymbol> e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the ActiveInteractionControllerChanged event of VisualTool object.
        /// </summary>
        private void VisualTool_ActiveInteractionControllerChanged(object sender, Vintasoft.Imaging.PropertyChangedEventArgs<IWpfInteractionController> e)
        {
            WpfOfficeDocumentVisualEditor visualEditor = null;
            if (e.NewValue != null)
                visualEditor = WpfCompositeInteractionController.FindInteractionController<WpfOfficeDocumentVisualEditor>(e.NewValue);
            VisualEditor = visualEditor;
        }

        /// <summary>
        /// Handles the EditDisabled event of VisualEditor object.
        /// </summary>
        private void VisualEditor_EditDisabled(object sender, EventArgs e)
        {
            OnEditingDisabled();
        }

        /// <summary>
        /// Handles the EditEnabled event of VisualEditor object.
        /// </summary>
        private void VisualEditor_EditEnabled(object sender, EventArgs e)
        {
            OnEditingEnabled();
        }

        /// <summary>
        /// Handles the DocumentChanged event of VisualEditor object.
        /// </summary>
        private void VisualEditor_DocumentChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }


        /// <summary>
        /// Called when editing is enabled.
        /// </summary>
        private void OnEditingEnabled()
        {
            fontNameComboBox.Items.Clear();
            foreach (string fontName in VisualEditor.GetFontNames())
                fontNameComboBox.Items.Add(fontName);

            VisualEditor.TextTool.FocusedTextSymbolChanged += TextTool_FocusedTextSymbolChanged;
            UpdateUI();
            IsEnabled = true;
        }

        /// <summary>
        /// Called when editing is disabled.
        /// </summary>
        private void OnEditingDisabled()
        {
            if (VisualEditor != null)
            {
                VisualEditor.TextTool.FocusedTextSymbolChanged -= TextTool_FocusedTextSymbolChanged;
            }
            IsEnabled = false;
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        private void UpdateUI()
        {
            _isUiUpdating = true;

            TextRegionSymbol symbol = VisualEditor.FocusedTextSymbol;

            OpenXmlTextProperties textProperties = null;
            if (symbol != null)
                textProperties = VisualEditor.GetTextProperties(symbol);
            UpdateTextPropertiesUI(textProperties);

            _isUiUpdating = false;
        }

        /// <summary>
        /// Updates UI for specified text properties.
        /// </summary>
        /// <param name="textProperties">The text properties.</param>
        private void UpdateTextPropertiesUI(OpenXmlTextProperties textProperties)
        {
            OpenXmlTextProperties properties = textProperties ?? new OpenXmlTextProperties();
            if (properties.FontName != null)
            {
                SetFontName(properties.FontName);
            }
            else
            {
                fontNameComboBox.Text = "";
            }
            fontSizeComboBox.Text = properties.FontSize.HasValue ? properties.FontSize.ToString() : "";
        }


        /// <summary>
        /// Sets the name of the font.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        private void SetFontName(string fontName)
        {
            for (int i = 0; i < fontNameComboBox.Items.Count; i++)
            {
                // if fontNameComboBox contains fontName
                if ((string)fontNameComboBox.Items[i] == fontName)
                {
                    // select font name in fontNameComboBox
                    fontNameComboBox.SelectedIndex = i;
                    return;
                }
            }

            // add font name to current fonts list
            string[] names = new string[fontNameComboBox.Items.Count + 1];
            names[0] = fontName;
            for (int i = 0; i < fontNameComboBox.Items.Count; i++)
            {
                names[i + 1] = (string)fontNameComboBox.Items[i];
            }

            // sort font names
            Array.Sort(names);

            // update fontNameComboBox
            fontNameComboBox.Items.Clear();
            foreach (string name in names)
                fontNameComboBox.Items.Add(name);
            fontNameComboBox.Text = fontName;
        }

        /// <summary>
        /// Handles the TextChanged event of FontNameComboBox object.
        /// </summary>
        private void fontNameComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUiUpdating)
            {
                // create text properties
                OpenXmlTextProperties textProperties = new OpenXmlTextProperties();
                textProperties.FontName = fontNameComboBox.Text;

                // set text properties for text
                VisualEditor.Actions.CreateSetTextProperties(textProperties).Execute();
            }
        }

        /// <summary>
        /// Handles the TextChanged event of FontSizeComboBox object.
        /// </summary>
        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUiUpdating)
            {
                try
                {
                    float value;
                    if (float.TryParse(fontSizeComboBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        // create text properties
                        OpenXmlTextProperties textProperties = new OpenXmlTextProperties();
                        textProperties.FontSize = value;

                        // set text properties for text
                        VisualEditor.Actions.CreateSetTextProperties(textProperties).Execute();
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        #endregion

    }
}
