using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Undo;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that shows an action list of undo manager.
    /// </summary>
    public partial class WpfUndoManagerHistoryWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfUndoManagerHistoryWindow"/> class.
        /// </summary>
        /// <param name="ownerWindow">The owner window.</param>
        /// <param name="undoManager">The undo manager.</param>
        public WpfUndoManagerHistoryWindow(Window ownerWindow, UndoManager undoManager)
        {
            InitializeComponent();

            this.Owner = ownerWindow;
            this.UndoManager = undoManager;
        }

        #endregion



        #region Properties

        UndoManager _undoManager;
        /// <summary>
        /// Gets or sets the undo manager.
        /// </summary>
        public UndoManager UndoManager
        {
            get
            {
                return _undoManager;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (_undoManager == value)
                    return;

                // unsubscribe from events of previous history manager
                UnsubscribeFromUndoManagerEvents(_undoManager);

                // set a reference to new history manager
                _undoManager = value;
                // subscribe to events of new history manager
                SubscribeToUndoManagerEvents(_undoManager);

                // update the listbox with descriptions of history action
                UpdateHistoryListBox();

                // select current action in the ist box of action descriptions
                undoManagerHistoryListBox.SelectedIndex = _undoManager.CurrentActionIndex;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation in history is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if navigate in history is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool CanNavigateOnHistory
        {
            get
            {
                return undoManagerHistoryListBox.IsEnabled;
            }
            set
            {
                undoManagerHistoryListBox.IsEnabled = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Form is closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_undoManager != null)
                UnsubscribeFromUndoManagerEvents(_undoManager);
        }


        /// <summary>
        /// Subscribes to the events of history manager.
        /// </summary>
        private void SubscribeToUndoManagerEvents(UndoManager undoManager)
        {
            undoManager.Changed += new EventHandler<UndoManagerChangedEventArgs>(undoManager_Changed);
            undoManager.Navigated += new EventHandler<UndoManagerNavigatedEventArgs>(undoManager_Navigated);
        }

        /// <summary>
        /// Unsubscribes from the events of history manager.
        /// </summary>
        private void UnsubscribeFromUndoManagerEvents(UndoManager undoManager)
        {
            if (undoManager == null)
                return;

            undoManager.Changed -= new EventHandler<UndoManagerChangedEventArgs>(undoManager_Changed);
            undoManager.Navigated -= new EventHandler<UndoManagerNavigatedEventArgs>(undoManager_Navigated);
        }

        /// <summary>
        /// Updates the listbox with descriptions of history action.
        /// </summary>
        private void UpdateHistoryListBox()
        {
            undoManagerHistoryListBox.Items.Clear();

            int i = 0;
            foreach (Vintasoft.Imaging.Undo.UndoAction action in _undoManager.GetActions())
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = GetUndoActionDescription(action);
                item.Name = string.Format("listBoxItem{0}", i++);
                undoManagerHistoryListBox.Items.Add(item);
            }

            undoManagerHistoryListBox.SelectedIndex = _undoManager.CurrentActionIndex;
            undoManagerHistoryListBox.ScrollIntoView(undoManagerHistoryListBox.SelectedItem);
        }

        /// <summary>
        /// Returns a description of undo action.
        /// </summary>
        /// <returns>
        /// A description of undo action.
        /// </returns>
        private string GetUndoActionDescription(Vintasoft.Imaging.Undo.UndoAction action)
        {
            return action.ToString();
        }

        /// <summary>
        /// History is changed.
        /// </summary>
        private void undoManager_Changed(object sender, UndoManagerChangedEventArgs e)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
                UpdateHistoryListBox();
            else
                Dispatcher.Invoke(new UpdateHistoryListBoxDelegate(UpdateHistoryListBox));
        }

        /// <summary>
        /// Current action in history is changed.
        /// </summary>
        private void undoManager_Navigated(object sender, UndoManagerNavigatedEventArgs e)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
                SetSelectedIndexImageProcessingHistoryListBox();
            else
                Dispatcher.Invoke(new UpdateHistoryListBoxDelegate(SetSelectedIndexImageProcessingHistoryListBox));
        }

        /// <summary>
        /// Selects the current undo action in list box.
        /// </summary>
        private void SetSelectedIndexImageProcessingHistoryListBox()
        {
            if (undoManagerHistoryListBox.SelectedIndex == _undoManager.CurrentActionIndex)
                return;

            undoManagerHistoryListBox.SelectedIndex = _undoManager.CurrentActionIndex;
        }

        /// <summary>
        /// Index of current action is changed.
        /// </summary>
        private void undoManagerHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            {
                int newHistoryIndex = undoManagerHistoryListBox.SelectedIndex;
                if (newHistoryIndex == _undoManager.CurrentActionIndex)
                    return;

                if (newHistoryIndex > _undoManager.CurrentActionIndex)
                    _undoManager.Redo(newHistoryIndex - _undoManager.CurrentActionIndex);
                else
                    _undoManager.Undo(_undoManager.CurrentActionIndex - newHistoryIndex);
            }
        }

        #endregion



        #region Delegates

        delegate void UpdateHistoryListBoxDelegate();

        #endregion

    }
}
