using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

using Vintasoft.Data;
using Vintasoft.Imaging.Undo;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window for editing the undo manager settings.
    /// </summary>
    public partial class WpfUndoManagerSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// The undo manager.
        /// </summary>
        UndoManager _undoManager = null;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfUndoManagerSettingsWindow"/> class.
        /// </summary>
        /// <param name="undoManager">The undo manager.</param>
        /// <param name="dataStorage">The data storage.</param>
        public WpfUndoManagerSettingsWindow(
            UndoManager undoManager,
            IDataStorage dataStorage)
        {
            InitializeComponent();

            _undoManager = undoManager;
            _dataStorage = dataStorage;

            undoLevelNumericUpDown.Value = _undoManager.UndoLevel;

            storageGroupBox.IsEnabled = false;
            string storagePath = string.Empty;

            Type type = this.GetType();
            storagePath = Path.GetDirectoryName(type.Assembly.Location);

            storagePath = Path.Combine(storagePath, "Undo");
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);

            if (dataStorage is CompressedImageStorageInMemory)
            {
                compressedVintasoftImageInMemoryRadioButton.IsChecked = true;
            }
            else if (dataStorage is CompressedImageStorageOnDisk)
            {
                compressedVintasoftImageOnDiscRadioButton.IsChecked = true;
                CompressedImageStorageOnDisk dataStorageOnDisk = (CompressedImageStorageOnDisk)dataStorage;
                storagePath = dataStorageOnDisk.StoragePath;
            }
            else
            {
                vintasoftImageInMemoryRadioButton.IsChecked = true;
            }

            storagePathTextBox.Text = storagePath;
        }

        #endregion



        #region Properties

        IDataStorage _dataStorage = null;
        /// <summary>
        /// Gets the data storage.
        /// </summary>
        public IDataStorage DataStorage
        {
            get
            {
                return _dataStorage;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _undoManager.UndoLevel = (int)undoLevelNumericUpDown.Value;


            if (compressedVintasoftImageInMemoryRadioButton.IsChecked.Value == true)
            {
                if (!(_dataStorage is CompressedImageStorageInMemory))
                    _dataStorage = new CompressedImageStorageInMemory();
            }
            else if (compressedVintasoftImageOnDiscRadioButton.IsChecked.Value == true)
            {
                CompressedImageStorageOnDisk prevDataStorage =
                    _dataStorage as CompressedImageStorageOnDisk;

                if (prevDataStorage == null ||
                    !prevDataStorage.StoragePath.Equals(storagePathTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    _dataStorage = new CompressedImageStorageOnDisk(storagePathTextBox.Text);
                }
            }
            else
            {
                _dataStorage = null;
            }

            this.DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// "Store compressed VintasoftImage on disk" radio button check is changed.
        /// </summary>
        private void compressedVintasoftImageOnDiscRadioButton_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (compressedVintasoftImageOnDiscRadioButton.IsChecked.Value == true)
                storageGroupBox.IsEnabled = true;
            else
                storageGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// The "Storage folder" button is clicked.
        /// </summary>
        private void storageFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = storagePathTextBox.Text;
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    storagePathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        #endregion

    }
}
