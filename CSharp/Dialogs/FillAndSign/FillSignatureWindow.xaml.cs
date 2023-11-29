using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging.Annotation.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode;

namespace WpfAnnotationDemo
{
    /// <summary>
    /// A window that allows to create, view and select the signature-annotation.
    /// </summary>
    public partial class FillSignatureWindow : Window
    {

        #region Fields

        /// <summary>
        /// The annotation template manager that stores the signature-annotations.
        /// </summary>
        WpfAnnotationTemplateManager _templateManager;

        /// <summary>
        /// Open file dialog.
        /// </summary>
        OpenFileDialog _openFileDialog = new OpenFileDialog();

        /// <summary>
        /// Save file dialog.
        /// </summary>
        SaveFileDialog _saveFileDialog = new SaveFileDialog();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FillSignatureWindow"/> class.
        /// </summary>
        public FillSignatureWindow()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillSignatureWindow"/> class.
        /// </summary>
        /// <param name="annotations">The annotation templates.</param>
        public FillSignatureWindow(IEnumerable<AnnotationData> annotations)
        {
            InitializeComponent();

            // set thumbnail caption style
            ThumbnailImageItemCaption caption = new ThumbnailImageItemCaption();
            caption.FontSize = 12;
            caption.FontFamily = new FontFamily("Arial Narrow");
            caption.Padding = new Thickness(0, 0, 0, 0);
            annotatedThumbnailViewer.ThumbnailCaption = caption;

            // create template manager and add annotations
            _templateManager = new WpfAnnotationTemplateManager(annotatedThumbnailViewer);
            if (annotations != null)
                _templateManager.AddRange(annotations);
            _templateManager.ShowDescriptions = true;
            if (_templateManager.ShowDescriptions)
            {
                double spacing = annotatedThumbnailViewer.ThumbnailCaption.FontFamily.LineSpacing;
                double fontSizeInPixels = Vintasoft.Imaging.Utils.UnitOfMeasureConverter.ConvertToPixels(12, UnitOfMeasure.Points);
                _templateManager.ThumbnailPaddingSize = new Size(0, spacing * fontSizeInPixels * 2);
            }

            // set file dialog filters
            string filterFormats = "TIFF File(*.tiff;*.tif)|*.tiff;*.tif|Binary Annotations(*.vsab)|*.vsab|XMP Annotations(*.xmp)|*.xmp|WANG Annotations(*.wng)|*.wng";
            _saveFileDialog.Filter = filterFormats;
            _openFileDialog.Filter = filterFormats + "|All Formats(*.tiff;*.tif;*.vsab;*.xmp;*.wng)|*.tiff;*.tif;*.vsab;*.xmp;*.wng";
            _openFileDialog.FilterIndex = 5;

            annotatedThumbnailViewer.Images.ImageCollectionChanged += Images_ImageCollectionChanged;
            annotatedThumbnailViewer.FocusedIndexChanged += AnnotationViewer1_FocusedIndexChanged;
            UpdateUI();
        }

        #endregion



        #region Properties

        WpfAnnotationView _selectedSignatureAnnotation = null;
        /// <summary>
        /// Gets the copy of selected signature-annotation.
        /// </summary>
        public WpfAnnotationView SelectedSignatureAnnotation
        {
            get
            {
                return _selectedSignatureAnnotation;
            }
        }

        /// <summary>
        /// Gets or sets the index of selected signature-annotation.
        /// </summary>
        public int SelectedSignatureAnnotationIndex
        {
            get
            {
                return _templateManager.SelectedTemplateIndex;
            }
            set
            {
                _templateManager.SelectedTemplateIndex = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Returns the signature-annotations.
        /// </summary>
        /// <returns>
        /// An array that contains copies of signature-annotations.
        /// </returns>
        public AnnotationData[] GetSignatureAnnotations()
        {
            return _templateManager.ToArray();
        }

        #endregion


        #region PRIVATE

        #region UI

        private void addButton_ButtonClick(object sender, EventArgs e)
        {
            if (addSplitButton.IsSubmenuOpen)
                addSplitButton.IsSubmenuOpen = false;
            else
                addSplitButton.IsSubmenuOpen = true;
        }

        /// <summary>
        /// Handles the Click event of AddSignatureMenuItem object.
        /// </summary>
        private void addSignatureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddSignatureWindow dlg = new AddSignatureWindow(_templateManager, "Signature");
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of AddInitialsMenuItem object.
        /// </summary>
        private void addInitialsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddSignatureWindow dlg = new AddSignatureWindow(_templateManager, "Initials");
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of AddTitleMenuItem object.
        /// </summary>
        private void addTitleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddSignatureWindow dlg = new AddSignatureWindow(_templateManager, "Title");
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Owner = this;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of AddFromFileMenuItem object.
        /// </summary>
        private void addFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _templateManager.Add(_openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        private void removeButton_ButtonClick(object sender, RoutedEventArgs e)
        {
            _templateManager.RemoveSelected();
        }

        /// <summary>
        /// Handles the Click event of RemoveAllMenuItem object.
        /// </summary>
        private void removeAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _templateManager.Clear();
        }

        /// <summary>
        /// Handles the Click event of SaveButton object.
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _templateManager.Save(_saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Handles the ImageCollectionChanged event of Images object.
        /// </summary>
        private void Images_ImageCollectionChanged(object sender, Vintasoft.Imaging.ImageCollectionChangeEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the FocusedIndexChanged event of AnnotationViewer1 object.
        /// </summary>
        private void AnnotationViewer1_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            if (_templateManager.SelectedTemplateIndex != -1)
                _selectedSignatureAnnotation = _templateManager.GetTemplateViewCopy(_templateManager.SelectedTemplateIndex);

            DialogResult = true;
        }

        #endregion


        #region UI State

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            saveButton.IsEnabled = _templateManager.Count != 0;
            removeSplitButton.IsEnabled = _templateManager.SelectedTemplateIndex != -1;
        }

        #endregion

        #endregion

        #endregion

    }
}
