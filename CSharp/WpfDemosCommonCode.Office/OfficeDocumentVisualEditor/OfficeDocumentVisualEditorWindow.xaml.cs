#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System;
using System.ComponentModel;
using System.Windows;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Editor.Docx;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OfficeDocumentVisualEditorWindow.xaml
    /// </summary>
    public partial class OfficeDocumentVisualEditorWindow : Window
    {

        #region Constants

        /// <summary>
        /// The content scale delta, in percents.
        /// </summary>
        const float ContentScaleDelta = 10f;

        #endregion



        #region Fields

        /// <summary>
        /// The last location of form that owns this form.
        /// </summary>
        Point _ownerLastLocation;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OfficeDocumentVisualEditorWindow"/> class.
        /// </summary>
        public OfficeDocumentVisualEditorWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        Point _locationOffsetFromOwnerForm = new Point(-100, 0);
        /// <summary>
        /// Gets or sets the offset from owner form.
        /// </summary>
        public Point LocationOffsetFromOwnerForm
        {
            get
            {
                return _locationOffsetFromOwnerForm;
            }
            set
            {
                _locationOffsetFromOwnerForm = value;
            }
        }

        IWpfInteractionController _officeDocumentInteractionController = null;
        /// <summary>
        /// Gets or sets the interaction controller for Office document.
        /// </summary>
        private IWpfInteractionController OfficeDocumentInteractionController
        {
            get
            {
                return _officeDocumentInteractionController;
            }
            set
            {
                if (_officeDocumentInteractionController != value)
                {
                    if (_officeDocumentInteractionController != null)
                        _officeDocumentInteractionController.Interaction -= OfficeDocumentInteractionController_Interaction;
                    _officeDocumentInteractionController = value;
                    if (_officeDocumentInteractionController != null)
                        _officeDocumentInteractionController.Interaction += OfficeDocumentInteractionController_Interaction;
                }
            }
        }

        WpfOfficeDocumentVisualEditor _visualEditor = null;
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
                        _visualEditor.DocumentChanged -= _visualEditor_DocumentChanged;

                    _visualEditor = value;

                    if (_visualEditor != null)
                        _visualEditor.DocumentChanged += _visualEditor_DocumentChanged;

                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Gets or sets the location of owner window.
        /// </summary>
        private Point OwnerLocation
        {
            get
            {
                return new Point(Owner.Left, Owner.Top);
            }
        }


        /// <summary>
        /// Gets or sets the location of this window.
        /// </summary>
        private Point Location
        {
            get
            {
                return new Point(Left, Top);
            }
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }
        #endregion



        #region Methods

        /// <summary>
        /// Adds visual tool, which uses the Office document visual editor.
        /// </summary>
        public void AddVisualTool(WpfUserInteractionVisualTool visualTool)
        {
            visualTool.ActiveInteractionControllerChanged += VisualTool_ActiveInteractionControllerChanged;
            officeDocumentFontPropertiesVisualEditorToolBar.AddVisualTool(visualTool);
            officeDocumentTextPropertiesVisualEditorToolBar.AddVisualTool(visualTool);
            officeDocumentParagraphPropertiesVisualEditorToolBar.AddVisualTool(visualTool);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
        }

        /// <summary>
        /// Handles the ActiveInteractionControllerChanged event of the VisualTool.
        /// </summary>
        private void VisualTool_ActiveInteractionControllerChanged(object sender, Vintasoft.Imaging.PropertyChangedEventArgs<IWpfInteractionController> e)
        {
            // if new interaction controller exists
            if (e.NewValue != null)
            {
                // find visual editor in interaction controller
                VisualEditor = WpfCompositeInteractionController.FindInteractionController<WpfOfficeDocumentVisualEditor>(e.NewValue);
            }
            else
            {
                VisualEditor = null;
            }

            // if visual editor exists
            if (VisualEditor != null)
            {
                // save reference to interaction controller for Office document
                OfficeDocumentInteractionController = e.NewValue;
            }
            else
            {
                OfficeDocumentInteractionController = null;
            }

            // if visual editor is not found or disabled
            if (VisualEditor == null || !VisualEditor.IsEnabled)
            {
                // if this windows is visible
                if (IsVisible)
                    // hide this window
                    Hide();
            }
            else
            {
                // if this windows is not visible
                if (!IsVisible)
                {
                    try
                    {
                        // if owner window is specified
                        if (Owner != null)
                        {
                            // if owner form is moved
                            if (_ownerLastLocation != OwnerLocation)
                            {
                                // reset location
                                _ownerLastLocation = OwnerLocation;
                                double x = LocationOffsetFromOwnerForm.X;
                                if (x < 0)
                                    x += Owner.Width - Width;
                                double y = LocationOffsetFromOwnerForm.Y;
                                if (y < 0)
                                    y += Owner.Height - Height;
                                Location = new Point(OwnerLocation.X + x, OwnerLocation.Y + y);
                            }

                            // show this window
                            Show();

                            // set focus to the owner window
                            Owner.Focus();
                        }
                        else
                        {
                            // show this window
                            Show();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            if (_visualEditor != null)
            {
                try
                {
                    DocxDocumentEditor editor = _visualEditor.CreateDocumentEditor();
                    if (editor != null)
                    {
                        chartsButton.IsEnabled = editor.Charts.Length > 0;
                        editor.Dispose();
                    }
                }
                catch
                {
                    chartsButton.IsEnabled = false;
                }
            }
            else
            {
                chartsButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the Interaction event of the OfficeDocumentInteractionController.
        /// </summary>
        private void OfficeDocumentInteractionController_Interaction(object sender, WpfInteractionEventArgs e)
        {
            // if mouse is double clicked
            if (e.MouseClickCount == 2)
            {
                // find the visual editor
                WpfOfficeDocumentVisualEditor visualEditor = WpfCompositeInteractionController.FindInteractionController<WpfOfficeDocumentVisualEditor>(e.InteractionController);
                if (visualEditor != null && visualEditor.HasCharts)
                {
                    // create document editor
                    using (DocxDocumentEditor editor = visualEditor.CreateDocumentEditor())
                    {
                        // if document has chart(s) add does not have text
                        if (editor.Charts.Length > 0 && string.IsNullOrEmpty(editor.Body.Text.Trim('\n')))
                        {
                            // open window that allows to view a chart
                            ShowChartWindow(visualEditor);
                            e.InteractionOccured();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows window that allows to view a chart.
        /// </summary>
        /// <param name="visualEditor">The visual editor for Office document.</param>
        private void ShowChartWindow(WpfOfficeDocumentVisualEditor visualEditor)
        {
            OpenXmlDocumentChartDataWindow chartForm = new OpenXmlDocumentChartDataWindow();
            chartForm.VisualEditor = visualEditor;
            chartForm.WindowStartupLocation = WindowStartupLocation.Manual;
            if (Owner != null)
            {
                chartForm.Left = Owner.Left;
                chartForm.Top = Owner.Top;
            }
            else
            {
                chartForm.Left = Left;
                chartForm.Top = Top;
            }
            chartForm.ShowDialog();
        }

        /// <summary>
        /// Handles the DocumentChanged event of VisualEditor object.
        /// </summary>
        private void _visualEditor_DocumentChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of ChartButton object.
        /// </summary>
        private void chartButton_Click(object sender, RoutedEventArgs e)
        {
            ShowChartWindow(_visualEditor);
        }

        /// <summary>
        /// Handles the Click event of IncreaseContentScaleButton object.
        /// </summary>
        private void increaseContentScaleButton_Click(object sender, RoutedEventArgs e)
        {
            _visualEditor.InteractiveObject.ContentScale *= (1 + ContentScaleDelta / 100f);
        }

        /// <summary>
        /// Handles the Click event of DecreaseContentScaleButton object.
        /// </summary>
        private void decreaseContentScaleButton_Click(object sender, RoutedEventArgs e)
        {
            _visualEditor.InteractiveObject.ContentScale *= (1 - ContentScaleDelta / 100f);
        }

    }
}
