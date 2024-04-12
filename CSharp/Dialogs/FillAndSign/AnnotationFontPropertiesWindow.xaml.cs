using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging.Annotation;

namespace WpfAnnotationDemo
{
    /// <summary>
    /// A form that allows to change the font properties of text annotation.
    /// </summary>
    public partial class AnnotationFontPropertiesWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationFontPropertiesForm"/> class.
        /// </summary>
        /// <param name="font">The annotation font.</param>
        /// <param name="fontColor">The annotation font color.</param>
        public AnnotationFontPropertiesWindow(AnnotationFont font, Color fontColor)
        {
            InitializeComponent();

            _annotationFont = font;
            _annotationFontColor = fontColor;

            fontFamilyNameComboBox.BeginInit();
            List<string> list = new List<string>();
            foreach (FontFamily family in Fonts.SystemFontFamilies)
                list.Add(family.Source);
            list.Sort();

            foreach (string family in list)
                fontFamilyNameComboBox.Items.Add(family);
            fontFamilyNameComboBox.SelectedItem = font.FamilyName;
            fontFamilyNameComboBox.EndInit();

            fontSizeNumericUpDown.Value = font.Size;
            fontColorPanelControl.Color = fontColor;
            isBoldCheckBox.IsChecked = font.Bold;
            isItalicCheckBox.IsChecked = font.Italic;
            isStrikeoutCheckBox.IsChecked = font.Strikeout;
            isUnderlineCheckBox.IsChecked = font.Underline;
        }

        #endregion



        #region Properties

        AnnotationFont _annotationFont;
        /// <summary>
        /// Gets the annotation font.
        /// </summary>
        public AnnotationFont AnnotationFont
        {
            get
            {
                return _annotationFont;
            }
        }

        Color _annotationFontColor;
        /// <summary>
        /// Gets the annotation font color.
        /// </summary>
        public Color AnnotationFontColor
        {
            get
            {
                return _annotationFontColor;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of okButton object.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            _annotationFont = new AnnotationFont(
                (string)fontFamilyNameComboBox.SelectedItem,
                (float)fontSizeNumericUpDown.Value,
                isBoldCheckBox.IsChecked == true,
                isItalicCheckBox.IsChecked == true,
                isStrikeoutCheckBox.IsChecked == true,
                isUnderlineCheckBox.IsChecked == true,
                _annotationFont.Unit);

            _annotationFontColor = fontColorPanelControl.Color;

            DialogResult = true;
        }

        #endregion

    }
}
