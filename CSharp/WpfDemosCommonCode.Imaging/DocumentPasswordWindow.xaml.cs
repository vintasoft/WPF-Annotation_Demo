using System.IO;
using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to enter password for document.
    /// </summary>
    public partial class DocumentPasswordWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPasswordWindow"/> class.
        /// </summary>
        public DocumentPasswordWindow()
        {
            InitializeComponent();

            authenticateTypeComboBox.Items.Add("User");
            authenticateTypeComboBox.Items.Add("Owner");

            passwordTextBox.Focus();
        }

        #endregion



        #region Properties

        string _filename;
        /// <summary>
        /// Gets or sets the filename of document.
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (_filename != null)
                    Title = string.Format("Authentication - {0}", Path.GetFileName(_filename));
                else
                    Title = "Authentication";
            }
        }

        /// <summary>
        /// Gets the password of document.
        /// </summary>
        public string Password
        {
            get
            {
                return passwordTextBox.Text;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Shows the incorrect password message.
        /// </summary>
        public void ShowIncorrectPasswordMessage()
        {
            MessageBox.Show(string.Format("The '{0}' password is incorrect.", Password), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Authenticates the specified document.
        /// </summary>
        /// <param name="decoder">The decoder that provides access to the document.</param>
        public static bool Authenticate(DecoderBase decoder)
        {
            // if authentication is required
            if (decoder.IsAuthenticationRequired)
            {
                // get the document filename
                string filename = null;
                if (decoder.SourceStream != null && decoder.SourceStream is FileStream)
                    filename = ((FileStream)decoder.SourceStream).Name;

                while (true)
                {
                    // create the document password dialog
                    DocumentPasswordWindow documentPasswordWindow = new DocumentPasswordWindow();
                    documentPasswordWindow.Filename = filename;

                    // show the document password dialog
                    bool? showDialogResult = documentPasswordWindow.ShowDialog();

                    // if "OK" button is clicked
                    if (showDialogResult == true)
                    {
                        DocumentAuthenticationRequest authenticationRequest = new DocumentAuthenticationRequest(
                            documentPasswordWindow.authenticateTypeComboBox.Text,
                            documentPasswordWindow.Password);

                        // uuthenticate the decoder
                        DocumentAuthorizationResult authorizationResult = decoder.Authenticate(authenticationRequest);
                        // if user is NOT authorized
                        if (!authorizationResult.IsAuthorized)
                        {
                            documentPasswordWindow.ShowIncorrectPasswordMessage();
                        }
                        // if user is authorized
                        else
                        {
                            if (documentPasswordWindow.authenticateTypeComboBox.Text != authorizationResult.UserName)
                                MessageBox.Show(
                                    string.Format("Authorized as: {0}", authorizationResult.UserName),
                                    "Authorization Result", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }
                    // if "Cancel" button is clicked
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Enables the authentication for specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        public static void EnableAuthentication(WpfImageViewerBase imageViewer)
        {
            // enable the authentication for image collection of image viewer
            EnableAuthentication(imageViewer.Images);
        }

        /// <summary>
        /// Enables the authentication for specified image collection.
        /// </summary>
        /// <param name="imageViewer">The image collection.</param>
        public static void EnableAuthentication(ImageCollection imageCollection)
        {
            // subscribe to the ImageCollection.AuthenticationRequest event
            imageCollection.AuthenticationRequest += ImageViewer_AuthenticationRequest;
        }

        /// <summary>
        /// Disables the authentication for specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        public static void DisableAuthentication(WpfImageViewerBase imageViewer)
        {
            // disable the authentication for image collection of image viewer
            DisableAuthentication(imageViewer.Images);
        }

        /// <summary>
        /// Disables the authentication for specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image collection.</param>
        public static void DisableAuthentication(ImageCollection imageCollection)
        {
            // unsubscribe from the ImageCollection.AuthenticationRequest event
            imageCollection.AuthenticationRequest -= ImageViewer_AuthenticationRequest;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
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
        /// The document, which is adding to the image collection, requires authentication.
        /// </summary>
        private static void ImageViewer_AuthenticationRequest(object sender, Vintasoft.Imaging.DocumentAuthenticationRequestEventArgs e)
        {
            if (!Authenticate(e.Decoder))
                e.IsCanceled = true;
        }

        #endregion

        #endregion

    }
}
