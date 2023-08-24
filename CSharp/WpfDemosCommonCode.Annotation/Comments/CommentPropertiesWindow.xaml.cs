using System;
using System.Windows;
using System.Windows.Controls;

#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.Comments;
#endif
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents a window that allows to display the comment properties.
    /// </summary>
    public partial class CommentPropertiesWindow : Window
    {

        #region Fields

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// The comment.
        /// </summary>
        Comment _comment;

        /// <summary>
        /// Idicates whether the <see cref="Comment.Type"/> can be changed.
        /// </summary>
        bool _canEditType = false;
#endif

        #endregion



        #region Constructors

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentPropertiesWindow"/> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public CommentPropertiesWindow(Comment comment)
        {
            InitializeComponent();

            _comment = comment;

            commentDataPropertyGrid.SelectedObject = comment;
            commentStateHistoryControl.Comment = comment;

            UpdateCommonTabItem();
        }
#endif

        #endregion



        #region Methods

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Updates the user interface of this window.
        /// </summary>
        private void UpdateUI()
        {
            typeComboBox.IsEnabled = _canEditType && !_comment.IsReadOnly;
            colorPanelControl.IsEnabled = !_comment.IsReadOnly;
            userNameTextBox.IsEnabled = !_comment.IsReadOnly;
            subjectTextBox.IsEnabled = !_comment.IsReadOnly;
            textBox.IsEnabled = !_comment.IsReadOnly;
        }

        /// <summary>
        /// Updates the common tab item.
        /// </summary>
        private void UpdateCommonTabItem()
        {
            UpdateModifyDateTime();

            if (_comment.CreationDate != DateTime.MinValue)
                creationDateTimeTextBox.Text = _comment.CreationDate.ToString();
            else
                creationDateTimeTextBox.Text = string.Empty;


            // update type combo box

            typeComboBox.Items.Clear();
            typeComboBox.IsEditable = false;
            string[] availableTypes = _comment.GetAvailableTypes();
            if (availableTypes != null)
            {
                if (availableTypes.Length == 0)
                {
                    typeComboBox.IsEditable = true;
                    typeComboBox.Text = _comment.Type;
                }
                else
                {
                    foreach (string commentType in availableTypes)
                        typeComboBox.Items.Add(commentType);

                    if (_comment.Type != null && Array.IndexOf(availableTypes, _comment.Type) == -1)
                        typeComboBox.Items.Add(_comment.Type);

                    _canEditType = true;
                }
            }
            else
            {
                if (_comment.Type != null)
                    typeComboBox.Items.Add(_comment.Type);
            }
            typeComboBox.SelectedItem = _comment.Type;

            isOpenCheckBox.IsChecked = _comment.IsOpen;
            colorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_comment.Color);
            userNameTextBox.Text = _comment.UserName;
            subjectTextBox.Text = _comment.Subject;
            textBox.Text = _comment.Text;

            isLockedCheckBox.IsChecked = _comment.IsReadOnly;
        }

        /// <summary>
        /// Updates the comment modify date time.
        /// </summary>
        private void UpdateModifyDateTime()
        {
            if (_comment.ModifyDate != DateTime.MinValue)
                modifyDateTimeTextBox.Text = _comment.ModifyDate.ToString();
            else
                modifyDateTimeTextBox.Text = string.Empty;
        }
#endif

        /// <summary>
        /// Updates the user interface of tab control.
        /// </summary>
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN            
            if (e.OriginalSource is TabControl)
            {
                if (tabPageCommon.IsSelected)
                    UpdateCommonTabItem();
                else if (tabPageAdvanced.IsSelected)
                    commentDataPropertyGrid.Refresh();
            }
#endif
        }

        /// <summary>
        /// Changes the <see cref="Comment.IsReadOnly"/> property.
        /// </summary>
        private void isLockedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            _comment.IsReadOnly = isLockedCheckBox.IsChecked.Value == true;

            UpdateUI();
            UpdateModifyDateTime();
#endif
        }

        /// <summary>
        /// Changes the <see cref="Comment.IsOpen"/> property.
        /// </summary>
        private void isOpenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            _comment.IsOpen = isOpenCheckBox.IsChecked.Value == true;
#endif        
        }

        /// <summary>
        /// Changes the <see cref="Comment.Color"/> property.
        /// </summary>
        private void colorPanelControl_ColorChanged(object sender, EventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            _comment.Color = WpfObjectConverter.CreateDrawingColor(colorPanelControl.Color);
            UpdateModifyDateTime();
#endif        
        }

        /// <summary>
        /// Changes the <see cref="Comment.Type"/> property.
        /// </summary>
        private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (_comment == null || typeComboBox.SelectedItem == null)
                return;

            _comment.Type = (string)typeComboBox.SelectedItem;
            UpdateModifyDateTime();
#endif        
        }

        /// <summary>
        /// Changes the <see cref="Comment.Type"/> property.
        /// </summary>
        private void TypeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (_comment == null)
                return;

            if (!string.Equals(_comment.Type, typeComboBox.Text))
            {
                _comment.Type = typeComboBox.Text;
                UpdateModifyDateTime();
            }
#endif
        }

        /// <summary>
        /// Changes the <see cref="Comment.UserName"/> property.
        /// </summary>
        private void userNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (_comment == null)
                return;

            _comment.UserName = userNameTextBox.Text;
            UpdateModifyDateTime();
#endif        
        }

        /// <summary>
        /// Changes the <see cref="Comment.Subject"/> property.
        /// </summary>
        private void subjectTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (_comment == null)
                return;

            _comment.Subject = subjectTextBox.Text;
            UpdateModifyDateTime();
#endif
        }

        /// <summary>
        /// Changes the <see cref="Comment.Text"/> property.
        /// </summary>
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (_comment == null)
                return;

            _comment.Text = textBox.Text;
            UpdateModifyDateTime();
#endif        
        }

        /// <summary>
        /// Closes the property window.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

    }
}
