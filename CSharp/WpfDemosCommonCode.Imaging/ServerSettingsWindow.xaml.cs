using System.Windows;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to view and edit the server settings.
    /// </summary>
    public partial class ServerSettingsWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSettingsWindow"/> class.
        /// </summary>
        public ServerSettingsWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets a server URL.
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return urlTextBox.Text;
            }
            set
            {
                urlTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets a user name.
        /// </summary>
        public string ServerUserName
        {
            get
            {
                return userNameTextBox.Text;
            }
            set
            {
                userNameTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets a user password.
        /// </summary>
        public string ServerPassword
        {
            get
            {
                return passwordTextBox.Text;
            }
            set
            {
                passwordTextBox.Text = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
