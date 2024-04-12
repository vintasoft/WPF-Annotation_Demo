#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentParagraphPropertiesVisualEditorToolBar from the project.
#endif

using System;
using System.Windows;
using System.Windows.Controls;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Utils;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OfficeDocumentParagraphPropertiesVisualEditorToolBar.xaml
    /// </summary>
    public partial class OfficeDocumentParagraphPropertiesVisualEditorToolBar : ToolBar
    {

        #region Constants

        /// <summary>
        /// The paragraph indentation delta.
        /// </summary>
        static readonly float ParagraphIndentationDelta = (float)UnitOfMeasureConverter.ConvertToPoints(1, UnitOfMeasure.Centimeters);

        /// <summary>
        /// The paragraph first line indentation delta.
        /// </summary>
        static readonly float ParagraphFirstLineIndentationDelta = (float)UnitOfMeasureConverter.ConvertToPoints(0.75, UnitOfMeasure.Centimeters);

        #endregion



        #region Fields

        /// <summary>
        /// A value indicating whether UI is updating.
        /// </summary>
        bool _isUiUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficeDocumentParagraphPropertiesVisualEditorToolBar"/> class.
        /// </summary>
        public OfficeDocumentParagraphPropertiesVisualEditorToolBar()
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
        /// Handles the Click event of paragraphJLeftButton object.
        /// </summary>
        private void paragraphJLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateSetParagraphJustification(OpenXmlParagraphJustification.Left).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of paragraphJCenterButton object.
        /// </summary>
        private void paragraphJCenterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateSetParagraphJustification(OpenXmlParagraphJustification.Center).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of paragraphJRightButton object.
        /// </summary>
        private void paragraphJRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateSetParagraphJustification(OpenXmlParagraphJustification.Right).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of paragraphJBothButton object.
        /// </summary>
        private void paragraphJBothButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateSetParagraphJustification(OpenXmlParagraphJustification.Both).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of decParagraphLeftIndentationButton object.
        /// </summary>
        private void decParagraphLeftIndentationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeParagraphLeftIndentation(-ParagraphIndentationDelta).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of incParagraphLeftIndentationButton object.
        /// </summary>
        private void incParagraphLeftIndentationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeParagraphLeftIndentation(ParagraphIndentationDelta).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of decParagraphFirstLineIndentationButton object.
        /// </summary>
        private void decParagraphFirstLineIndentationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeParagraphFirstLineIndentation(-ParagraphFirstLineIndentationDelta).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }


        /// <summary>
        /// Handles the Click event of incParagraphFirstLineIndentationButton object.
        /// </summary>
        private void incParagraphFirstLineIndentationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                VisualEditor.Actions.CreateChangeParagraphFirstLineIndentation(ParagraphFirstLineIndentationDelta).Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }


        /// <summary>
        /// Handles the Click event of paragraphNumberingButton object.
        /// </summary>
        private void paragraphNumberingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // if focused paragraph has numertion
                if (paragraphNumberingButton.IsChecked)
                {
                    // remove numeration
                    OpenXmlParagraphProperties paragraphProperties = new OpenXmlParagraphProperties();
                    paragraphProperties.RemoveNumeration();
                    VisualEditor.SetParagraphProperties(paragraphProperties);
                }
                else
                {
                    // if visual editor has focused paragraph
                    if (VisualEditor.GetParagraphProperties() != null)
                    {
                        // set numeration properties
                        SetOpenXmlParagraphNumerationWindow form = new SetOpenXmlParagraphNumerationWindow(VisualEditor);
                        form.Owner = Window.GetWindow(this);
                        form.ShowDialog();
                        // set focus to parent form
                        Window.GetWindow(this).Owner.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of paragraphPropertiesButton object.
        /// </summary>
        private void paragraphPropertiesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // if visual editor has focused paragraph
                OpenXmlParagraphProperties paragraphProperties = VisualEditor.GetParagraphProperties();
                if (paragraphProperties != null)
                {
                    // show form that allows to edit paragraph properties
                    OpenXmlParagraphPropertiesWindow paragraphPropertiesWindow = new OpenXmlParagraphPropertiesWindow();
                    paragraphPropertiesWindow.Owner = Window.GetWindow(this);
                    paragraphPropertiesWindow.ParagraphProperties = paragraphProperties;
                    if (paragraphPropertiesWindow.ShowDialog() == true)
                    {
                        // set paragraph properties
                        paragraphProperties = paragraphPropertiesWindow.GetChangedParagraphProperties();
                        if (paragraphProperties != null)
                            VisualEditor.SetParagraphProperties(paragraphProperties);
                    }
                    // set focus to parent form
                    Window.GetWindow(this).Owner.Focus();
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

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

            OpenXmlParagraphProperties paragraphProperties = null;
            if (symbol != null)
                paragraphProperties = VisualEditor.GetParagraphProperties(symbol);
            UpdateParagraphPropertiesUI(paragraphProperties);

            _isUiUpdating = false;
        }

        /// <summary>
        /// Updates UI for specified text properties.
        /// </summary>
        /// <param name="ParagraphProperties">The text properties.</param>
        private void UpdateParagraphPropertiesUI(OpenXmlParagraphProperties paragraphProperties)
        {
            if (paragraphProperties == null)
                paragraphProperties = new OpenXmlParagraphProperties();
            if (paragraphProperties.Justification.HasValue)
            {
                paragraphJBothButton.IsChecked = paragraphProperties.Justification.Value == OpenXmlParagraphJustification.Both;
                paragraphJLeftButton.IsChecked = paragraphProperties.Justification.Value == OpenXmlParagraphJustification.Left;
                paragraphJRightButton.IsChecked = paragraphProperties.Justification.Value == OpenXmlParagraphJustification.Right;
                paragraphJCenterButton.IsChecked = paragraphProperties.Justification.Value == OpenXmlParagraphJustification.Center;
            }
            else
            {
                paragraphJBothButton.IsChecked = false;
                paragraphJLeftButton.IsChecked = false;
                paragraphJRightButton.IsChecked = false;
                paragraphJCenterButton.IsChecked = false;
            }
            paragraphNumberingButton.IsChecked = paragraphProperties.Numeration != null;
        }

        #endregion

    }
}
