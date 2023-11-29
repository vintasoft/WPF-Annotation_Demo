using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Wpf;

using WpfDemosCommonCode;
using WpfDemosCommonCode.Imaging;

namespace WpfAnnotationDemo
{
    /// <summary>
    /// A form that allows to create the annotation template, which represents signature, initials or title.
    /// </summary>
    public partial class AddSignatureWindow : Window
    {

        #region Fields

        /// <summary>
        /// The build manager for annotation templates.
        /// </summary>
        WpfAnnotationTemplateBuildManager _buildManager = null;

        /// <summary>
        /// The template category.
        /// </summary>
        string _category;

        /// <summary>
        /// Open file dialog.
        /// </summary>
        OpenFileDialog _openFileDialog = new OpenFileDialog();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddSignatureWindow"/> class.
        /// </summary>
        /// <param name="templateManager">The annotation template manager.</param>
        /// <param name="category">The template category.</param>
        public AddSignatureWindow(WpfAnnotationTemplateManager templateManager, string category)
        {
            InitializeComponent();

            _category = category;

            Title = string.Format("Create {0} Template", category);

            // set the filter for open file dialog
            WpfDemosCommonCode.Imaging.Codecs.CodecsFileFilters.SetFilters(_openFileDialog);

            // create the build manager for annotation templates
            _buildManager = new WpfAnnotationTemplateBuildManager(annotationViewer, templateManager);

            // init name comboBox
            foreach (AnnotationData annotationData in templateManager.ToArray())
            {
                if (!string.IsNullOrEmpty(annotationData.Name))
                    nameComboBox.Items.Add(annotationData.Name);
            }

            nameComboBox.Text = Environment.UserName;

            annotationViewer.AnnotationDataCollection.Changed += AnnotationDataCollection_Changed;
            annotationViewer.FocusedAnnotationViewChanged += AnnotationViewer_FocusedAnnotationViewChanged;

            UpdateUI();
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (_buildManager != null)
            {
                _buildManager.Dispose();
                _buildManager = null;
            }

            base.OnClosed(e);
        }

        #endregion


        #region PRIVATE

        #region UI

        /// <summary>
        /// Handles the Click event of FreehandButton object.
        /// </summary>
        private void freehandButton_Click(object sender, RoutedEventArgs e)
        {
            _buildManager.BuildSignature();
        }

        /// <summary>
        /// Handles the Click event of TextButton object.
        /// </summary>
        private void textButton_Click(object sender, RoutedEventArgs e)
        {
            _buildManager.AddText("Test");
        }

        /// <summary>
        /// Handles the Click event of StampButton object.
        /// </summary>
        private void stampButton_Click(object sender, RoutedEventArgs e)
        {
            _buildManager.AddStamp("APPROVED", System.Drawing.Color.Green);
        }

        /// <summary>
        /// Handles the Click event of ImageButton object.
        /// </summary>
        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            _buildManager.CancelAnnotationBuilding();

            // if image file is selected
            if (_openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _buildManager.AddImage(_openFileDialog.FileName);
                }
                catch (Exception exc)
                {
                    DemosTools.ShowErrorMessage(exc);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of FontButton object.
        /// </summary>
        private void fontButton_Click(object sender, RoutedEventArgs e)
        {
            TextAnnotationData textAnnotation = (TextAnnotationData)annotationViewer.FocusedAnnotationData;
            AnnotationSolidBrush solidBrush = (AnnotationSolidBrush)textAnnotation.FontBrush;

            AnnotationFontPropertiesWindow dlg = new AnnotationFontPropertiesWindow(textAnnotation.Font, WpfObjectConverter.CreateWindowsColor(solidBrush.Color));

            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            if (dlg.ShowDialog() == true)
            {
                textAnnotation.Font = dlg.AnnotationFont;
                textAnnotation.FontBrush = new AnnotationSolidBrush(WpfObjectConverter.CreateDrawingColor(dlg.AnnotationFontColor));
                textAnnotation.Outline.Color = WpfObjectConverter.CreateDrawingColor(dlg.AnnotationFontColor);
            }
        }

        /// <summary>
        /// Handles the Click event of ClearButton object.
        /// </summary>
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            _buildManager.Clear();
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            AnnotationData data = _buildManager.AddTemplateAnnotationToTemplateManager();
            if (data != null)
            {
                data.Intent = _category;
                data.Name = nameComboBox.Text;
            }

            DialogResult = true;
        }


        #region Annotation Viewer

        /// <summary>
        /// Handles the Changed event of AnnotationDataCollection object.
        /// </summary>
        private void AnnotationDataCollection_Changed(object sender, CollectionChangeEventArgs<AnnotationData> e)
        {
            if (e.Action == CollectionChangeActionType.InsertItem ||
                e.Action == CollectionChangeActionType.ClearAndAddItems ||
                e.Action == CollectionChangeActionType.SetItem)
            {
                WpfAnnotationView view = annotationViewer.AnnotationViewCollection.FindView(e.NewValue);

                // create context menu for annotation view
                ContextMenu contextMenu = new ContextMenu();
                MenuItem propertiesMenuItem = new MenuItem();
                propertiesMenuItem.Header = "Properties...";
                propertiesMenuItem.Click += propertiesMenuItem_Click;
                contextMenu.Items.Add(propertiesMenuItem);
                view.ContextMenu = contextMenu;
            }

            UpdateUI();
        }

        /// <summary>
        /// Handles the Click event of PropertiesMenuItem object.
        /// </summary>
        private void propertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PropertyGridWindow dlg = new PropertyGridWindow(annotationViewer.FocusedAnnotationData, "Annotation Properties", false);

            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Handles the FocusedAnnotationViewChanged event of AnnotationViewer object.
        /// </summary>
        private void AnnotationViewer_FocusedAnnotationViewChanged(object sender, WpfAnnotationViewChangedEventArgs e)
        {
            UpdateUI();
        }

        #endregion

        #endregion


        #region UI State

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool hasAnnotations =
                annotationViewer.AnnotationDataCollection != null &&
                annotationViewer.AnnotationDataCollection.Count != 0;
            bool isTextAnnotationSelected = annotationViewer.FocusedAnnotationData is TextAnnotationData;

            okButton.IsEnabled = hasAnnotations;
            fontButton.IsEnabled = isTextAnnotationSelected;
        }

        #endregion

        #endregion

        #endregion

    }
}
