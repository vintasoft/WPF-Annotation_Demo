#if REMOVE_OFFICE_PLUGIN
#error Remove OfficeDocumentFontPropertiesVisualEditorToolStrip from the project.
#endif

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Vintasoft.Imaging.Office.OpenXml.Editor;
using Vintasoft.Imaging.Office.OpenXml.Editor.Docx;
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Interaction logic for OpenXmlDocumentChartDataWindow.xaml
    /// </summary>
    public partial class OpenXmlDocumentChartDataWindow : Window
    {

        #region Fields

        /// <summary>
        /// A value indicating whether UI is updating.
        /// </summary>
        bool _isUiUpdating = false;

        /// <summary>
        /// A value indicating whether series is updating.
        /// </summary>
        bool _seriesUiUpdating = false;

        /// <summary>
        /// The DOCX document editor.
        /// </summary>
        DocxDocumentEditor _documentEditor;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenXmlDocumentChartDataWindow"/> class.
        /// </summary>
        public OpenXmlDocumentChartDataWindow()
        {
            InitializeComponent();

            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Circle);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Cross);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Dash);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Diamond);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Dot);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Picture);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Plus);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Square);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Star);
            seriesMarkerStyleComboBox.Items.Add(OpenXmlDocumentChartMarkerStyle.Triangle);

        }

        #endregion



        #region Properties

        WpfOfficeDocumentVisualEditor _visualEditor;
        /// <summary>
        /// Gets or sets the visual editor for Office document.
        /// </summary>
        public WpfOfficeDocumentVisualEditor VisualEditor
        {
            get
            {
                return _visualEditor;
            }
            set
            {
                if (_visualEditor != null)
                {
                    _documentEditor.Dispose();
                    chartComboBox.Items.Clear();
                }

                _visualEditor = value;

                if (_visualEditor != null)
                {
                    _documentEditor = _visualEditor.CreateDocumentEditor();
                    if (_documentEditor != null)
                    {
                        int number = 1;
                        foreach (OpenXmlDocumentChart chart in _documentEditor.Charts)
                        {
                            string name = null;
                            if (chart.Properties != null)
                                name = chart.Properties.Name;
                            else
                                name = string.Format("Chart {0}", number);
                            chartComboBox.Items.Add(name);
                            number++;
                        }
                    }
                    chartComboBox.SelectedIndex = 0;

                    UpdateSeriesUI();
                }
            }
        }

        /// <summary>
        /// Gets the index of the focused chart.
        /// </summary>
        public int FocusedChartIndex
        {
            get
            {
                return chartComboBox.SelectedIndex;
            }
        }

        /// <summary>
        /// Gets the focused chart.
        /// </summary>
        public OpenXmlDocumentChart FocusedChart
        {
            get
            {
                return _documentEditor.Charts[FocusedChartIndex];
            }
        }

        /// <summary>
        /// Gets the focused series.
        /// </summary>
        private OpenXmlDocumentChartDataSeries FocusedSeries
        {
            get
            {
                int seriesIndex = FocusedColumnIndex - 1;
                if (seriesIndex >= 0)
                    return FocusedChart.ChartData.GetSeries(seriesIndex);
                return null;
            }
        }

        /// <summary>
        /// Gets the focused data point.
        /// </summary>
        private OpenXmlDocumentChartDataPoint FocusedDataPoint
        {
            get
            {
                OpenXmlDocumentChartDataSeries focusedSeries = FocusedSeries;
                if (focusedSeries != null)
                {
                    int dataPointIndex = FocusedRowIndex - 1;
                    if (dataPointIndex >= 0)
                        return focusedSeries.GetDataPoint(dataPointIndex);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the index of the focused row.
        /// </summary>
        private int FocusedRowIndex
        {
            get
            {
                if (SelectedCell.IsValid)
                    return chartDataGrid.Items.IndexOf(SelectedCell.Item);
                DataTable dataTable = GetFocusedDataTable();
                return dataTable.Rows.Count - 1;
            }
        }

        /// <summary>
        /// Gets the index of the focused column.
        /// </summary>
        private int FocusedColumnIndex
        {
            get
            {

                if (SelectedCell.IsValid)
                    return SelectedCell.Column.DisplayIndex;
                DataTable dataTable = GetFocusedDataTable();
                return dataTable.Columns.Count - 1;
            }
        }

        /// <summary>
        /// Gets the selected cell.
        /// </summary>
        private DataGridCellInfo SelectedCell
        {
            get
            {
                if (chartDataGrid.SelectedCells.Count == 0)
                    return new DataGridCellInfo();
                return chartDataGrid.SelectedCells[0];
            }
        }
        #endregion



        #region Methods

        /// <summary>
        /// Handles the SelectionChanged event of ChartComboBox object.
        /// </summary>
        private void chartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isUiUpdating = true;

            if (FocusedChartIndex >= 0)
            {
                UpdateFocusedChartDataTable();
            }

            _isUiUpdating = false;
        }

        /// <summary>
        /// Handles the ColumnChanged event of DataTable object.
        /// </summary>
        private void DataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            OnChartDataChanged();
        }


        /// <summary>
        /// Handles the TextChanged event of TitleTextBox object.
        /// </summary>
        private void titleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUiUpdating)
            {
                FocusedChart.Title = titleTextBox.Text;
                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of AddRowButton object.
        /// </summary>
        private void addRowButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            dataTable.Rows.Add(dataTable.NewRow());

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the Click event of InsertRowButton object.
        /// </summary>
        private void insertRowButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            dataTable.Rows.InsertAt(dataTable.NewRow(), Math.Max(1, FocusedRowIndex));

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the Click event of ClearRowButton object.
        /// </summary>
        private void clearRowButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            int focusedRowIndex = FocusedRowIndex;
            for (int i = 0; i < dataTable.Columns.Count; i++)
                dataTable.Rows[focusedRowIndex][i] = null;

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the Click event of RemoveRowButton object.
        /// </summary>
        private void removeRowButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            if (dataTable.Rows.Count > 2)
                dataTable.Rows.RemoveAt(FocusedRowIndex);

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the Click event of AddColumnButton object.
        /// </summary>
        private void addColumnButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            dataTable.Columns.Add();

            OnChartDataChanged();

            UpdateFocusedChartDataTable();
        }

        /// <summary>
        /// Handles the Click event of InsertColumnButton object.
        /// </summary>
        private void insertColumnButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            int focusedColumnIndex = Math.Max(1, FocusedColumnIndex);

            dataTable.Columns.Add();

            if (focusedColumnIndex < dataTable.Columns.Count - 1)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = dataTable.Columns.Count - 1; j > focusedColumnIndex; j--)
                    {
                        dataTable.Rows[i][j] = dataTable.Rows[i][j - 1];
                    }
                }
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    dataTable.Rows[i][focusedColumnIndex] = null;
                }
            }

            OnChartDataChanged();

            UpdateFocusedChartDataTable();
        }

        /// <summary>
        /// Handles the Click event of ClearColumnButton object.
        /// </summary>
        private void clearColumnButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            int focusedColumnIndex = FocusedColumnIndex;
            for (int i = 0; i < dataTable.Rows.Count; i++)
                dataTable.Rows[i][focusedColumnIndex] = null;

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the Click event of RemoveColumnButton object.
        /// </summary>
        private void removeColumnButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = GetFocusedDataTable();

            if (dataTable.Columns.Count > 2)
            {
                int focusedColumnIndex = FocusedColumnIndex;

                dataTable.Columns.RemoveAt(focusedColumnIndex);
                chartDataGrid.Columns.RemoveAt(focusedColumnIndex);
            }

            OnChartDataChanged();
        }

        /// <summary>
        /// Handles the SelectedCellsChanged event of ChartDataGrid object.
        /// </summary>
        private void chartDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            UpdateSeriesUI();
        }

        /// <summary>
        /// Handles the ColorChanged event of SeriesColorPanelControl object.
        /// </summary>
        private void seriesColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            if (!_seriesUiUpdating)
            {
                if (FocusedSeries.ShapeProperties.FillColor.HasValue)
                    FocusedSeries.ShapeProperties.FillColor = WpfObjectConverter.CreateDrawingColor(seriesColorPanelControl.Color);
                if (FocusedSeries.ShapeProperties.OutlineColor.HasValue)
                    FocusedSeries.ShapeProperties.OutlineColor = WpfObjectConverter.CreateDrawingColor(seriesColorPanelControl.Color);

                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Handles the ColorChanged event of MarkerColorPanelControl object.
        /// </summary>
        private void markerColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            if (!_seriesUiUpdating)
            {
                UpdateFocusedSeriesMarkerColor();

                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Handles the ColorChanged event of DataPointColorPanelControl object.
        /// </summary>
        private void dataPointColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            if (!_seriesUiUpdating)
            {
                if (FocusedDataPoint.ShapeProperties.FillColor.HasValue)
                    FocusedDataPoint.ShapeProperties.FillColor = WpfObjectConverter.CreateDrawingColor(dataPointColorPanelControl.Color);
                if (FocusedDataPoint.ShapeProperties.OutlineColor.HasValue)
                    FocusedDataPoint.ShapeProperties.OutlineColor = WpfObjectConverter.CreateDrawingColor(dataPointColorPanelControl.Color);

                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of SeriesMarkerStyleComboBox object.
        /// </summary>
        private void seriesMarkerStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_seriesUiUpdating)
            {
                FocusedSeries.Marker.Style = (OpenXmlDocumentChartMarkerStyle)seriesMarkerStyleComboBox.SelectedItem;

                UpdateFocusedSeriesMarkerColor();

                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of SeriesMarkerSizeNumericUpDown object.
        /// </summary>
        private void seriesMarkerSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!_seriesUiUpdating)
            {
                FocusedSeries.Marker.Size = (byte)seriesMarkerSizeNumericUpDown.Value;

                VisualEditor.OnDocumentChanged();
            }
        }

        /// <summary>
        /// Updates the series UI.
        /// </summary>
        private void UpdateSeriesUI()
        {
            _seriesUiUpdating = true;

            OpenXmlDocumentChartDataSeries focusedSeries = FocusedSeries;
            // if series are selected
            if (focusedSeries != null)
            {
                // enable series color controls
                seriesColorLabel.IsEnabled = true;
                seriesColorPanelControl.IsEnabled = true;

                // set color in seriesColorPanelControl
                if (focusedSeries.ShapeProperties.FillColor.HasValue)
                    seriesColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(focusedSeries.ShapeProperties.FillColor.Value);
                else if (focusedSeries.ShapeProperties.OutlineColor.HasValue)
                    seriesColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(focusedSeries.ShapeProperties.OutlineColor.Value);
                else
                    seriesColorPanelControl.Color = Colors.Transparent;
            }
            else
            {
                // disable series color controls
                seriesColorLabel.IsEnabled = false;
                seriesColorPanelControl.IsEnabled = false;
                seriesColorPanelControl.Color = Colors.Transparent;
            }

            // if series are selected and focused series have markers
            if (focusedSeries != null && focusedSeries.HasMarkers)
            {
                // enable marker color controls
                markerColorLabel.IsEnabled = true;
                markerColorPanelControl.IsEnabled = true;

                // set color in markerColorPanelControl
                if (FocusedSeries.Marker.ShapeProperties.FillColor.HasValue)
                    markerColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(FocusedSeries.Marker.ShapeProperties.FillColor.Value);
                else if (FocusedSeries.Marker.ShapeProperties.OutlineColor.HasValue)
                    markerColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(FocusedSeries.Marker.ShapeProperties.OutlineColor.Value);
                else
                    markerColorPanelControl.Color = Colors.Transparent;
            }
            else
            {
                // disable marker color controls
                markerColorLabel.IsEnabled = false;
                markerColorPanelControl.IsEnabled = false;
                markerColorPanelControl.Color = Colors.Transparent;
            }

            OpenXmlDocumentChartDataPoint focusedDataPoint = FocusedDataPoint;
            // if data point is selected
            if (focusedDataPoint != null)
            {
                // enable series color controls
                dataPointColorLabel.IsEnabled = true;
                dataPointColorPanelControl.IsEnabled = true;

                // set color in dataPointColorPanelControl
                if (focusedDataPoint.ShapeProperties.FillColor.HasValue)
                    dataPointColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(focusedDataPoint.ShapeProperties.FillColor.Value);
                else if (focusedDataPoint.ShapeProperties.OutlineColor.HasValue)
                    dataPointColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(focusedDataPoint.ShapeProperties.OutlineColor.Value);
                else
                    dataPointColorPanelControl.Color = Colors.Transparent;
            }
            else
            {
                // disable data point color controls
                dataPointColorLabel.IsEnabled = false;
                dataPointColorPanelControl.IsEnabled = false;
                dataPointColorPanelControl.Color = Colors.Transparent;
            }

            // if focused series have marker
            if (focusedSeries != null && focusedSeries.Marker != null)
            {
                seriesMarkerStyleComboBox.IsEnabled = true;
                seriesMarkerStyleLabel.IsEnabled = true;
                seriesMarkerSizeNumericUpDown.IsEnabled = true;
                seriesMarkerSizeLabel.IsEnabled = true;
                if (focusedSeries.Marker.Style == null)
                    seriesMarkerStyleComboBox.SelectedItem = OpenXmlDocumentChartMarkerStyle.Auto;
                else
                    seriesMarkerStyleComboBox.SelectedItem = focusedSeries.Marker.Style;
                if (focusedSeries.Marker.Size.HasValue)
                    seriesMarkerSizeNumericUpDown.Value = focusedSeries.Marker.Size.Value;
                else
                    seriesMarkerSizeNumericUpDown.Value = 5;
            }
            else
            {
                seriesMarkerSizeLabel.IsEnabled = false;
                seriesMarkerSizeNumericUpDown.IsEnabled = false;
                seriesMarkerStyleComboBox.IsEnabled = false;
                seriesMarkerStyleLabel.IsEnabled = false;
            }

            _seriesUiUpdating = false;
        }


        /// <summary>
        /// Updates the color of the focused series marker.
        /// </summary>
        private void UpdateFocusedSeriesMarkerColor()
        {
            OpenXmlDocumentChartMarker marker = FocusedSeries.Marker;
            if (marker.Style == OpenXmlDocumentChartMarkerStyle.Star ||
                marker.Style == OpenXmlDocumentChartMarkerStyle.Cross ||
                marker.Style == OpenXmlDocumentChartMarkerStyle.Plus)
            {
                marker.ShapeProperties.FillColor = null;
                marker.ShapeProperties.OutlineColor = WpfObjectConverter.CreateDrawingColor(markerColorPanelControl.Color);
            }
            else
            {
                marker.ShapeProperties.FillColor = WpfObjectConverter.CreateDrawingColor(markerColorPanelControl.Color);
                marker.ShapeProperties.OutlineColor = WpfObjectConverter.CreateDrawingColor(markerColorPanelControl.Color);
            }
        }


        /// <summary>
        /// Returns the focused data table.
        /// </summary>
        /// <returns></returns>
        private DataTable GetFocusedDataTable()
        {
            return ((DataView)chartDataGrid.ItemsSource).Table;
        }

        /// <summary>
        /// Called when chart data is changed.
        /// </summary>
        private void OnChartDataChanged()
        {
            if (_isUiUpdating)
                return;

            FocusedChart.ChartData.SetFromDataTable(GetFocusedDataTable());

            VisualEditor.OnDocumentChanged();
        }

        /// <summary>
        /// Updates the focused chart data table.
        /// </summary>
        private void UpdateFocusedChartDataTable()
        {
            OpenXmlDocumentChart chart = FocusedChart;
            titleTextBox.Text = chart.Title;

            SetDataGridSource(chart.ChartData.GetAsDataTable());
        }

        /// <summary>
        /// Sets the source of DataGrid.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        private void SetDataGridSource(DataTable dataTable)
        {
            if (chartDataGrid.ItemsSource != null)
            {
                DataTable oldDataTable = (chartDataGrid.ItemsSource as DataView).Table;
                oldDataTable.ColumnChanged -= DataTable_ColumnChanged;
            }
            chartDataGrid.ItemsSource = dataTable.DefaultView;

            if (dataTable != null)
                dataTable.ColumnChanged += DataTable_ColumnChanged;
        }

        #endregion

    }
}
