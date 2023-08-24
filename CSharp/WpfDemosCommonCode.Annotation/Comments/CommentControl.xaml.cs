#if REMOVE_ANNOTATION_PLUGIN
#error Remove CommentControl from the project.
#else
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.UI.Comments;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Represents a control that allows to display a comment in image viewer.
    /// </summary>
    /// <seealso cref="CommentTools"/>
    public partial class CommentControl : UserControl, ICommentControl
    {

        #region Constants

        /// <summary>
        /// The maximum count of nested replies.
        /// </summary>
        /// <remarks>
        /// This limitation is added because WPF thrown exception if 255 or more visuals are nested.
        /// </remarks>
        public const int MaxReplyNestingCount = 20;

        /// <summary>
        /// The opacity of selected comment.
        /// </summary>
        public const double SelectedCommentOpacity = 1;

        /// <summary>
        /// The opacity of not selected comment.
        /// </summary>
        public const double NotSelectedCommentOpacity = 0.75;

        /// <summary>
        /// The resource file name format.
        /// </summary>
        const string ResourceFileNameFormat = "WpfDemosCommonCode.Annotation.Comments.Resources.{0}.png";

        /// <summary>
        /// The reply margin in pixels.
        /// </summary>
        const int ReplyMargin = 2;

        /// <summary>
        /// The states panel height in pixels.
        /// </summary>
        const int StatesPanelHeight = 28;

        #endregion



        #region Fields

        /// <summary>
        /// The expanded button image.
        /// </summary>
        static BitmapSource ExpandedImage;

        /// <summary>
        /// The collapsed button image.
        /// </summary>
        static BitmapSource CollapsedImage;

        /// <summary>
        /// Dictionary: "Comment state" => "Icon image".
        /// </summary>
        static Dictionary<string, BitmapSource> CommentStateImages;


        /// <summary>
        /// Dictionary: "State name" => "Menu item".
        /// </summary>
        Dictionary<string, MenuItem> _commentStateNameToMenuItem;

        /// <summary>
        /// Dictionary: "State comment" => "Label".
        /// </summary>
        Dictionary<Comment, Label> _commentStateToControl = new Dictionary<Comment, Label>();

        /// <summary>
        /// A value indicating whether UI is updating.
        /// </summary>
        bool _isUiUpdating = false;

        /// <summary>
        /// A value indicating whether control size is updating.
        /// </summary>
        bool _isSizeUpdating = false;

        /// <summary>
        /// The control transformer.
        /// </summary>
        CommentControlTransformer _transformer = null;

        /// <summary>
        /// The current states of this comment.
        /// </summary>
        Comment[] _currentStates = null;

        /// <summary>
        /// A value indicating whether the reply is added.
        /// </summary>
        bool _isReplyAdded = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="CommentControl"/> class.
        /// </summary>
        static CommentControl()
        {
            ExpandedImage = CreateBitmapSource("CommentExpand");
            CollapsedImage = CreateBitmapSource("CommentCollapse");

            string[] states = new string[] {
                "ReviewAccepted", "ReviewRejected", "ReviewCancelled", "ReviewCompleted", "ReviewNone"
            };
            CommentStateImages = new Dictionary<string, BitmapSource>();
            foreach (string state in states)
                CommentStateImages.Add(state, CreateBitmapSource("CommentState_" + state));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentControl"/> class.
        /// </summary>
        public CommentControl()
        {
            InitializeComponent();

            closeButton.Content = CreateImageForButton("CommentClose");
            lockIconLabel.Content = CreateImageForButton("CommentLock");
            settingsButton.Content = CreateImageForButton("CommentSettings");

            _commentStateNameToMenuItem = new Dictionary<string, MenuItem>();
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewAccepted, reviewAcceptedMenuItem);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewRejected, reviewRejectedMenuItem);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewCancelled, reviewCancelledMenuItem);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewCompleted, reviewCompletedMenuItem);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewNone, reviewNoneMenuItem);
            foreach (string stateName in _commentStateNameToMenuItem.Keys)
                _commentStateNameToMenuItem[stateName].Tag = stateName;

            showStateHistoryMenuItem.IsChecked = ShowStateHistory;
            AutoHeight = false;
            CanExpand = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentControl"/> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public CommentControl(Comment comment)
            : this()
        {
            Comment = comment;
        }

        #endregion



        #region Properties

        bool _autoHeight = false;
        /// <summary>
        /// Gets or sets a value indicating whether control has automatic height.
        /// </summary>
        public bool AutoHeight
        {
            get
            {
                return _autoHeight;
            }
            set
            {
                if (_autoHeight != value)
                {
                    _autoHeight = value;
                    scrollViewer.VerticalScrollBarVisibility = value ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Visible;
                    if (value)
                    {
                        SetSize();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control can be closed.
        /// </summary>
        public bool CanClose
        {
            get
            {
                return closeButton.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                    closeButton.Visibility = Visibility.Visible;
                else
                    closeButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control can be expanded.
        /// </summary>
        public bool CanExpand
        {
            get
            {
                return expandButton.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                    expandButton.Visibility = Visibility.Visible;
                else
                    expandButton.Visibility = Visibility.Collapsed;

                SetSize();
            }
        }

        bool _showStateHistory = false;
        /// <summary>
        /// Gets or sets a value indicating whether states must be shown in replies.
        /// </summary>
        public bool ShowStateHistory
        {
            get
            {
                return _showStateHistory;
            }
            set
            {
                if (_showStateHistory != value)
                {
                    _showStateHistory = value;
                    UpdateReplies();
                    foreach (CommentControl reply in repliesStackPanel.Children)
                        reply.ShowStateHistory = value;
                    SetSize();
                }
            }
        }


        Comment _comment;
        /// <summary>
        /// Gets or sets the comment that is displayed in control.
        /// </summary>
        public Comment Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_comment != value)
                {
                    if (_comment != null)
                    {
                        _comment.PropertyChanged -= Comment_PropertyChanged;
                        if (_comment.Replies != null)
                            _comment.Replies.Changed -= CommentReplies_Changed;
                        _comment.StatesChanged -= Comment_StatesChanged;
                        ClearRepliesPanel();
                    }

                    _comment = value;

                    if (_comment != null)
                    {
                        _comment.PropertyChanged += Comment_PropertyChanged;
                        if (_comment.Replies != null)
                            _comment.Replies.Changed += CommentReplies_Changed;
                        _comment.StatesChanged += Comment_StatesChanged;
                    }

                    UpdateUI();
                    UpdateReplies();
                    SetSize();
                }
            }
        }

        bool _isCommentSelected = false;
        /// <summary>
        /// Gets or sets a value indicating whether the comment is selected.
        /// </summary>
        /// <value>
        /// <b>true</b> if the comment is selected; otherwise, <b>false</b>.
        /// </value>
        public bool IsCommentSelected
        {
            get
            {
                return _isCommentSelected;
            }
            set
            {
                if (_isCommentSelected != value)
                {
                    _isCommentSelected = value;

                    UpdateOpacity();

                    OnIsCommentSelectedChanged(this, new PropertyChangedEventArgs<bool>(!value, value));

                    if (value)
                    {
                        CommentControl parenctCommentControl = GetParentCommentControl();
                        if (parenctCommentControl != null)
                            parenctCommentControl.IsCommentSelected = true;
                    }
                    else
                    {
                        foreach (UIElement element in repliesStackPanel.Children)
                        {
                            if (element is CommentControl)
                                ((CommentControl)element).IsCommentSelected = false;
                        }
                    }
                }
            }
        }

        bool _needSetFocusWhenLoaded = false;
        /// <summary>
        /// Gets or sets a value indicating whether control must get focus when control is loaded.
        /// </summary>
        public bool NeedSetFocusWhenLoaded
        {
            get
            {
                return _needSetFocusWhenLoaded;
            }
            set
            {
                _needSetFocusWhenLoaded = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Selects the comment.
        /// </summary>
        public void SelectComment()
        {
            IsCommentSelected = true;
        }

        /// <summary>
        /// Searches the comment control that displays specified comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns>
        /// Comment control that diplays specified comment.
        /// </returns>
        public ICommentControl FindCommentControl(Comment comment)
        {
            if (Comment == comment)
                return this;
            foreach (ICommentControl control in repliesStackPanel.Children)
            {
                ICommentControl result = control.FindCommentControl(comment);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:PreviewMouseDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (Comment.IsOpen)
                SelectComment();

            base.OnPreviewMouseDown(e);
        }

        /// <summary>
        /// Called when the visual parent is changed.
        /// </summary>
        /// <param name="oldParent">The old parent.</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            // remove old transformer
            if (_transformer != null)
            {
                _transformer.Dispose();
                _transformer = null;
            }

            // find image viewer
            WpfImageViewer imageViewer = WpfImageViewer.GetImageViewer(this);
            // if this control is located on image viewer
            if (imageViewer != null)
            {
                if (GetCommentControl(this.Parent) == null)
                {
                    _transformer = new CommentControlTransformer(this);

                    UpdateOpacity();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:IsCommentSelectedChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs{T}"/> instance containing the event data.</param>
        protected virtual void OnIsCommentSelectedChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (IsCommentSelectedChanged != null)
                IsCommentSelectedChanged(sender, e);
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Sets size of control.
        /// </summary>
        internal void SetSize()
        {
            _isSizeUpdating = true;

            if (Comment == null)
            {
                textTextBox.Visibility = Visibility.Visible;
                bottomGrid.Visibility = Visibility.Visible;
            }
            else
            {
                if (Comment.IsReadOnly && string.IsNullOrEmpty(textTextBox.Text))
                    textTextBox.Visibility = Visibility.Collapsed;
                else
                    textTextBox.Visibility = Visibility.Visible;

                if (CanExpand && !Comment.IsOpen)
                    bottomGrid.Visibility = Visibility.Collapsed;
                else
                    bottomGrid.Visibility = Visibility.Visible;
            }

            _isSizeUpdating = false;
        }


        /// <summary>
        /// Updates the comment value.
        /// </summary>
        private void UpdateValueUI()
        {
            textTextBox.Text = Comment.Text;
            textTextBox.IsReadOnly = Comment.IsReadOnly;
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        internal void UpdateUI()
        {
            _isUiUpdating = true;
            if (Comment != null)
            {
                if (Comment.IsOpen)
                    expandButton.Content = CreateImageForButton(CollapsedImage);
                else
                    expandButton.Content = CreateImageForButton(ExpandedImage);

                UpdateHeaderUI();

                if (Comment.ModifyDate != DateTime.MinValue)
                    modifyDateLabel.Content = Comment.ModifyDate.ToString();
                else
                    modifyDateLabel.Content = "";

                UpdateStatesUI();

                UpdateValueUI();

                Color commentControlBackgroundColor;
                if (Comment.Color.ToArgb() == System.Drawing.Color.Black.ToArgb())
                    commentControlBackgroundColor = Colors.Gray;
                else
                    commentControlBackgroundColor = GetLightenColor(WpfObjectConverter.CreateWindowsColor(Comment.Color));
                topGrid.Background = new SolidColorBrush(commentControlBackgroundColor);

                if (Comment.IsReadOnly)
                {
                    replyMenuItem.IsEnabled = false;
                    removeMenuItem.IsEnabled = false;
                }
                else
                {
                    replyMenuItem.IsEnabled = true;
                    removeMenuItem.IsEnabled = true;
                }

                if (GetCommentNestingCount(Comment) >= MaxReplyNestingCount)
                    replyMenuItem.IsEnabled = false;
            }
            _isUiUpdating = false;
        }

        #endregion


        #region PRIVATE

        #region Buttons

        /// <summary>
        /// Closes this control.
        /// </summary>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Comment.IsOpen = false;
            IsCommentSelected = false;
        }

        /// <summary>
        /// Opens this control.
        /// </summary>
        private void expandButton_Click(object sender, RoutedEventArgs e)
        {
            Comment.IsOpen = !Comment.IsOpen;
            IsCommentSelected = Comment.IsOpen;
        }

        /// <summary>
        /// Shows context menu.
        /// </summary>
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            commentContextMenu.IsOpen = true;
            SelectComment();
        }

        /// <summary>
        /// Shows comment state history.
        /// </summary>
        private void stateHistoryToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CommentStateHistoryWindow dlg = new CommentStateHistoryWindow(Comment);
            dlg.Owner = Window.GetWindow(this);
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.ShowDialog();
        }

        /// <summary>
        /// Shows comment properties.
        /// </summary>
        private void propertiesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CommentPropertiesWindow dlg = new CommentPropertiesWindow(Comment);
            dlg.Owner = Window.GetWindow(this);
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.ShowDialog();
            UpdateUI();
        }

        /// <summary>
        /// Removes comment.
        /// </summary>
        private void removeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Comment.Remove();
        }


        /// <summary>
        /// Adds reply to this comment.
        /// </summary>
        private void replyToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _isReplyAdded = true;
            Comment.AddReply(System.Drawing.Color.Yellow, GetCurrentUserName());
        }

        /// <summary>
        /// Expands all replies.
        /// </summary>
        private void expandAllToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Comment.Expand();
        }

        /// <summary>
        /// Collapses all replies.
        /// </summary>
        private void collapseRepliesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (Comment reply in Comment.Replies)
                reply.Collapse();
        }

        /// <summary>
        /// Resets comment location.
        /// </summary>
        private void resetLocationToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Comment.ResetBoundingBox();
        }

        /// <summary>
        /// Collapses all comments but this.
        /// </summary>
        private void collapseAllButThisToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CommentCollection parentCollection = Comment.ParentCollection;
            while (parentCollection != null)
            {
                foreach (Comment rootComment in parentCollection)
                {
                    if (rootComment.FindCommentBySource(Comment.Source) == null)
                        rootComment.IsOpen = false;
                }
                if (parentCollection.Parent == null)
                    break;
                parentCollection = parentCollection.Parent.ParentCollection;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the showStateHistoryToolStripMenuItem control.
        /// </summary>
        private void showStateHistoryToolStripMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ShowStateHistory = showStateHistoryMenuItem.IsChecked;
        }

        /// <summary>
        /// Sets the state state of this comment.
        /// </summary>
        private void setStateToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string stateName = (string)menuItem.Tag;

            string[] parsedName = stateName.Split('.');
            string stateModel = parsedName[0];
            string state = parsedName[1];

            Comment stateComment = Comment.SetState(
                System.Drawing.Color.Yellow,
                GetCurrentUserName(),
                stateModel, state,
                CommentTools.SplitStatesByUserName);
            stateComment.IsOpen = false;
            stateComment.Text = string.Format("{0} sets by {1}", state, stateComment.UserName);
        }

        /// <summary>
        /// Updates the comment control opacity.
        /// </summary>
        private void UpdateOpacity()
        {
            if (WpfImageViewer.GetImageViewer(this) != null)
            {
                if (GetParentCommentControl() == null)
                {
                    if (IsCommentSelected)
                        Opacity = SelectedCommentOpacity;
                    else
                        Opacity = NotSelectedCommentOpacity;
                }
                else
                {
                    Opacity = 1;
                }
            }
        }

        #endregion


        #region Event handlers

        /// <summary>
        /// Handles the Comment.PropertyChanged event.
        /// </summary>
        private void Comment_PropertyChanged(object sender, Vintasoft.Imaging.ObjectPropertyChangedEventArgs e)
        {
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(new EventHandler<ObjectPropertyChangedEventArgs>(Comment_PropertyChanged), sender, e);
            }
            else
            {
                if (e.PropertyName == "IsOpen")
                {
                    SetSize();
                    UpdateHeaderUI();
                    UpdateValueUI();
                    if (CanExpand)
                    {
                        if ((bool)e.NewValue)
                            expandButton.Content = CreateImageForButton(CollapsedImage);
                        else
                            expandButton.Content = CreateImageForButton(ExpandedImage);
                    }

                    IsCommentSelected = (bool)e.NewValue;
                }
                else if (e.PropertyName == "BoundingBox")
                {
                }
                else
                {
                    if (e.PropertyName == "IsReadOnly")
                        SetSize();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Handles the ReplyControl.SizeChanged event.
        /// </summary>
        private void ReplyControl_SizeChanged(object sender, EventArgs e)
        {
            if (!_isSizeUpdating)
                SetSize();
        }

        /// <summary>
        /// Handles the TextChanged event of the textTextBox control.
        /// </summary>
        private void textTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUiUpdating)
                Comment.Text = textTextBox.Text;
        }

        /// <summary>
        /// Sets the keyboard focus to the text box.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (NeedSetFocusWhenLoaded)
                textTextBox.Focus();
        }

        #endregion


        #region UI

        /// <summary>
        /// Clears the replies panel.
        /// </summary>
        private void ClearRepliesPanel()
        {
            UIElement[] replies = new UIElement[repliesStackPanel.Children.Count];
            repliesStackPanel.Children.CopyTo(replies, 0);
            repliesStackPanel.Children.Clear();
            foreach (UIElement control in replies)
                if (control is CommentControl)
                    ((CommentControl)control).Comment = null;
        }

        /// <summary>
        /// Updates the replies panel.
        /// </summary>
        private void UpdateReplies()
        {
            ClearRepliesPanel();
            if (Comment != null && Comment.Replies != null)
            {
                foreach (Comment reply in Comment.Replies)
                {
                    if (ShowStateHistory || string.IsNullOrEmpty(reply.ParentState))
                    {
                        AddReplyControl(reply);
                    }
                }
                UpdateReplyMargins();
            }
        }

        /// <summary>
        /// Updates the reply margins.
        /// </summary>
        private void UpdateReplyMargins()
        {
            UIElementCollection repliesControls = repliesStackPanel.Children;
            if (repliesControls.Count > 0)
            {
                for (int i = 1; i < repliesControls.Count - 1; i++)
                    ((FrameworkElement)repliesControls[i]).Margin = new Thickness(0, ReplyMargin, 0, ReplyMargin);

                ((FrameworkElement)repliesControls[0]).Margin = new Thickness(0, 0, 0, ReplyMargin);
                ((FrameworkElement)repliesControls[repliesControls.Count - 1]).Margin = new Thickness(0, ReplyMargin, 0, 0);
            }
        }

        /// <summary>
        /// Updates control header UI.
        /// </summary>
        private void UpdateHeaderUI()
        {
            lockIconLabel.Visibility = Comment.IsReadOnly ? Visibility.Visible : Visibility.Collapsed;
            if (string.IsNullOrEmpty(Comment.ParentState))
            {
                // if comment is open
                if (Comment.IsOpen)
                {
                    userNameLabel.Content = Comment.UserName;
                }
                // if comment is closed
                else
                {
                    // get all replies without states
                    Comment[] replies = Comment.GetAllReplies(false);
                    if (replies.Length > 0)
                        userNameLabel.Content = string.Format("{0} ({1} replies)", Comment.UserName, replies.Length);
                    else
                        userNameLabel.Content = Comment.UserName;
                }
                if (!string.IsNullOrEmpty(Comment.Subject) && Comment.Subject != Comment.UserName)
                    nameLabel.Content = Comment.Subject;
                else
                    nameLabel.Content = Comment.Type;
            }
            else
            {
                // comment state
                if (string.IsNullOrEmpty(Comment.StateModel))
                    userNameLabel.Content = Comment.UserName;
                else
                    userNameLabel.Content = string.Format("{0} ({1})", Comment.UserName, Comment.StateModel);
                nameLabel.Content = Comment.ParentState;
            }
        }

        /// <summary>
        /// Updates the states UI.
        /// </summary>
        private void UpdateStatesUI()
        {
            bool isComment = string.IsNullOrEmpty(Comment.ParentState);
            reviewMenuItem.Visibility = isComment ? Visibility.Visible : Visibility.Collapsed;
            stateHistoryMenuItem.Visibility = isComment ? Visibility.Visible : Visibility.Collapsed;

            Comment[] states = null;
            if (string.IsNullOrEmpty(Comment.ParentState))
                states = Comment.GetStates(CommentTools.SplitStatesByUserName);

            foreach (MenuItem item in _commentStateNameToMenuItem.Values)
            {
                item.IsEnabled = !Comment.IsReadOnly;
                item.IsChecked = false;
            }
            if (states != null)
            {
                foreach (Comment state in states)
                {
                    if (!CommentTools.SplitStatesByUserName || state.UserName == GetCurrentUserName())
                    {
                        MenuItem item = null;
                        string stateName = string.Format("{0}.{1}", state.StateModel, state.ParentState);
                        if (_commentStateNameToMenuItem.TryGetValue(stateName, out item))
                            item.IsChecked = true;
                    }
                }
            }

            if (_currentStates != null)
            {
                foreach (Comment state in _currentStates)
                    UnsubscribeFromStateEvent(state);
            }
            _currentStates = states;

            bool isHeightChanged = false;
            if (states == null)
            {
                statesStackPanel.Children.Clear();
                _commentStateToControl.Clear();
                if (statesStackPanel.ActualHeight != 0)
                {
                    isHeightChanged = true;
                    statesStackPanel.Height = 0;
                }
            }
            else
            {
                if (statesStackPanel.ActualHeight != StatesPanelHeight)
                {
                    statesStackPanel.Height = StatesPanelHeight;
                    isHeightChanged = true;
                }
                Comment[] oldStates = new Comment[_commentStateToControl.Count];
                _commentStateToControl.Keys.CopyTo(oldStates, 0);

                foreach (Comment oldState in oldStates)
                {
                    if (Array.IndexOf(states, oldState) < 0)
                    {
                        Label oldStateControl = _commentStateToControl[oldState];
                        statesStackPanel.Children.Remove(oldStateControl);
                        _commentStateToControl.Remove(oldState);
                    }
                }
                foreach (Comment state in states)
                {
                    ShowState(state);
                    SubscribeToStateEvent(state);
                }
            }

            if (isHeightChanged)
                SetSize();
        }

        /// <summary>
        /// Subscribes to the state events.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SubscribeToStateEvent(Comment state)
        {
            state.PropertyChanged += State_PropertyChanged;
        }

        /// <summary>
        /// Unsubscribe from the state events.
        /// </summary>
        /// <param name="state">The state.</param>
        private void UnsubscribeFromStateEvent(Comment state)
        {
            state.PropertyChanged -= State_PropertyChanged;
        }

        /// <summary>
        /// The state property is changed.
        /// </summary>
        private void State_PropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, "StateModel", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(e.PropertyName, "ParentState", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(e.PropertyName, "UserName", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(e.PropertyName, "ModifyDate", StringComparison.InvariantCultureIgnoreCase))
            {
                bool isPropertyRemoved = false;

                if (e.NewValue == null ||
                    e.NewValue is string && string.IsNullOrEmpty((string)e.NewValue) ||
                    e.NewValue is DateTime && DateTime.MinValue == (DateTime)e.NewValue)
                    isPropertyRemoved = true;

                if (isPropertyRemoved)
                    UpdateStatesUI();
                else
                    ShowState(((Comment)sender));
            }
        }

        /// <summary>
        /// Shows the state in UI.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ShowState(Comment state)
        {
            Label stateControl = null;
            if (!_commentStateToControl.TryGetValue(state, out stateControl))
            {
                stateControl = new Label();

                _commentStateToControl.Add(state, stateControl);
                statesStackPanel.Children.Add(stateControl);
            }

            if (!UpdateStateControl(stateControl, state))
            {
                _commentStateToControl.Remove(state);
                statesStackPanel.Children.Remove(stateControl);
            }
        }

        /// <summary>
        /// Updates the state control.
        /// </summary>
        /// <param name="stateLabel">The state label.</param>
        /// <param name="state">The state.</param>
        private bool UpdateStateControl(Label stateLabel, Comment state)
        {
            bool result = false;

            string fullState = string.Format("{0}{1}", state.StateModel, state.ParentState);
            // if comment has image for comment state
            if (CommentStateImages.ContainsKey(fullState))
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                Image image = new Image();
                image.Source = CommentStateImages[fullState];
                image.Stretch = Stretch.None;
                stackPanel.Children.Add(image);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = state.UserName;
                textBlock.Margin = new Thickness(3, 0, 0, 0);
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(textBlock);

                stateLabel.Content = stackPanel;

                result = !string.IsNullOrEmpty(state.UserName);
            }
            else
            {
                stateLabel.Content = string.Format("{0}: {1}", state.ParentState, state.UserName);

                result = !string.IsNullOrEmpty((string)stateLabel.Content);
            }
            stateLabel.ToolTip = string.Format("{0}, {1}: {2} ({3})",
                state.UserName, state.StateModel, state.ParentState, state.ModifyDate.ToString());

            return result;
        }

        #endregion


        #region Tools

        /// <summary>
        /// Returns the parent comment control of this control.
        /// </summary>
        private CommentControl GetParentCommentControl()
        {
            DependencyObject currentControl = this.Parent;
            while (currentControl != null)
            {
                if (currentControl is CommentControl)
                {
                    return (CommentControl)currentControl;
                }

                currentControl = VisualTreeHelper.GetParent(currentControl);
            }
            return null;
        }

        /// <summary>
        /// Adds the reply control to the reply panel.
        /// </summary>
        /// <param name="reply">The reply.</param>
        private CommentControl AddReplyControl(Comment reply)
        {
            if (GetCommentNestingCount(reply) > MaxReplyNestingCount)
                return null;

            CommentControl replyControl = new CommentControl(reply);

            replyControl.Focusable = true;

            replyControl.ShowStateHistory = ShowStateHistory;
            replyControl.AutoHeight = true;
            replyControl.CanClose = false;
            replyControl.CanExpand = true;
            replyControl.SizeChanged += ReplyControl_SizeChanged;
            repliesStackPanel.Children.Add(replyControl);

            return replyControl;
        }


        /// <summary>
        /// Returns the name of the current user.
        /// </summary>
        /// <returns>The name of the current user.</returns>
        private string GetCurrentUserName()
        {
            return Environment.UserName;
        }


        /// <summary>
        /// Returns a color that is lighter than specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>Color that is lighter than specified color.</returns>
        private static Color GetLightenColor(Color color)
        {
            return Color.FromArgb(
                255,
                (byte)Math.Min(255, color.R + 96),
                (byte)Math.Min(255, color.G + 96),
                (byte)Math.Min(255, color.B + 96));
        }

        /// <summary>
        /// Handles the Comment.StatesChanged event.
        /// </summary>
        private void Comment_StatesChanged(object sender, EventArgs e)
        {
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(new EventHandler(Comment_StatesChanged), sender, e);
            }
            else
            {
                UpdateStatesUI();
            }
        }

        /// <summary>
        /// Handles the Comment.Replies.Changed event.
        /// </summary>
        private void CommentReplies_Changed(object sender, CollectionChangeEventArgs<Comment> e)
        {
            if (Comment == null || Comment.Source == null)
                return;

            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(new CollectionChangeEventHandler<Comment>(CommentReplies_Changed), sender, e);
            }
            else
            {
                // if reply was removed
                if (e.Action == CollectionChangeActionType.RemoveItem)
                {
                    // remove reply control
                    ICommentControl control = FindCommentControl((Comment)e.OldValue);
                    if (control != null)
                    {
                        repliesStackPanel.Children.Remove((Control)control);
                        control.Comment = null;
                        SetSize();
                    }
                }
                // if reply was added
                else if (e.Action == CollectionChangeActionType.InsertItem)
                {
                    Comment newComment = (Comment)e.NewValue;
                    // if new reply must be displayed
                    if (ShowStateHistory || string.IsNullOrEmpty(newComment.ParentState))
                    {
                        // add reply control
                        CommentControl replyControl = AddReplyControl(newComment);
                        if (replyControl != null)
                        {
                            if (_isReplyAdded)
                            {
                                _isReplyAdded = false;
                                replyControl.NeedSetFocusWhenLoaded = true;
                            }
                            UpdateReplyMargins();
                            SetSize();
                            if (!Comment.IsOpen)
                                Comment.IsOpen = true;

                            SelectComment();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="BitmapSource"/> of specified resource name.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>
        /// The <see cref="BitmapSource"/>.
        /// </returns>
        private static BitmapSource CreateBitmapSource(string resourceName)
        {
            string filename = string.Format(ResourceFileNameFormat, resourceName);
            return DemosResourcesManager.GetResourceAsBitmap(filename);
        }

        /// <summary>
        /// Creates the <see cref="Image"/> of specified resource name.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        private static Image CreateImageForButton(string resourceName)
        {
            return CreateImageForButton(CreateBitmapSource(resourceName));
        }

        /// <summary>
        /// Creates the <see cref="Image"/> of specified <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="bitmapSource">The bitmap.</param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        private static Image CreateImageForButton(BitmapSource bitmapSource)
        {
            Image image = new Image();
            image.Source = bitmapSource;
            image.Stretch = Stretch.None;
            image.Width = 16;
            image.Height = 16;
            return image;
        }

        /// <summary>
        /// Returns the <see cref="CommentControl"/> where the <paramref name="dependencyObject"/> is located.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        private CommentControl GetCommentControl(DependencyObject dependencyObject)
        {
            DependencyObject currentObject = dependencyObject;

            while (currentObject != null)
            {
                if (currentObject is CommentControl)
                    return (CommentControl)currentObject;

                currentObject = VisualTreeHelper.GetParent(currentObject);
            }

            return null;
        }

        /// <summary>
        /// Returns the nesting count of specified comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns>
        /// The nesting count.
        /// </returns>
        private int GetCommentNestingCount(Comment comment)
        {
            int count = 0;
            Comment currentComment = comment;

            while (currentComment != null)
            {
                count++;
                currentComment = currentComment.Parent;
            }

            return count;
        }

        #endregion

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when the property <see cref="P:Vintasoft.Imaging.Annotation.UI.Comments.ICommentControl.IsCommentSelected" /> is changed.
        /// </summary>
        public event PropertyChangedEventHandler<bool> IsCommentSelectedChanged;

        #endregion

    }
}
#endif