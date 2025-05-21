#if REMOVE_PDF_PLUGIN
#error Remove DocumentSignaturesWindow from project.
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Microsoft.Win32;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.DigitalSignatures;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that shows digital signatures of PDF document.
    /// </summary>
    public partial class DocumentSignaturesWindow : Window
    {

        #region Fields

        /// <summary>
        /// "Fail" default color.
        /// </summary>
        static readonly Brush _failColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 120, 120));

        /// <summary>
        /// "Warning" default color.
        /// </summary>
        static readonly Brush _warningColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 50));

        /// <summary>
        /// "Success" default color.
        /// </summary>
        static readonly Brush _successColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(165, 255, 90));

        /// <summary>
        /// "Signing is not implemented" default color.
        /// </summary>
        static readonly Brush _signingNotImplementedColor = new SolidColorBrush(System.Windows.Media.Colors.LightGray);


        /// <summary>
        /// The PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// List of PdfSignatureInformation objects.
        /// </summary>
        List<PdfSignatureInformation> _signatureInfos = new List<PdfSignatureInformation>();

        /// <summary>
        /// List of PdfPkcsSignature objects.
        /// </summary>
        List<PdfPkcsSignature> _signatures = new List<PdfPkcsSignature>();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSignaturesWindow"/> class.
        /// </summary>
        public DocumentSignaturesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSignaturesWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <exception cref="Exception">Thrown if
        /// the document does not contain digital signatures.</exception>
        public DocumentSignaturesWindow(PdfDocument document)
            : this()
        {
            _document = document;

            if (_document.InteractiveForm == null)
                throw new Exception("Document does not contain digital signatures.");

            BuildSignaturesTreeView();
            UpdateUI();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the selected signature field.
        /// </summary>
        public PdfInteractiveFormSignatureField SelectedSignatureField
        {
            get
            {
                TreeViewItem node = (TreeViewItem)signaturesTreeView.SelectedItem;
                if (node == null)
                    return null;
                while (node.Parent != null)
                    node = (TreeViewItem)node.Parent;
                return (PdfInteractiveFormSignatureField)node.Tag;
            }
            set
            {
                foreach (TreeViewItem node in signaturesTreeView.Items)
                    if (node.Tag == value)
                    {
                        node.IsSelected = true;
                        break;
                    }

            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of the "OK" button control.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the "Verify All Signatures" button control.
        /// </summary>
        private void verifyAllButton_Click(object sender, RoutedEventArgs e)
        {
            // for each signature field
            for (int i = 0; i < _signatures.Count; i++)
            {
                // if signature field does not have signature information
                if (_signatureInfos[i] == null)
                    continue;

                PdfSignatureInformation signatureInfo = _signatureInfos[i];
                PdfPkcsSignature signature = _signatures[i];
                TreeViewItem item = (TreeViewItem)signaturesTreeView.Items[i];
                TreeViewItem subItem;

                // if signature information does not have PKCS signature
                // (signature is not supported OR damaged)
                if (signature == null)
                {
                    item.Background = _failColor;
                }
                // if signature cannot be used for signing
                else if (!signature.IsSigningImplemented)
                {
                    item.Background = _signingNotImplementedColor;
                    ((TreeViewItem)item.Items[0]).Background = item.Background;
                }
                else
                {
                    // verify signature

                    bool signatureVerifyResult = false;
                    bool certificateVerifyResult = false;
                    bool embeddedTimestampVerifyResult = false;
                    bool signatureCoversWholeDocument = false;
                    bool hasTimestampCertificate = false;
                    bool timestampCertificateVerifyResult = false;
                    TreeViewItem items = (TreeViewItem)signaturesTreeView.Items[i];
                    TreeViewItem subItems = (TreeViewItem)items.Items[0];
                    subItems.Items.Clear();
                    X509Chain certChain = null;
                    X509Chain timestampCertChain = null;

                    TreeViewItem firstSubItem = ((TreeViewItem)item.Items[0]);
                    item.Background = Brushes.White;
                    firstSubItem.Header = "Verifying signature...";
                    item.IsExpanded = true;

                    bool failed = false;
                    bool pagesModified = false;
                    string subsequentChangesMessage = "";
                    try
                    {
                        // check that signature covers the whole document
                        signatureCoversWholeDocument = _signatureInfos[i].SignatureCoversWholeDocument();

                        // verify PKCS signature
                        signatureVerifyResult = signature.VerifySignature();

                        // if signature has embedded timestamp
                        if (signature.HasEmbeddedTimeStamp)
                        {
                            // verify embedded timestamp
                            embeddedTimestampVerifyResult = signature.VerifyTimestamp();
                        }


                        // signer revision
                        PdfDocumentRevision signerRevision = signatureInfo.SignedRevision;
                        item.Tag = signerRevision;

                        // if signature verification is success and signature not covers whole document
                        if (signerRevision != null && signatureVerifyResult && !signatureCoversWholeDocument)
                        {
                            // check subsequent changes
                            using (PdfDocumentRevisionComparer revisionComparer = signatureInfo.GetDocumentChanges())
                            {
                                // detect was page(s) modified
                                pagesModified = revisionComparer.HasModifiedPages;

                                // build subsequent changes message
                                if (revisionComparer.ChangedPageContents.Count > 0)
                                    subsequentChangesMessage += string.Format("{0} page(s) modified; ", revisionComparer.ChangedPageContents.Count);
                                if (revisionComparer.AddedPages.Count > 0)
                                    subsequentChangesMessage += string.Format("{0} page(s) added; ", revisionComparer.AddedPages.Count);
                                if (revisionComparer.RemovedPages.Count > 0)
                                    subsequentChangesMessage += string.Format("{0} page(s) removed; ", revisionComparer.RemovedPages.Count);
                                if (revisionComparer.RemovedAnnotations.Count > 0)
                                    subsequentChangesMessage += string.Format("annotations(s) on {0} page(s) removed; ", revisionComparer.RemovedAnnotations.Count);
                                if (revisionComparer.RemovedAnnotations.Count > 0)
                                    subsequentChangesMessage += string.Format("removed annotation(s) on {0} page(s); ", revisionComparer.RemovedAnnotations.Count);
                                if (revisionComparer.AddedAnnotations.Count > 0)
                                    subsequentChangesMessage += string.Format("added annotation(s) on {0} page(s); ", revisionComparer.AddedAnnotations.Count);
                                if (revisionComparer.ChangedAnnotations.Count > 0)
                                    subsequentChangesMessage += string.Format("changed annotation(s) on {0} page(s); ", revisionComparer.ChangedAnnotations.Count);
                                if (revisionComparer.MiscellaneousChanges.Count > 0)
                                    subsequentChangesMessage += string.Format("miscellaneous changes: {0}; ", revisionComparer.MiscellaneousChanges.Count);
                            }
                        }

                        // use certificate chain from signature to build and verify certificate chain (no revocation check)
                        bool useSigningCertificateChainToBuildChain = useSigningCertificateChainToBuildChainCheckBox.IsChecked.Value == true;

                        // build and verify signing certificate chain
                        certificateVerifyResult = BuildX509Chain(signature.SigningCertificateChain, useSigningCertificateChainToBuildChain, out certChain);

                        // if signature has Timestamp
                        X509Certificate2 timestampCertificate = signature.TimestampCertificate;
                        if (timestampCertificate != null)
                        {
                            hasTimestampCertificate = true;
                            // build and verify timestamp certificate chain
                            timestampCertificateVerifyResult = BuildX509Chain(signature.TimestampCertificateChain, useSigningCertificateChainToBuildChain, out timestampCertChain);
                        }
                    }
                    catch (Exception validateException)
                    {
                        failed = true;
                        item.Background = _failColor;
                        firstSubItem.Header = string.Format("Error: {0}", validateException.Message);
                        firstSubItem.Background = _failColor;
                    }
                    if (failed)
                        continue;

                    // show signature verification results

                    Brush validateColor = Brushes.White;
                    string validateResult = "";

                    // if signature verification is failed OR signature does not cover the whole document AND page(s) modified
                    if (!signatureVerifyResult || (!signatureCoversWholeDocument && pagesModified))
                    {
                        validateResult = "Signature is invalid";
                        validateColor = _failColor;
                    }
                    // if certificate validation is failed
                    else if (!certificateVerifyResult || (hasTimestampCertificate && !timestampCertificateVerifyResult) || (signature.HasEmbeddedTimeStamp && !embeddedTimestampVerifyResult))
                    {
                        validateResult = "Signature validity is unknown";
                        validateColor = _warningColor;
                    }
                    // certificate is valid
                    else
                    {
                        validateResult = "Signature is valid";
                        validateColor = _successColor;
                    }
                    item.Background = validateColor;
                    firstSubItem.Header = validateResult;
                    firstSubItem.Background = validateColor;


                    // show signature verification details

                    // if signature verification is successful
                    if (signatureVerifyResult)
                    {
                        // if signature covers the whole document
                        if (signatureCoversWholeDocument)
                        {
                            AddTreeViewItem(firstSubItem, "Signature verification: Document has not been modified since this signature was applied");
                            ((TreeViewItem)firstSubItem.Items[0]).Background = _successColor;
                        }
                        // if signature does NOT cover the whole document
                        else
                        {
                            if (pagesModified)
                            {
                                // veroification falied
                                subItem = AddTreeViewItem(firstSubItem, "Signature verification: Document has been modified or corrupted since it was signed");
                                subItem.Background = _failColor;
                                if (subsequentChangesMessage != "")
                                {
                                    subItem = AddTreeViewItem(firstSubItem, string.Format("Subsequent changes: {0}", subsequentChangesMessage));
                                    subItem.Background = _failColor;
                                }
                            }
                            else
                            {
                                // verification passed
                                if (subsequentChangesMessage != "")
                                {
                                    subItem = AddTreeViewItem(firstSubItem, "Signature verification: The revision of the document that was covered by this signature has not been altered; however, there have been subsequent changes to the document");
                                    subItem.Background = _warningColor;
                                    subItem = AddTreeViewItem(firstSubItem, string.Format("Subsequent changes: {0}", subsequentChangesMessage));
                                    subItem.Background = _warningColor;
                                }
                                else
                                {
                                    subItem = AddTreeViewItem(firstSubItem, "Signature verification: Document has not been modified since this signature was applied");
                                    subItem.Background = _successColor;
                                }
                            }
                        }
                    }
                    // if signature verification is NOT successful
                    else
                    {
                        subItem = AddTreeViewItem(firstSubItem, "Signature verification: Document has been modified or corrupted since it was signed");
                        subItem.Background = _failColor;
                    }

                    // if signature has embedded timestamp
                    if (signature.HasEmbeddedTimeStamp)
                    {
                        if (embeddedTimestampVerifyResult)
                        {
                            // verification passed
                            subItem = AddTreeViewItem(firstSubItem, "Timestamp verification: Timestamp is valid");
                            subItem.Background = _successColor;
                        }
                        else
                        {

                            // verification falied
                            subItem = AddTreeViewItem(firstSubItem, "Timestamp verification: Timestamp is corrupted");
                            subItem.Background = _failColor;
                        }
                    }


                    // show certificate verification details

                    if (certChain != null)
                    {
                        // if certificate verification is successful 
                        if (certificateVerifyResult)
                        {
                            subItem = AddTreeViewItem(firstSubItem, "Certificate verification: Signer's certificate is valid");
                            subItem.Background = _successColor;
                        }
                        // if certificate verification is NOT successful 
                        else
                        {
                            subItem = AddTreeViewItem(firstSubItem, "Certificate verification: Signer's certificate is invalid");
                            subItem.Background = _warningColor;
                            foreach (X509ChainStatus status in certChain.ChainStatus)
                                AddTreeViewItem(subItem, string.Format("{0}: {1}", status.Status, status.StatusInformation));
                        }

                        if (hasTimestampCertificate)
                        {
                            // if certificate verification is successful 
                            if (timestampCertificateVerifyResult)
                            {
                                subItem = AddTreeViewItem(firstSubItem, "Timestamp certificate verification: Signer's certificate is valid");
                                subItem.Background = _successColor;
                            }
                            // if certificate verification is NOT successful 
                            else
                            {
                                subItem = AddTreeViewItem(firstSubItem, "Timestamp certificate verification: Signer's certificate is invalid");
                                subItem.Background = _warningColor;
                                foreach (X509ChainStatus status in timestampCertChain.ChainStatus)
                                    AddTreeViewItem(subItem, string.Format("{0}: {1}", status.Status, status.StatusInformation));
                            }
                        }
                    }

                    firstSubItem.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// Builds the X509 chain.
        /// </summary>
        /// <param name="signingCertificateChain">The signing certificate chain.</param>
        /// <param name="useSigningCertificateChain">Use certificate chain from signature to build and verify certificate chain (no revocation check).</param>
        /// <param name="certChain">The cert chain.</param>
        /// <returns><b>True</b> if the X.509 certificate is valid; otherwise, <b>false</b>.</returns>
        private bool BuildX509Chain(X509Certificate2[] signingCertificateChain, bool useSigningCertificateChainToBuildChain, out X509Chain certChain)
        {
            // signing certificate
            X509Certificate2 certificate = signingCertificateChain[0];
            // 
            // root certificate of signing certificate chain
            X509Certificate2 signingCertificateChainRoot = signingCertificateChain[signingCertificateChain.Length - 1];

            // need use signing certificate chain from signature
            if (signingCertificateChain.Length == 1)
                useSigningCertificateChainToBuildChain = false;

            // create X509Chain
            certChain = new X509Chain();

            // if need use signing certificate chain from signature
            if (useSigningCertificateChainToBuildChain)
            {
                // add certificate chain to ExtraStore of certChain
                for (int j = signingCertificateChain.Length - 1; j > 0; j--)
                    certChain.ChainPolicy.ExtraStore.Add(signingCertificateChain[j]);

                // no revocation check is performed on the certificate
                certChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                // ignore that the chain cannot be verified due to an unknown certificate authority (CA)
                certChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            }

            // build and verify certificate chain
            bool certificateVerifyResult = certChain.Build(certificate);

            // if need use signing certificate chain from signature and 
            if (useSigningCertificateChainToBuildChain && certificateVerifyResult)
            {
                // check builded chainRoot and root certificate of signing certificate chain from signature 
                if (certChain.ChainElements.Count < signingCertificateChain.Length)
                {
                    certificateVerifyResult = false;
                }
                else
                {
                    X509Certificate2 chainRoot = certChain.ChainElements[signingCertificateChain.Length - 1].Certificate;
                    if (!chainRoot.RawData.SequenceEqual(signingCertificateChainRoot.RawData))
                        certificateVerifyResult = false;
                }
            }

            return certificateVerifyResult;
        }

        /// <summary>
        /// Handles the Click event of the "Save Document Revision" button.
        /// </summary>
        private void saveDocumentRevisionButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "PDF Documents (*.pdf)|*.pdf";
            try
            {
                if (saveFile.ShowDialog().Value)
                {
                    PdfDocumentRevision revision = (PdfDocumentRevision)((TreeViewItem)signaturesTreeView.SelectedItem).Tag;
                    revision.CopyRevisionTo(saveFile.FileName);
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>       
        private void UpdateUI()
        {
            if (signaturesTreeView.SelectedItem != null &&
                ((TreeViewItem)signaturesTreeView.SelectedItem).Tag is PdfDocumentRevision)
            {
                PdfDocumentRevision revision = (PdfDocumentRevision)((TreeViewItem)signaturesTreeView.SelectedItem).Tag;
                saveDocumentRevisionButton.Content = string.Format("Save Document Resivion {0} As...", revision.RevisionNumber);
                saveDocumentRevisionButton.IsEnabled = true;
            }
            else
            {
                saveDocumentRevisionButton.Content = "Save Document Resivion As...";
                saveDocumentRevisionButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Builds the tree view for signatures.
        /// </summary>
        private void BuildSignaturesTreeView()
        {
            signaturesTreeView.Items.Clear();

            // if document has interactive form
            if (_document.InteractiveForm != null)
            {
                // get signature fields of PDF document
                PdfInteractiveFormSignatureField[] signatureFields = _document.InteractiveForm.GetSignatureFields();
                // for each signature field
                for (int i = 0; i < signatureFields.Length; i++)
                {
                    PdfInteractiveFormSignatureField signatureField = signatureFields[i];

                    // get signature information
                    PdfSignatureInformation signatureInfo = signatureFields[i].SignatureInfo;
                    _signatureInfos.Add(signatureInfo);

                    // text of signature header
                    string signatureText = "";

                    // if signature has information about document revision
                    if (signatureInfo != null && signatureInfo.SignedRevision != null)
                        signatureText += string.Format("{0}: ", signatureInfo.SignedRevision);

                    // signature field fully qualified name
                    signatureText += string.Format("{0}: ", signatureField.FullyQualifiedName);

                    // get PKCS signature
                    PdfPkcsSignature signature = null;
                    if (signatureInfo == null)
                    {
                        signatureText += "Unsigned Signature Field";
                    }
                    else
                    {
                        try
                        {
                            signature = signatureInfo.GetSignature();
                            signatureText += ConvertCertificateToString(signature.SigningCertificate);
                        }
                        catch (Exception ex)
                        {
                            signatureText += string.Format("Not supported or corrupted signature: {0}", ex.Message);
                        }
                    }
                    _signatures.Add(signature);

                    // add signature header text to tree view
                    TreeViewItem item = AddTreeViewItem(signaturesTreeView, signatureText);
                    item.Tag = signatureField;

                    // if signature field has signature info
                    if (signatureInfo != null)
                    {
                        if (signature != null)
                        {
                            if (signature.IsSigningImplemented)
                                AddTreeViewItem(signaturesTreeView.Items[i], "Signature is not verified");
                            else
                                AddTreeViewItem(signaturesTreeView.Items[i], "Signing is not implemented");
                        }

                        // add signature details
                        TreeViewItem signatureDetailsNode = new TreeViewItem();
                        signatureDetailsNode.Header = "Signature Details";
                        ((TreeViewItem)signaturesTreeView.Items[i]).Items.Add(signatureDetailsNode);

                        if (signature != null)
                        {
                            if (signature.IsTimeStamp || signature.HasEmbeddedTimeStamp)
                            {
                                TreeViewItem timestampDetails = null;
                                if (signature.IsTimeStamp)
                                {
                                    timestampDetails = AddTreeViewItem(signatureDetailsNode, "Signature is a document timestamp signature");
                                }
                                else if (signature.HasEmbeddedTimeStamp)
                                {
                                    timestampDetails = AddTreeViewItem(signatureDetailsNode, "Signature includes an embeded timestamp");
                                    if (signature.TimestampCertificate != null)
                                    {
                                        AddTreeViewItem(timestampDetails, string.Format("Timestamp certificate: {0}", ConvertCertificateToString(signature.TimestampCertificate)));
                                        TreeViewItem timestampCertificateDetails = AddTreeViewItem(timestampDetails, "Timestamp certificate details...");
                                        timestampCertificateDetails.Tag = signature.TimestampCertificate;

                                        if (signature.TimestampCertificateChain.Length > 1)
                                        {
                                            AddCerteficateChainToTreeView(signature.TimestampCertificateChain, AddTreeViewItem(timestampDetails, "Timestamp Certificate Chain"));
                                        }
                                    }
                                }

                                AddTreeViewItem(timestampDetails, string.Format("Timestamp Date: {0}", signature.TimeStampDate));
                                timestampDetails.IsExpanded = true;
                            }
                        }

                        if (signature != null)
                        {
                            TreeViewItem certificateDetails = AddTreeViewItem(signatureDetailsNode, "Certificate Details...");
                            certificateDetails.Tag = signature.SigningCertificate;
                            AddTreeViewItem(signatureDetailsNode, string.Format("Signature Algorithm: {0}", signature.SignatureAlgorithmName));

                            if (signature.SigningCertificateChain.Length > 1)
                            {
                                AddCerteficateChainToTreeView(signature.SigningCertificateChain, AddTreeViewItem(signatureDetailsNode, "Certificate Chain"));
                            }
                        }

                        // add signature information details
                        AddTreeViewItem(signatureDetailsNode, string.Format("Filter: {0} ({1})", signatureInfo.Filter, signatureInfo.SubFilter));
                        if (signatureInfo.SignerName != null)
                            AddTreeViewItem(signatureDetailsNode, string.Format("Name of Signer: {0}", signatureInfo.SignerName));
                        if (signatureInfo.Reason != null)
                            AddTreeViewItem(signatureDetailsNode, string.Format("Reason: {0}", signatureInfo.Reason));
                        if (signatureInfo.ContactInfo != null)
                            AddTreeViewItem(signatureDetailsNode, string.Format("Contact Info: {0}", signatureInfo.ContactInfo));
                        if (signatureInfo.Location != null)
                            AddTreeViewItem(signatureDetailsNode, string.Format("Location: {0}", signatureInfo.Location));
                        if (signatureInfo.SigningTime != DateTime.MinValue)
                            AddTreeViewItem(signatureDetailsNode, string.Format("Signing Time: {0}", signatureInfo.SigningTime));
                        if (signatureInfo.BuildProperties != null)
                        {
                            if (signatureInfo.BuildProperties.Application != null)
                            {
                                if (signatureInfo.BuildProperties.Application.Name != null)
                                    AddTreeViewItem(signatureDetailsNode, string.Format("Application: {0}", signatureInfo.BuildProperties.Application.Name));
                            }
                        }
                    }

                    if (signatureField.Annotation.Page != null)
                        AddTreeViewItem(signaturesTreeView.Items[i], string.Format("Page: {0}", _document.Pages.IndexOf(signatureField.Annotation.Page) + 1));

                    ((TreeViewItem)signaturesTreeView.Items[i]).IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// Adds the certeficate chain to TreeView.
        /// </summary>
        /// <param name="certificateChain">The certificate chain.</param>
        /// <param name="treeNode">The tree node.</param>
        private void AddCerteficateChainToTreeView(X509Certificate2[] certificateChain, TreeViewItem treeNode)
        {
            treeNode.IsExpanded = true;
            for (int j = certificateChain.Length - 1; j >= 0; j--)
            {
                AddTreeViewItem(treeNode, ConvertCertificateToString(certificateChain[j]));
                ((TreeViewItem)treeNode.Items[0]).Tag = certificateChain[j];
                treeNode = (TreeViewItem)treeNode.Items[0];
                treeNode.IsExpanded = true;
            }
        }

        /// <summary>
        /// Converts the certificate to string.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        private string ConvertCertificateToString(X509Certificate2 certificate)
        {
            string email = certificate.GetNameInfo(X509NameType.EmailName, false);
            if (email != "")
                return string.Format("{0} <{1}>", certificate.GetNameInfo(X509NameType.SimpleName, false), email);
            else
                return certificate.GetNameInfo(X509NameType.SimpleName, false);
        }

        /// <summary>
        /// Handles the NodeMouseClick event of the signaturesTreeView control.
        /// </summary>
        private void signaturesTreeView_SelectedItemChanged(
            object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                TreeViewItem item = (TreeViewItem)e.NewValue;
                if (item.Tag != null)
                {
                    if (item.Tag is X509Certificate2)
                        X509Certificate2UI.DisplayCertificate((X509Certificate2)item.Tag);
                }
            }
            UpdateUI();
        }

        /// <summary>
        /// Adds the item to the tree view.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="header">The header.</param>
        private TreeViewItem AddTreeViewItem(object root, string header)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = header;
            ((ItemsControl)root).Items.Add(item);
            return item;
        }

        #endregion

    }
}
