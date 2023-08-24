#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OfficeDocumentTextPropertiesVisualEditorToolBar.xaml
    /// </summary>
    public partial class OfficeDocumentTextPropertiesVisualEditorToolBar : ToolBar
    {

        #region Fields

        /// <summary>
        /// A value indicating whether UI is updating.
        /// </summary>
        bool _isUiUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficeDocumentTextPropertiesVisualEditorToolBar"/> class.
        /// </summary>
        public OfficeDocumentTextPropertiesVisualEditorToolBar()
        {
            InitializeComponent();

            OnEditingDisabled();
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
                        _visualEditor.EditingEnabled -= VisualEditor_EditingEnabled;
                        _visualEditor.EditingDisabled -= VisualEditor_EditingDisabled;
                        _visualEditor.DocumentChanged -= VisualEditor_DocumentChanged;
                    }

                    _visualEditor = value;

                    if (_visualEditor != null)
                    {
                        _visualEditor.EditingEnabled += VisualEditor_EditingEnabled;
                        _visualEditor.EditingDisabled += VisualEditor_EditingDisabled;
                        _visualEditor.DocumentChanged += VisualEditor_DocumentChanged;
                    }
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of IncreaseTextSizeButton object.
        /// </summary>
        private void increaseTextSizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeTextSize(1).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of DecreaseTextSizeButton object.
        /// </summary>
        private void decreaseTextSizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeTextSize(-1).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }


        /// <summary>
        /// Handles the Click event of ChangeBoldTextButton object.
        /// </summary>
        private void changeBoldTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.ChangeFocusedTextBold.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of ChangeItalicTextButton object.
        /// </summary>
        private void changeItalicTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.ChangeFocusedTextItalic.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of ChangeUnderlineTextButton object.
        /// </summary>
        private void changeUnderlineTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeTextUnderlineUIAction(OpenXmlTextUnderlineType.Single).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of StrikeoutTextButton object.
        /// </summary>
        private void strikeoutTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.ChangeFocusedTextStrikeout.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of SubscriptTextButton object.
        /// </summary>
        private void subscriptTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.ChangeFocusedTextSubscript.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of SuperscriptTextButton object.
        /// </summary>
        private void superscriptTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.ChangeFocusedTextSuperscript.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of TextPropertiesButton object.
        /// </summary>
        private void textPropertiesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // if visual editor has focused text 
                OpenXmlTextProperties textProperties = VisualEditor.GetTextProperties();
                if (textProperties != null)
                {
                    // show dialog that allows to view and edit text properties
                    OpenXmlTextPropertiesWindow textPropertiesForm = new OpenXmlTextPropertiesWindow(VisualEditor.GetFontNames());
                    textPropertiesForm.Owner = Window.GetWindow(this);
                    textPropertiesForm.TextProperties = textProperties;
                    if (textPropertiesForm.ShowDialog() == true)
                    {
                        // set text properties
                        textProperties = textPropertiesForm.GetChangedTextProperties();
                        if (textProperties != null)
                            VisualEditor.SetTextProperties(textProperties);
                    }
                    // set focus to the parent window
                    Window.GetWindow(this).Owner.Focus();
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Returns the visual tool, which uses the Office document visual editor.
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
        /// Handles the EditingDisabled event of VisualEditor object.
        /// </summary>
        private void VisualEditor_EditingDisabled(object sender, EventArgs e)
        {
            OnEditingDisabled();
        }

        /// <summary>
        /// Handles the EditingEnabled event of VisualEditor object.
        /// </summary>
        private void VisualEditor_EditingEnabled(object sender, EventArgs e)
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
            changeBoldButton.IsChecked = properties.IsBold ?? false;
            changeItalicButton.IsChecked = properties.IsItalic ?? false;
            changeUnderlineButton.IsChecked = properties.IsUnderline ?? false;
            changeStrikeoutButton.IsChecked = properties.IsStrike ?? false;
            if (properties.VerticalAlignment.HasValue)
            {
                changeSubscriptButton.IsChecked = properties.VerticalAlignment.Value == OpenXmlTextVerticalPositionType.Subscript;
                changeSuperscriptButton.IsChecked = properties.VerticalAlignment.Value == OpenXmlTextVerticalPositionType.Superscript;
            }
            else
            {
                changeSubscriptButton.IsChecked = false;
                changeSuperscriptButton.IsChecked = false;
            }
        }

        #endregion

    }
}
