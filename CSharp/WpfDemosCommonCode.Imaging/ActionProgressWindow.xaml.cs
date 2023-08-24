using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Vintasoft.Imaging.Utils;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A form that executes a method in a background thread and shows the execution progress.
    /// </summary>
    public partial class ActionProgressWindow : Window
    {

        #region Classes

        /// <summary>
        /// Custom action progress handler that sets captions and progress bar values.
        /// </summary>
        private class ActionProgressHandler : IActionProgressHandler
        {

            #region Fields

            /// <summary>
            /// Progress bars on a form.
            /// </summary>
            ProgressBar[] _progressBars;

            /// <summary>
            /// Labels on a form.
            /// </summary>
            Label[] _labels;

            /// <summary>
            /// Determines whether action cancellation is requested.
            /// </summary>
            bool _cancelRequested = false;

            TextBox _logTextBox;

            Dictionary<int, string> _levelToMessage = new Dictionary<int, string>();

            #endregion



            #region Constructors

            internal ActionProgressHandler(ProgressBar[] progressBars, Label[] labels, TextBox logTextBox)
                : base()
            {
                _progressBars = progressBars;
                _labels = labels;
                _logTextBox = logTextBox;
            }

            #endregion



            #region Methods

            /// <summary>
            /// Resets this action progress controller.
            /// </summary>
            public void Reset()
            {
            }

            /// <summary>
            /// Called when action step is changed.
            /// </summary>
            /// <param name="actionProgressController">The action progress controller.</param>
            /// <param name="actionStep">The action step.</param>
            /// <param name="canCancel">Indicates that action can be canceled.</param>
            /// <returns>
            /// <b>False</b> if action is canceled; otherwise <b>true</b>.
            /// </returns>
            public bool OnActionStep(
                ActionProgressController actionProgressController,
                double actionStep,
                bool canCancel)
            {
                if (canCancel && _cancelRequested)
                    return false;

                // action description
                string actionDescription = actionProgressController.ActionDescription;
                // action level
                int actionLevel = actionProgressController.ActionLevel;
                // is this first step of action
                bool firstStepOfAction = actionStep == 0;
                // if action level has progress bar
                if (actionLevel < _progressBars.Length)
                {
                    // show progress value
                    ProgressBar progressBar = _progressBars[actionLevel];

                    double progress = 0;
                    if (actionProgressController.StepCount > 0)
                    {
                        progress = actionStep / actionProgressController.StepCount;

                        SetProgressBarValueSafe(progressBar, progress);
                    }

                    string percentageDescription;
                    if (actionProgressController.StepCount > 0)
                        percentageDescription = string.Format(CultureInfo.InvariantCulture, "{0:f1}%", progress * 100);
                    else
                        percentageDescription = "0%";

                    string labelDescription;
                    if (actionDescription != null)
                        labelDescription = string.Format("{0}... ({1})", actionDescription, percentageDescription);
                    else
                        labelDescription = percentageDescription;

                    SetLabelTextSafe(_labels[actionLevel], labelDescription);

                    if (firstStepOfAction)
                    {
                        for (int i = actionLevel + 1; i < _progressBars.Length; i++)
                        {
                            SetProgressBarValueSafe(_progressBars[i], 0.0);
                            SetLabelTextSafe(_labels[i], string.Empty);
                        }
                    }
                }

                // add action description to Log TextBox
                string logMessage = actionDescription;
                if (logMessage != null)
                {
                    if (firstStepOfAction)
                    {
                        logMessage = string.Format("{0}...", logMessage);
                    }
                    else if (actionProgressController.IsFinished)
                    {
                        logMessage = string.Format("  Finished ({0}).", logMessage);
                    }
                    else
                    {
                        logMessage = "";
                    }
                    if (logMessage != "")
                    {
                        string prevMessage = null;
                        if (!_levelToMessage.TryGetValue(actionLevel, out prevMessage))
                            prevMessage = null;
                        if (prevMessage != logMessage)
                        {
                            _levelToMessage[actionLevel] = logMessage;
                            logMessage = logMessage.PadLeft(logMessage.Length + actionLevel * 4, ' ');
                            if (_logTextBox.Dispatcher.Thread != Thread.CurrentThread)
                                _logTextBox.Dispatcher.Invoke(new AddLogMessageDelegate(AddLogMessage), logMessage);
                            else
                                AddLogMessage(logMessage);
                        }
                    }
                }

                return true;
            }


            private void SetLabelTextSafe(Label label, string text)
            {
                if (label.Dispatcher.Thread != Thread.CurrentThread)
                    label.Dispatcher.Invoke(
                        new SetLabelTextDelegate(SetLabelText),
                        label,
                        text);
                else
                    SetLabelText(label, text);
            }

            private void SetProgressBarValueSafe(ProgressBar progressBar, double progress)
            {
                if (progressBar.Dispatcher.Thread != Thread.CurrentThread)
                    progressBar.Dispatcher.Invoke(
                        new SetProgressBarValueDelegate(SetProgressBarValue),
                        progressBar,
                        progress);
                else
                    SetProgressBarValue(progressBar, progress);
            }

            internal void CancelAction()
            {
                _cancelRequested = true;
            }

            private void SetProgressBarValue(ProgressBar progressBar, double progress)
            {
                double range = progressBar.Maximum - progressBar.Minimum;
                progressBar.Value = progressBar.Minimum + progress * range;
            }

            private void SetLabelText(Label label, string text)
            {
                label.Content = text;
            }

            private void AddLogMessage(string message)
            {
                _logTextBox.AppendText(string.Format("{0}{1}", message, Environment.NewLine));
                _logTextBox.ScrollToEnd();
            }

            #endregion



            #region Delegates

            delegate void SetProgressBarValueDelegate(ProgressBar progressBar, double progress);

            delegate void SetLabelTextDelegate(Label label, string text);

            delegate void AddLogMessageDelegate(string message);

            #endregion

        }

        #endregion



        #region Fields

        ProgressBar[] _progressBars;

        Label[] _labels;

        TextBox _logText;

        Button _cancelButton;

        BackgroundWorker _backgroundWorker;

        DoBackgroundWorkDelegate _callback;

        ActionProgressHandler _progressHandler;

        ActionProgressController _progressController;

        bool _errorOccured;

        #endregion



        #region Constructors

        public ActionProgressWindow(DoBackgroundWorkDelegate callback, int levelCount, string caption)
        {
            if (levelCount <= 0)
                throw new Exception();

            _callback = callback;

            InitializeComponent();

            layoutGrid.ColumnDefinitions.Clear();
            layoutGrid.RowDefinitions.Clear();

            ColumnDefinition column = new ColumnDefinition();
            column.Width = new GridLength(1, GridUnitType.Auto);
            layoutGrid.ColumnDefinitions.Add(column);

            int rowCount = 2 * levelCount + 2;
            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Auto);
                layoutGrid.RowDefinitions.Add(row);
            }

            ProgressBar[] progressBars = new ProgressBar[levelCount];
            Label[] labels = new Label[levelCount];
            for (int i = 0; i < levelCount; i++)
            {
                Label label = new Label();
                labels[i] = label;
                label.Background = Brushes.White;
                label.Width = 500;
                label.Height = 36;
                label.Margin = new Thickness(5, 0, 5, 0);
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.VerticalContentAlignment = VerticalAlignment.Bottom;

                Grid.SetColumn(label, 0);
                Grid.SetRow(label, 2 * i);
                layoutGrid.Children.Add(label);

                ProgressBar progressBar = new ProgressBar();

                progressBars[i] = progressBar;
                progressBar.Width = 500;
                progressBar.Height = 30;
                progressBar.Margin = new Thickness(5, 0, 5, 0);

                Grid.SetColumn(progressBar, 0);
                Grid.SetRow(progressBar, 2 * i + 1);
                layoutGrid.Children.Add(progressBar);
            }

            TextBox logText = new TextBox();
            _logText = logText;
            logText.TextWrapping = TextWrapping.Wrap;
            logText.Width = 500;
            logText.Height = 150;
            logText.Margin = new Thickness(0, 5, 0, 5);
            logText.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            logText.IsReadOnly = true;

            Grid.SetColumn(logText, 0);
            Grid.SetRow(logText, 2 * levelCount);
            layoutGrid.Children.Add(logText);

            Button cancelButton = new Button();
            _cancelButton = cancelButton;
            cancelButton.Width = 100;
            cancelButton.Height = 30;
            cancelButton.Content = "Cancel";
            cancelButton.Margin = new Thickness(200, 10, 200, 10);
            cancelButton.Click += new RoutedEventHandler(cancelButton_Click);

            Grid.SetColumn(cancelButton, 0);
            Grid.SetRow(cancelButton, 2 * levelCount + 1);
            layoutGrid.Children.Add(cancelButton);

            this.SizeToContent = SizeToContent.WidthAndHeight;

            this.Title = caption;

            _progressHandler = new ActionProgressHandler(progressBars, labels, logText);
            _progressController = new ActionProgressController(_progressHandler);

            _progressBars = progressBars;
            _labels = labels;
        }

        #endregion



        #region Properties

        bool _closeAfterComplete = false;
        /// <summary>
        /// Gets or sets a value indicating whether the form can be closed.
        /// </summary>
        /// <value>
        /// <b>true</b> - form must be closed when action is completed; otherwise, <b>false</b>.
        /// </value>
        [DefaultValue(false)]
        public bool CloseAfterComplete
        {
            get
            {
                return _closeAfterComplete;
            }
            set
            {
                _closeAfterComplete = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        public bool? RunAndShowDialog(Window owner)
        {
            _errorOccured = false;
            this.Owner = owner;
            return ShowDialog();
        }

        #endregion


        #region PROTECTED

        protected override void OnClosing(CancelEventArgs e)
        {
            bool? result = ClickToCancelButton();
            if (result == null)
                e.Cancel = true;
            else
                DialogResult = result;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Handles the Loaded event of Window object.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            _backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the DoWork event of BackgroundWorker object.
        /// </summary>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _callback(_progressController);
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of BackgroundWorker object.
        /// </summary>
        private void backgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            string text;
            if (e.Error != null)
            {
                _errorOccured = true;
                DemosTools.ShowErrorMessage(e.Error);
                text = "Error.";
            }
            else
            {
                if (CloseAfterComplete)
                    this.Close();

                if (_progressController.IsCanceled)
                    text = "Canceled.";
                else
                    text = "Finished.";
            }
            _cancelButton.Content = "Close";
            for (int i = 0; i < _labels.Length; i++)
                _labels[i].Content = text;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = ClickToCancelButton();
            if (result != null)
                DialogResult = result;
        }

        private bool? ClickToCancelButton()
        {
            if (_progressController.IsCanceled || _errorOccured)
                return false;

            if (_progressController.IsFinished)
                return true;

            _cancelButton.IsEnabled = false;
            _progressHandler.CancelAction();
            while (_backgroundWorker.IsBusy)
            {
                Thread.Sleep(1);
                DemosTools.DoEvents();
            }
            _cancelButton.IsEnabled = true;
            for (int i = 0; i < _progressBars.Length; i++)
                _progressBars[i].Value = 0;
            _logText.AppendText("Canceled.");
            return null;
        }

        #endregion

        #endregion



        #region Delegates

        public delegate void DoBackgroundWorkDelegate(IActionProgressController progressController);

        #endregion

    }
}
