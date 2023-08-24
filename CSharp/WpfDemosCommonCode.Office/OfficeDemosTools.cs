using System;
using System.IO;
using System.Windows;

using Microsoft.Win32;

using WpfDemosCommonCode.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.Office.OpenXml.Editor.Docx;
#if !REMOVE_OFFICE_PLUGIN
using Vintasoft.Imaging.Office.OpenXml.Editor;
#endif
using Vintasoft.Imaging.UI;

namespace WpfDemosCommonCode.Office
{
    /// <summary>
    /// Contains collection of helper-algorithms for demo applications, which use VintaSoft Office .NET Plugin.
    /// </summary>
    public static class OfficeDemosTools
    {

        #region Fields

        /// <summary>
        /// The chart images.
        /// </summary>
        static ImageCollection _chartImages;

        /// <summary>
        /// The open file dialog that allows to open Office document.
        /// </summary>
        static OpenFileDialog _openDocumentDialog = new OpenFileDialog();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="OfficeDemosTools"/> class.
        /// </summary>
        static OfficeDemosTools()
        {
            _openDocumentDialog.Filter = "All Supported Documents (*.docx;*.doc;*.txt)|*.docx;*.doc;*.txt";
        }

        #endregion



        #region Methods

        /// <summary>
        /// Selects the Chart resource.
        /// </summary>
        public static Stream SelectChartResource()
        {
            string[] chartResourceNames = DemosResourcesManager.FindResourceNames("Chart_");

            // if chart images are not created
            if (_chartImages == null)
            {
                // create collection of chart images
                _chartImages = new ImageCollection();
                _chartImages.LayoutSettingsRequest += ChartImages_LayoutSettingsRequest;
                foreach (string chartResourceName in chartResourceNames)
                    _chartImages.Add(DemosResourcesManager.GetResourceAsStream(chartResourceName), true);
            }
            if (_chartImages.Count == 0)
                throw new Exception("Chart resources was not found.");

            // create dialog that allows to select image
            SelectImageWindow selectImageWindow = new SelectImageWindow(_chartImages);
            selectImageWindow.Width = 900;
            selectImageWindow.Height = 550;
            selectImageWindow.ImagePreviewViewer.ThumbnailSize = new Size(200, 200);
            selectImageWindow.ImagePreviewViewer.ThumbnailFlowStyle = ThumbnailFlowStyle.WrappedRows;
            selectImageWindow.Title = "Select a chart";
            selectImageWindow.Topmost = true;

            if (selectImageWindow.ShowDialog() != true)
                return null;

            string selectedChartResourceName = chartResourceNames[selectImageWindow.SelectedImageIndex];

            return DemosResourcesManager.GetResourceAsStream(selectedChartResourceName);
        }

        /// <summary> 
        /// Handles the LayoutSettingsRequest event of the ChartImages.
        /// </summary>
        private static void ChartImages_LayoutSettingsRequest(object sender, DocumentLayoutSettingsRequestEventArgs e)
        {
            // layout only first page
            e.LayoutSettings.PageCount = 1;

            // use releative size instread specified size of graphics object
            e.LayoutSettings.UseGraphicObjectReleativeSize = true;

            // set page size to 70x70mm
            e.LayoutSettings.PageLayoutSettings = new PageLayoutSettings(ImageSize.FromMillimeters(70, 70, new Resolution(96)));
        }

        /// <summary>
        /// Selects the Office document.
        /// </summary>
        public static Stream SelectOfficeDocument()
        {
#if !REMOVE_OFFICE_PLUGIN
            // if image is selected
            if (_openDocumentDialog.ShowDialog() == true)
            {
                string documentFilename = _openDocumentDialog.FileName;
                if (File.Exists(documentFilename))
                {
                    // if selected file is TXT file
                    if (Path.GetExtension(documentFilename).ToLowerInvariant() == ".txt")
                    {
                        // get "EmptyDocument.docx" resource
                        Stream documentStream = DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx");
                        MemoryStream tempStream = new MemoryStream();
                        // create DOCX editor for "EmptyDocument.docx"
                        using (DocxDocumentEditor editor = new DocxDocumentEditor(documentStream))
                        {
                            // set document text to a TXT file
                            editor.Body.Text = File.ReadAllText(documentFilename);

                            // save document
                            editor.Save(tempStream);
                        }

                        documentStream.Dispose();
                        return tempStream;
                    }

                    // open document
                    return File.OpenRead(documentFilename);
                }

                // use empty document
                return DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx");
            }
#endif
            return null;
        }

        /// <summary>
        /// Converts the text file to a DOCX document.
        /// </summary>
        /// <param name="txtFilename">The text filename.</param>
        /// <returns>Stream that contains converted DOCX document.</returns>
        public static Stream ConvertTxtFileToDocxDocument(string txtFilename)
        {
#if !REMOVE_OFFICE_PLUGIN
            // get "EmptyDocument.docx" resource
            using (Stream documentStream = DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx"))
            {
                if (documentStream == null)
                    throw new Exception("TXT to DOCX conversion error: Resource 'EmptyDocument.docx' is not found in demo application.");

                MemoryStream tempStream = new MemoryStream();
                // create DOCX editor for "EmptyDocument.docx"
                using (DocxDocumentEditor editor = new DocxDocumentEditor(documentStream))
                {
                    // set document text to a TXT file
                    editor.Body.Text = File.ReadAllText(txtFilename);

                    // save document
                    editor.Save(tempStream);
                }

                return tempStream;
            }
#else
            return null;
#endif
        }

        #endregion

    }
}
