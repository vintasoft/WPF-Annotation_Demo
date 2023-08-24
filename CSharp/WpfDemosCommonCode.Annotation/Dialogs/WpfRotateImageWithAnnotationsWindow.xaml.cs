using System.Text;
using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.ImageProcessing;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// A window that allows to rotate image with annotations.
    /// </summary>
    public partial class WpfRotateImageWithAnnotationsWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfRotateImageWithAnnotationsWindow"/> class.
        /// </summary>
        /// <param name="sourceImagePixelFormat">The pixel format of source image.</param>
        public WpfRotateImageWithAnnotationsWindow(PixelFormat sourceImagePixelFormat)
        {
            InitializeComponent();
            _sourceImagePixelFormat = sourceImagePixelFormat;
        }

        #endregion



        #region Properties

        decimal _angle;
        /// <summary>
        /// Gets the rotation angle.
        /// </summary>
        public decimal Angle
        {
            get
            {
                return _angle;
            }
        }

        /// <summary>
        /// Gets the type of border color.
        /// </summary>
        public BorderColorType BorderColorType
        {
            get
            {
                if (BackgroundBlack.IsChecked.Value == true)
                    return BorderColorType.Black;
                else if (BackgroundWhite.IsChecked.Value == true)
                    return BorderColorType.White;
                else if (BackgroundTransparent.IsChecked.Value == true)
                    return BorderColorType.Transparent;
                return BorderColorType.AutoDetect;
            }
        }

        PixelFormat _sourceImagePixelFormat;
        /// <summary>
        /// Gets the pixel format of source image.
        /// </summary>
        public PixelFormat SourceImagePixelFormat
        {
            get
            {
                return _sourceImagePixelFormat;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _angle = (int)angleNumericUpDown.Value;

            if ((bool)BackgroundTransparent.IsChecked &&
                SourceImagePixelFormat != PixelFormat.Bgra32 &&
                SourceImagePixelFormat != PixelFormat.Bgra64)
            {
                PixelFormat pixelFormatWithTransparencySupport = GetPixelFormatWithTransparencySupport(SourceImagePixelFormat);
                StringBuilder message = new StringBuilder();
                message.AppendLine("You chose transparent background, but image pixel format does not support it.");
                message.AppendLine(string.Format("Image should be converted to {0} pixel format.", pixelFormatWithTransparencySupport));
                message.AppendLine("Press 'OK' to convert image pixel format.");
                message.AppendLine("Press 'Cancel' to use auto color detection.");

                if (MessageBox.Show(message.ToString(), "Rotate image", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    _sourceImagePixelFormat = pixelFormatWithTransparencySupport;
                }
                else
                {
                    BackgroundAutoDetect.IsChecked = true;
                }
            }

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Returns a pixel format, which supports transparency.
        /// </summary>
        /// <param name="sourceImagePixelFormat">The pixel format of source image.</param>
        /// <returns>The pixel format, which supports transparency.</returns>
        private PixelFormat GetPixelFormatWithTransparencySupport(PixelFormat sourceImagePixelFormat)
        {
            switch (sourceImagePixelFormat)
            {
                case PixelFormat.Bgr48:
                case PixelFormat.Bgra64:
                case PixelFormat.Gray16:
                    return PixelFormat.Bgra64;
            }
            return PixelFormat.Bgra32;
        }

        #endregion

    }
}
