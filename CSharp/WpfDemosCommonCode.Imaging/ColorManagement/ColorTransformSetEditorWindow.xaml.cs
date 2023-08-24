using System.Windows;

using Vintasoft.Imaging.ColorManagement;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to view and edit the color transformation set.
    /// </summary>
    public partial class ColorTransformSetEditorWindow : Window
    {

        #region Constructors

        private ColorTransformSetEditorWindow()
        {
            InitializeComponent();
        }

        public ColorTransformSetEditorWindow(ColorTransformSet colorTransformSet)
            : this()
        {
            _colorTransformSet = new ColorTransformSet(colorTransformSet);
            RefreshColorTransformsList();

            // add standard transforms
            availableColorTransformsListBox.Items.Add(ColorTransforms.CmykToPcsXyzD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.SRgbToPcsXyzD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.SRgbToPcsXyzD50Fast);
            availableColorTransformsListBox.Items.Add(ColorTransforms.GrayToPcsXyzD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.PcsLabToPcsXyzD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.PcsXyzToPcsLabD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.PcsXyzToBgrD50);
            availableColorTransformsListBox.Items.Add(ColorTransforms.PcsXyzToBgrD50Fast);
            availableColorTransformsListBox.Items.Add(ColorTransforms.PcsXyzToGray);
        }

        #endregion



        #region Properties

        ColorTransformSet _colorTransformSet;
        /// <summary>
        /// Gets color transform set.
        /// </summary>
        public ColorTransformSet ColorTransformSet
        {
            get
            {
                return _colorTransformSet;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Delete selected color transform form transform set.
        /// </summary>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (colorTransformsListBox.SelectedItem != null)
            {
                ColorTransform selectedColorTransform = (ColorTransform)colorTransformsListBox.SelectedItem;
                _colorTransformSet.Remove(selectedColorTransform);
                RefreshColorTransformsList();
            }
        }

        /// <summary>
        /// Copy selected color transform to transform set.
        /// </summary>
        private void copyToTransformSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (availableColorTransformsListBox.SelectedItem != null)
            {
                ColorTransform selectedColorTransform = (ColorTransform)availableColorTransformsListBox.SelectedItem;
                _colorTransformSet.Add(selectedColorTransform);
                RefreshColorTransformsList();
            }
        }

        /// <summary>
        /// Refresh the list of color tansforms in transform set.
        /// </summary>
        private void RefreshColorTransformsList()
        {
            colorTransformsListBox.Items.Clear();
            ColorTransform[] colorTransformSet = _colorTransformSet.ToArray();
            foreach (ColorTransform colorTransform in colorTransformSet)
                colorTransformsListBox.Items.Add(colorTransform);
        }

        /// <summary>
        /// Handles the Click event of ButtonOk object.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
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
