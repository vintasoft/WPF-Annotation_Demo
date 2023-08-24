using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs;
using Vintasoft.Imaging.ColorManagement;
using Vintasoft.Imaging.ColorManagement.Icc;

using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Codecs.Decoders;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to view and edit the color management decode settings.
    /// </summary>
    public partial class ColorManagementSettingsWindow : Window
    {

        #region Constants

        /// <summary>
        /// Default input CMYK ICC profile.
        /// </summary>
        public const string DefaultInputCmykProfile = "DefaultCMYK.icc";

        #endregion



        #region Fields

        /// <summary>
        /// Open ICC file dialog.
        /// </summary>
        OpenFileDialog _openIccFileDialog;

        #endregion



        #region Constructors

        public ColorManagementSettingsWindow()
        {
            InitializeComponent();

            intentComboBox.Items.Add(RenderingIntent.Perceptual);
            intentComboBox.Items.Add(RenderingIntent.MediaRelativeColorimetric);
            intentComboBox.Items.Add(RenderingIntent.Saturation);
            intentComboBox.Items.Add(RenderingIntent.IccAbsoluteColorimetric);
            intentComboBox.SelectedItem = RenderingIntent.Perceptual;

            RemoveProfileDescription(inputCmykTextBox);
            RemoveProfileDescription(inputRgbTextBox);
            RemoveProfileDescription(inputGrayscaleTextBox);
            RemoveProfileDescription(outputRgbTextBox);
            RemoveProfileDescription(outputGrayscaleTextBox);

            _openIccFileDialog = new OpenFileDialog();
            _openIccFileDialog.Filter = "ICC profiles|*.icc;*.icm|All files|*.*";
            _openIccFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ResourceAssembly.Location);
        }

        #endregion



        #region Properties

        ColorManagementDecodeSettings _colorManagementSettings;
        /// <summary>
        /// Gets or sets a color management decode settings.
        /// </summary>
        public ColorManagementDecodeSettings ColorManagementSettings
        {
            get
            {
                SetSettings();
                return _colorManagementSettings;
            }
            set
            {
                _colorManagementSettings = value;
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        #region Event handlers

        /// <summary>
        /// Sets input ICC profile.
        /// </summary>
        private void setInputProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_openIccFileDialog.ShowDialog() == true)
            {
                try
                {
                    IccProfile iccProfile = new IccProfile(_openIccFileDialog.FileName);
                    _openIccFileDialog.InitialDirectory = Path.GetDirectoryName(_openIccFileDialog.FileName);
                    IccProfile oldIccProfile = null;

                    switch (iccProfile.DeviceColorSpace)
                    {
                        case ColorSpaceType.CMYK:
                            oldIccProfile = _colorManagementSettings.InputCmykProfile;
                            _colorManagementSettings.InputCmykProfile = iccProfile;
                            ShowProfileDescription(inputCmykTextBox, iccProfile);
                            break;

                        case ColorSpaceType.sRGB:
                            oldIccProfile = _colorManagementSettings.InputRgbProfile;
                            _colorManagementSettings.InputRgbProfile = iccProfile;
                            ShowProfileDescription(inputRgbTextBox, iccProfile);
                            break;

                        case ColorSpaceType.Gray:
                            oldIccProfile = _colorManagementSettings.InputGrayscaleProfile;
                            _colorManagementSettings.InputGrayscaleProfile = iccProfile;
                            ShowProfileDescription(inputGrayscaleTextBox, iccProfile);
                            break;

                        default:
                            iccProfile.Dispose();
                            throw new Exception(string.Format("Unexpected profile color space: {0}.", iccProfile.DeviceColorSpace));
                    }

                    if (oldIccProfile != null)
                        oldIccProfile.Dispose();
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Sets output ICC profile.
        /// </summary>
        private void setOutputProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_openIccFileDialog.ShowDialog() == true)
            {
                try
                {
                    IccProfile iccProfile = new IccProfile(_openIccFileDialog.FileName);
                    _openIccFileDialog.InitialDirectory = Path.GetDirectoryName(_openIccFileDialog.FileName);
                    IccProfile oldIccProfile = null;

                    switch (iccProfile.DeviceColorSpace)
                    {
                        case ColorSpaceType.sRGB:
                            oldIccProfile = _colorManagementSettings.OutputRgbProfile;
                            _colorManagementSettings.OutputRgbProfile = iccProfile;
                            ShowProfileDescription(outputRgbTextBox, iccProfile);
                            break;

                        case ColorSpaceType.Gray:
                            oldIccProfile = _colorManagementSettings.OutputGrayscaleProfile;
                            _colorManagementSettings.OutputGrayscaleProfile = iccProfile;
                            ShowProfileDescription(outputGrayscaleTextBox, iccProfile);
                            break;

                        default:
                            iccProfile.Dispose();
                            throw new Exception(string.Format("Unexpected profile color space: {0}.", iccProfile.DeviceColorSpace));
                    }

                    if (oldIccProfile != null)
                        oldIccProfile.Dispose();
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Removes input CMYK profile.
        /// </summary>
        private void removeInputCmykButton_Click(object sender, RoutedEventArgs e)
        {
            IccProfile profile = _colorManagementSettings.InputCmykProfile;
            if (profile != null)
            {
                _colorManagementSettings.InputCmykProfile = null;
                profile.Dispose();
            }
            RemoveProfileDescription(inputCmykTextBox);
        }

        /// <summary>
        /// Removes input RGB profile.
        /// </summary>
        private void removeInputRgbButton_Click(object sender, RoutedEventArgs e)
        {
            IccProfile profile = _colorManagementSettings.InputRgbProfile;
            if (profile != null)
            {
                _colorManagementSettings.InputRgbProfile = null;
                profile.Dispose();
            }
            RemoveProfileDescription(inputRgbTextBox);
        }

        /// <summary>
        /// Removes input Gray profile.
        /// </summary>
        private void removeInputGrayscaleButton_Click(object sender, RoutedEventArgs e)
        {
            IccProfile profile = _colorManagementSettings.InputGrayscaleProfile;
            if (profile != null)
            {
                _colorManagementSettings.InputGrayscaleProfile = null;
                profile.Dispose();
            }
            RemoveProfileDescription(inputGrayscaleTextBox);
        }

        /// <summary>
        /// Removes output RGB profile.
        /// </summary>
        private void removeOutputRgbButton_Click(object sender, RoutedEventArgs e)
        {
            IccProfile profile = _colorManagementSettings.OutputRgbProfile;
            if (profile != null)
            {
                _colorManagementSettings.OutputRgbProfile = null;
                profile.Dispose();
            }
            RemoveProfileDescription(outputRgbTextBox);
        }

        /// <summary>
        /// Removes output Gray profile.
        /// </summary>
        private void removeOutputGrayscaleButton_Click(object sender, RoutedEventArgs e)
        {
            IccProfile profile = _colorManagementSettings.OutputGrayscaleProfile;
            if (profile != null)
            {
                _colorManagementSettings.OutputGrayscaleProfile = null;
                profile.Dispose();
            }
            RemoveProfileDescription(outputGrayscaleTextBox);
        }

        /// <summary>
        /// Open the color transform set editor.
        /// </summary>
        private void editColorTransformsButton_Click(object sender, RoutedEventArgs e)
        {
            ColorTransformSetEditorWindow editorForm = new ColorTransformSetEditorWindow(_colorManagementSettings.ColorSpaceTransforms);
            if (editorForm.ShowDialog() == true)
            {
                _colorManagementSettings.ColorSpaceTransforms.Clear();
                _colorManagementSettings.ColorSpaceTransforms.AddRange(editorForm.ColorTransformSet.ToArray());
            }
        }

        /// <summary>
        /// Handles the Checked event of EnableColorManagementCheckBox object.
        /// </summary>
        private void enableColorManagementCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (enableColorManagementCheckBox.IsChecked.Value)
                decodingSettingsGroupBox.IsEnabled = true;
            else
                decodingSettingsGroupBox.IsEnabled = false;

            if (decodingSettingsGroupBox.IsEnabled && _colorManagementSettings == null)
            {
                ColorManagementDecodeSettings settings = new ColorManagementDecodeSettings();
                LoadDefaultInputCmykProfile(settings);
                ColorManagementSettings = settings;
            }
        }

        /// <summary>
        /// Handles the Click event of ButtonOk object.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            SetSettings();
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

        /// <summary>
        /// Edits a color management settings of specified image viewer.
        /// </summary>
        public static bool EditColorManagement(WpfImageViewerBase imageViewer)
        {
            ColorManagementSettingsWindow colorManagementSettingsWindow = new ColorManagementSettingsWindow();
            if (imageViewer.ImageDecodingSettings == null)
                colorManagementSettingsWindow.ColorManagementSettings = null;
            else
                colorManagementSettingsWindow.ColorManagementSettings = imageViewer.ImageDecodingSettings.ColorManagement;

            if (colorManagementSettingsWindow.ShowDialog().Value)
            {
                DecodingSettings settings = imageViewer.ImageDecodingSettings;
                if (settings == null)
                    settings = new DecodingSettings();

                settings.ColorManagement = colorManagementSettingsWindow.ColorManagementSettings;
                imageViewer.ImageDecodingSettings = settings;

                try
                {
                    ImageCollection images = imageViewer.Images;
                    int focusedIndex = imageViewer.FocusedIndex;
                    VintasoftImage focusedImage = null;
                    if (focusedIndex >= 0 && focusedIndex < images.Count)
                    {
                        focusedImage = images[focusedIndex];
                        if (focusedImage != null)
                            focusedImage.Reload(true);
                    }

                    foreach (VintasoftImage nextImage in imageViewer.Images)
                    {
                        if (nextImage != focusedImage)
                            nextImage.Reload(true);
                    }
                }
                catch (Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            if (_colorManagementSettings == null)
            {
                enableColorManagementCheckBox.IsChecked = false;
            }
            else
            {
                enableColorManagementCheckBox.IsChecked = true;

                useEmbeddedProfilesCheckBox.IsChecked = _colorManagementSettings.UseEmbeddedInputProfile;
                blackPointCompensationCheckBox.IsChecked = _colorManagementSettings.UseBlackPointCompensation;
                intentComboBox.SelectedItem = _colorManagementSettings.RenderingIntent;

                if (_colorManagementSettings.InputCmykProfile != null)
                    ShowProfileDescription(inputCmykTextBox, _colorManagementSettings.InputCmykProfile);

                if (_colorManagementSettings.InputRgbProfile != null)
                    ShowProfileDescription(inputRgbTextBox, _colorManagementSettings.InputRgbProfile);

                if (_colorManagementSettings.InputGrayscaleProfile != null)
                    ShowProfileDescription(inputGrayscaleTextBox, _colorManagementSettings.InputGrayscaleProfile);

                if (_colorManagementSettings.OutputRgbProfile != null)
                    ShowProfileDescription(outputRgbTextBox, _colorManagementSettings.OutputRgbProfile);

                if (_colorManagementSettings.OutputGrayscaleProfile != null)
                    ShowProfileDescription(outputGrayscaleTextBox, _colorManagementSettings.OutputGrayscaleProfile);
            }
        }

        /// <summary>
        /// Sets curent settings to <see cref="ColorManagementSettings"/> property.
        /// </summary>
        private void SetSettings()
        {
            if (enableColorManagementCheckBox.IsChecked.Value == true)
            {
                if (useEmbeddedProfilesCheckBox.IsChecked.Value == true)
                    _colorManagementSettings.UseEmbeddedInputProfile = true;
                else
                    _colorManagementSettings.UseEmbeddedInputProfile = false;
                if (blackPointCompensationCheckBox.IsChecked.Value == true)
                    _colorManagementSettings.UseBlackPointCompensation = true;
                else
                    _colorManagementSettings.UseBlackPointCompensation = false;
                _colorManagementSettings.RenderingIntent = (RenderingIntent)intentComboBox.SelectedItem;
            }
            else
            {
                _colorManagementSettings = null;
            }
        }

        /// <summary>
        /// Loads a default input CMYK ICC profile.
        /// </summary>
        private void LoadDefaultInputCmykProfile(ColorManagementDecodeSettings settings)
        {
            try
            {
                // search directories
                string[] directories = new string[]
                    {
                        "",
                        @"..\..\..\..\Bin\DotNet4\AnyCPU\",
                    };

                // search profile
                string defaultInputCmykFilename = "";
                foreach (string dir in directories)
                {
                    string profileDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.ResourceAssembly.ManifestModule.FullyQualifiedName), dir));
                    if (File.Exists(Path.Combine(profileDirectory, DefaultInputCmykProfile)))
                    {
                        defaultInputCmykFilename = Path.Combine(profileDirectory, DefaultInputCmykProfile);
                        break;
                    }
                }

                // if profile was found
                if (defaultInputCmykFilename != "")
                    settings.InputCmykProfile = new IccProfile(defaultInputCmykFilename);
            }
            catch
            {
            }
        }

        private void ShowProfileDescription(TextBox textBox, IccProfile profile)
        {
            textBox.Text = profile.ToString();
        }

        private void RemoveProfileDescription(TextBox textBox)
        {
            textBox.Text = "(none)";
        }

        #endregion

    }
}
