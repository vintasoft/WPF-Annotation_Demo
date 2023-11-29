using System.Windows;
using System.IO;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to input password for opening the certificate with private key.
    /// </summary>
    public partial class CertificatePasswordWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificatePasswordWindow"/> class.
        /// </summary>
        public CertificatePasswordWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        string _filename;
        /// <summary>
        /// Gets or sets the certificate filename.
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
                    Title = string.Format("Password - {0}", Path.GetFileName(_filename));
                else
                    Title = "Password";
            }
        }

        /// <summary>
        /// Gets the password.
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

        /// <summary>
        /// Handles the Click event of the "OK" button.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of the "Cancel" button.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
