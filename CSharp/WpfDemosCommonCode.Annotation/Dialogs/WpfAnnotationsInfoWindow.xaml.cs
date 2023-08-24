using System.Windows;

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

            string _type;
            public string Type
            {
                get
                {
                    return _type;
                }
                set
                {
                    _type = value;
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

            public AnnotationInfo(int pageNumber, string type, string location, string creationTime)
            {
                PageNumber = pageNumber;
                Type = type;
                Location = location;
                CreationTime = creationTime;
            }
        }

        #endregion



        #region Constructor

        public WpfAnnotationsInfoWindow(AnnotationDataController annotations)
        {
            InitializeComponent();
            for (int i = 0; i < annotations.Images.Count; i++)
            {
                for (int j = 0; j < annotations[i].Count; j++)
                {
                    AnnotationData annotation = annotations[i][j];
                    AnnotationInfo info = new AnnotationInfo(
                        i + 1,
                        annotation.GetType().ToString(),
                        annotation.Location.ToString(),
                        annotation.CreationTime.ToString());
                    listView1.Items.Add(info);
                }
            }
        }

        #endregion



        #region Methods

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
