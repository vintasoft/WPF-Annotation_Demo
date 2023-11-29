using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation;


namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// A window that shows information about annotations.
    /// </summary>
    public partial class WpfAnnotationsInfoWindow : Window
    {

        #region Nested class

        class AnnotationInfo
        {
            int _pageNumber;
            public int PageNumber
            {
                get
                {
                    return _pageNumber;
                }
                set
                {
                    _pageNumber = value;
                }
            }

            string _name;
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            string _location;
            public string Location
            {
                get
                {
                    return _location;
                }
                set
                {
                    _location = value;
                }
            }

            string _creationTime;
            public string CreationTime
            {
                get
                {
                    return _creationTime;
                }
                set
                {
                    _creationTime = value;
                }
            }

            public AnnotationInfo(int pageNumber, string name, string location, string creationTime)
            {
                PageNumber = pageNumber;
                Name = name;
                Location = location;
                CreationTime = creationTime;
            }
        }

        #endregion



        #region Fields

        /// <summary>
        /// Dictionary: list item => annotation data.
        /// </summary>
        Dictionary<AnnotationInfo, AnnotationData> _annotationInfoToAnnotationData;

        /// <summary>
        /// Dictionary: annotation data => image.
        /// </summary>
        Dictionary<AnnotationData, VintasoftImage> _annotationDataToImage;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfAnnotationsInfoWindow"/> class.
        /// </summary>
        /// <param name="annotations">The annotations.</param>
        public WpfAnnotationsInfoWindow(AnnotationDataController annotations)
            : this(annotations, null, false, null, true, false, "Information about annotations")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfAnnotationsInfoWindow"/> class.
        /// </summary>
        /// <param name="annotationController">The annotation data controller.</param>
        /// <param name="annotationIntent">An intent to filter annotations.</param>
        /// <param name="needSelectAnnotations">A value indicating whether one or more annotations should be selected to proceed.</param>
        /// <param name="focusedAnnotationData">Currently focused annotation, which should be selected in list view.</param>
        /// <param name="showAnnotationType">A value indicating whether to show annotation type in "Annotation" column.</param>
        /// <param name="canSelectMultipleItems">A value indicating whether multiple items can be selected.</param>
        /// <param name="dialogTitle">Dialog title.</param>
        public WpfAnnotationsInfoWindow(
            AnnotationDataController annotationController,
            string annotationIntent,
            bool needSelectAnnotations,
            AnnotationData focusedAnnotationData,
            bool showAnnotationType,
            bool canSelectMultipleItems,
            string dialogTitle)
        {
            InitializeComponent();

            _annotationIntent = annotationIntent;
            _needSelectAnnotations = needSelectAnnotations;
            _showAnnotationType = showAnnotationType;
            _canSelectMultipleItems = canSelectMultipleItems;

            okButton.IsEnabled = !needSelectAnnotations;
            if (canSelectMultipleItems)
                annoInfoListView.SelectionMode = SelectionMode.Multiple;
            Title = dialogTitle;

            _annotationInfoToAnnotationData = new Dictionary<AnnotationInfo, AnnotationData>();
            _annotationDataToImage = new Dictionary<AnnotationData, VintasoftImage>();

            // for each image in annotation data controller
            for (int i = 0; i < annotationController.Images.Count; i++)
            {
                // create view group
                //ListViewGroup group = annoInfoListView.Groups.Add("pageNumber", "Page " + (i + 1));
                // get annotation collection for image
                AnnotationDataCollection annotations = annotationController[i];
                // for each annotation in annotation collection
                for (int j = 0; j < annotations.Count; j++)
                {
                    AnnotationData annotation = annotations[j];

                    // if annotation intent does not match
                    if (!string.IsNullOrEmpty(AnnotationIntent) && AnnotationIntent != annotation.Intent)
                        continue;

                    // get annotation type
                    string annotationType = annotation.GetType().Name;
                    if (annotationType.EndsWith("Data"))
                        annotationType = annotationType.Substring(0, annotationType.Length - 4);

                    // get annotation name
                    string annotationName = annotation.Name;

                    if (!string.IsNullOrEmpty(annotationName))
                        annotationName += " ";

                    if (ShowAnnotationType)
                        annotationName = string.Format("{0}({1})", annotationName, annotationType);

                    // create list item
                    AnnotationInfo infoItem = new AnnotationInfo(
                        i + 1,
                        annotationName,
                        annotation.Location.ToString(),
                        annotation.CreationTime.ToString());

                    annoInfoListView.Items.Add(infoItem);

                    // add information to dictionaries
                    _annotationInfoToAnnotationData.Add(infoItem, annotation);
                    _annotationDataToImage.Add(annotation, annotationController.Images[i]);

                    if (focusedAnnotationData == annotation)
                        annoInfoListView.SelectedItem = infoItem;
                }
            }
        }

        #endregion



        #region Properties

        string _annotationIntent = null;
        /// <summary>
        /// Gets the annotation intent, which is used as filter for shown annotations.
        /// </summary>
        /// <value>
        /// <b>null</b> - annotations are not filtered.
        /// Default value is <b>null.</b>
        /// </value>
        public string AnnotationIntent
        {
            get
            {
                return _annotationIntent;
            }
        }

        /// <summary>
        /// Gets a dictionary from selected annotation to an image.
        /// </summary>
        public Dictionary<AnnotationData, VintasoftImage> SelectedAnnotations
        {
            get
            {
                Dictionary<AnnotationData, VintasoftImage> result = new Dictionary<AnnotationData, VintasoftImage>();

                // add selected annotations to dictionary
                foreach (AnnotationInfo selectedItem in annoInfoListView.SelectedItems)
                {
                    AnnotationData annotationData = _annotationInfoToAnnotationData[selectedItem];
                    result.Add(annotationData, _annotationDataToImage[annotationData]);
                }

                return result;
            }
        }

        bool _needSelectAnnotations;
        /// <summary>
        /// Gets a value indicating whether one or more annotations should be selected to proceed.
        /// </summary>
        public bool NeedSelectAnnotations
        {
            get
            {
                return _needSelectAnnotations;
            }
        }

        bool _showAnnotationType;
        /// <summary>
        /// Gets a value indicating whether this form should show the annotation type in "Annotation" column.
        /// </summary>
        public bool ShowAnnotationType
        {
            get
            {
                return _showAnnotationType;
            }
        }

        bool _canSelectMultipleItems;
        /// <summary>
        /// Gets a value indicating whether this form allows to select multiple items.
        /// </summary>
        public bool CanSelectMultipleItems
        {
            get
            {
                return _canSelectMultipleItems;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the SelectionChanged event of AnnoInfoListView object.
        /// </summary>
        private void annoInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NeedSelectAnnotations)
                okButton.IsEnabled = annoInfoListView.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

    }
}
