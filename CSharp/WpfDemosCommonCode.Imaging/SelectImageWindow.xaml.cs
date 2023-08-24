using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A form that allows to select image of image collection.
    /// </summary>
    public partial class SelectImageWindow : Window
    {

        #region Fields

        /// <summary>
        /// The collection of images.
        /// </summary>
        ImageCollection _images;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectImageWindow"/> class.
        /// </summary>
        private SelectImageWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectImageForm"/> class.
        /// </summary>
        /// <param name="images">The collection of images.</param>
        public SelectImageWindow(ImageCollection images)
            :this()
        {
            _images = images;
            selectedImageNumericUpDown.Maximum = images.Count;
            ImagePreviewViewer.Images.AddRange(images.ToArray());
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the index of the selected image.
        /// </summary>
        public int SelectedImageIndex
        {
            get
            {
                return (int)selectedImageNumericUpDown.Value - 1;
            }
            set
            {
                selectedImageNumericUpDown.Value = value + 1;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Opens the form that allows to select image of image collection
        /// and returns the selected image.
        /// </summary>
        /// <param name="filename">The filename of image file.</param>
        /// <returns>Selected image.</returns>
        public static VintasoftImage SelectImageFromFile(string filename)
        {
            // temporary image for image in current file
            VintasoftImage image;

            // selected index
            int index;

            // create image collection
            using (ImageCollection images = SelectImageFromFile(filename, out index))
            {
                if (index == -1)
                    return null;

                // get the selected image
                image = images[index];

                // remove the selected image from image collection
                images.Remove(image);

                // if the count of images is more than one
                if (images.Count > 1)
                {
                    images.ClearAndDisposeItems();
                }
            }

            return image;
        }

        /// <summary>
        /// Opens a form that allows to select image of image collection
        /// and returns image collection and selected index.
        /// </summary>
        /// <param name="filename">The filename of image file.</param>
        /// <param name="selectedIndex">Selected index.</param>
        /// <returns>Image collection.</returns>
        public static ImageCollection SelectImageFromFile(string filename, out int selectedIndex)
        {
            selectedIndex = -1;

            // create image collection
            ImageCollection images = new ImageCollection();
            try
            {
                try
                {
                    // add an image file to the image collection
                    images.Add(filename);
                }
                catch (System.Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                    return null;
                }

                selectedIndex = 0;

                // if image file contains more than 1 image
                if (images.Count > 1)
                {
                    // create a dialog that allows to select image from multipage image file
                    SelectImageWindow selectImage = new SelectImageWindow(images);
                    selectImage.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    // if image is selected
                    if (selectImage.ShowDialog().Value)
                    {
                        // get the selected index
                        selectedIndex = selectImage.SelectedImageIndex;
                    }
                    else
                    {
                        selectedIndex = -1;
                    }
                }
            }
            finally
            {
                // if image is not selected
                if (selectedIndex == -1)
                {
                    // clear and dispose images from the image collection
                    images.ClearAndDisposeItems();

                    // dispose image collection
                    images.Dispose();
                }
            }
            return images;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// The index of focused image is changed.
        /// </summary>
        private void thumbnailViewer1_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            selectedImageNumericUpDown.Value = e.NewValue + 1;
        }

        /// <summary>
        /// The index of selected image is changed.
        /// </summary>
        private void NumericUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ImagePreviewViewer != null)
            {
                ImagePreviewViewer.FocusedIndex = SelectedImageIndex;
                if (ImagePreviewViewer.SelectedThumbnails.Count > 0)
                    ImagePreviewViewer.SelectedThumbnails.RemoveAt(0);

                ThumbnailImageItem thumbnail = ImagePreviewViewer.Thumbnails[SelectedImageIndex];
                ImagePreviewViewer.SelectedThumbnails.Add(thumbnail);
            }
        }

        #endregion

        #endregion

    }
}
