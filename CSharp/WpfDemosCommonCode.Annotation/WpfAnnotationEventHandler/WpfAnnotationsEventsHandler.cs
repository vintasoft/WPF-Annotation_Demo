using System;
using System.Windows.Input;

using Vintasoft.Imaging.Annotation;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Annotation.Wpf.UI;


namespace WpfDemosCommonCode.Annotation
{
    /// <summary>
    /// Class that handles all events of annotation and annotation collection.
    /// </summary>
    public class WpfAnnotationsEventsHandler
    {

        #region Fields

        WpfAnnotationViewer _annotationViewer;
        bool _handleDataEvents;
        bool _handleViewEvents;

        #endregion



        #region Constructor

        public WpfAnnotationsEventsHandler(
            WpfAnnotationViewer annotationViewer,
            bool handleDataEvents,
            bool handleViewEvents)
        {
            _annotationViewer = annotationViewer;
            _handleViewEvents = handleViewEvents;
            _handleDataEvents = handleDataEvents;
            _annotationViewer.AnnotationDataControllerChanged += new PropertyChangedEventHandler<AnnotationDataController>(AnnotationViewer_AnnotationsDataChanged);
            _annotationViewer.AnnotationViewCollectionChanged += new EventHandler<WpfAnnotationViewCollectionChangedEventArgs>(AnnotationViewer_SelectedAnnotationViewCollectionChanged);

            if (_annotationViewer.AnnotationViewCollection != null)
                SubscribeToAnnotationDataCollectionEvents(_annotationViewer.AnnotationViewCollection.DataCollection);
        }

        #endregion



        #region Properties

        bool _isEnabled = false;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }

        protected WpfAnnotationViewer Viewer
        {
            get
            {
                return _annotationViewer;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        protected virtual void OnAnnotationDataControllerChanged(WpfAnnotationViewer viewer, PropertyChangedEventArgs<AnnotationDataController> e)
        {
        }

        protected virtual void OnSelectedAnnotationViewCollectionChanged(WpfAnnotationViewer viewer, PropertyChangedEventArgs<WpfAnnotationViewCollection> e)
        {
        }

        protected virtual void OnDataCollectionChanging(AnnotationDataCollection annotationDataCollection, CollectionChangeEventArgs<AnnotationData> e)
        {
        }

        protected virtual void OnDataCollectionChanged(AnnotationDataCollection annotationDataCollection, CollectionChangeEventArgs<AnnotationData> e)
        {
        }

        protected virtual void OnAnnotationDataPropertyChanging(AnnotationDataCollection annotationDataCollection, AnnotationData annotationData, ObjectPropertyChangingEventArgs e)
        {
        }

        protected virtual void OnAnnotationDataPropertyChanged(AnnotationDataCollection annotationDataCollection, AnnotationData annotationData, ObjectPropertyChangedEventArgs e)
        {
        }

        protected virtual void OnMouseWheel(WpfAnnotationView annotationView, MouseEventArgs e)
        {
        }

        protected virtual void OnMouseUp(WpfAnnotationView annotationView, MouseEventArgs e)
        {
        }

        protected virtual void OnMouseDown(WpfAnnotationView annotationView, MouseEventArgs e)
        {
        }

        protected virtual void OnMouseMove(WpfAnnotationView annotationView, MouseEventArgs e)
        {
        }
        
        protected virtual void OnMouseEnter(WpfAnnotationView annotationView, MouseEventArgs e)
        {
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Handles the AnnotationsDataChanged event of AnnotationViewer object.
        /// </summary>
        private void AnnotationViewer_AnnotationsDataChanged(object sender, PropertyChangedEventArgs<AnnotationDataController> e)
        {
            OnAnnotationDataControllerChanged((WpfAnnotationViewer)sender, e);
        }

        /// <summary>
        /// Handles the SelectedAnnotationViewCollectionChanged event of AnnotationViewer object.
        /// </summary>
        private void AnnotationViewer_SelectedAnnotationViewCollectionChanged(object sender, PropertyChangedEventArgs<WpfAnnotationViewCollection> e)
        {
            if (e.OldValue != null)
                UnubscribeFromAnnotationDataCollectionEvents(e.OldValue.DataCollection);

            if (e.NewValue != null)
                SubscribeToAnnotationDataCollectionEvents(e.NewValue.DataCollection);

            if (_isEnabled)
                OnSelectedAnnotationViewCollectionChanged((WpfAnnotationViewer)sender, e);
        }

        private void SubscribeToAnnotationDataCollectionEvents(AnnotationDataCollection dataCollection)
        {
            dataCollection.Changing += new CollectionChangeEventHandler<AnnotationData>(dataCollection_Changing);
            dataCollection.Changed += new CollectionChangeEventHandler<AnnotationData>(dataCollection_Changed);

            if (_handleDataEvents)
            {
                dataCollection.ItemPropertyChanging += new EventHandler<AnnotationDataPropertyChangingEventArgs>(dataCollection_ItemPropertyChanging);
                dataCollection.ItemPropertyChanged += new EventHandler<AnnotationDataPropertyChangedEventArgs>(dataCollection_ItemPropertyChanged);
            }

            if (_handleViewEvents)
            {
                WpfAnnotationViewCollection viewCollection = _annotationViewer.AnnotationViewController.GetAnnotationsView(dataCollection);
                foreach (WpfAnnotationView view in viewCollection)
                    SubscribeToAnnotationViewEvents(view);
            }
        }

        private void UnubscribeFromAnnotationDataCollectionEvents(AnnotationDataCollection dataCollection)
        {
            if (_handleViewEvents)
            {
                WpfAnnotationViewCollection viewCollection = _annotationViewer.AnnotationViewController.GetAnnotationsView(dataCollection);
                foreach (WpfAnnotationView view in viewCollection)
                    UnsubscribeFromAnnotationViewEvents(view);
            }

            if (_handleDataEvents)
                dataCollection.ItemPropertyChanged -= new EventHandler<AnnotationDataPropertyChangedEventArgs>(dataCollection_ItemPropertyChanged);

            dataCollection.Changing -= new CollectionChangeEventHandler<AnnotationData>(dataCollection_Changing);
            dataCollection.Changed -= new CollectionChangeEventHandler<AnnotationData>(dataCollection_Changed);
        }


        private void SubscribeToAnnotationViewEvents(WpfAnnotationView annotationView)
        {
            annotationView.MouseDown +=new MouseButtonEventHandler(annotationView_MouseDown);
            annotationView.MouseEnter += new MouseEventHandler(annotationView_MouseEnter);
            annotationView.MouseLeave += new MouseEventHandler(annotationView_MouseLeave);
            annotationView.MouseMove  += new MouseEventHandler(annotationView_MouseMove);
            annotationView.MouseUp += new MouseButtonEventHandler(annotationView_MouseUp);
            annotationView.MouseWheel += new MouseWheelEventHandler(annotationView_MouseWheel);
        }

        private void UnsubscribeFromAnnotationViewEvents(WpfAnnotationView annotationView)
        {
            annotationView.MouseDown -= new MouseButtonEventHandler(annotationView_MouseDown);
            annotationView.MouseEnter -= new MouseEventHandler(annotationView_MouseEnter);
            annotationView.MouseLeave -= new MouseEventHandler(annotationView_MouseLeave);
            annotationView.MouseMove -= new MouseEventHandler(annotationView_MouseMove);
            annotationView.MouseUp -= new MouseButtonEventHandler(annotationView_MouseUp);
            annotationView.MouseWheel -= new MouseWheelEventHandler(annotationView_MouseWheel);
        }

        /// <summary>
        /// Handles the Changing event of DataCollection object.
        /// </summary>
        private void dataCollection_Changing(object sender, CollectionChangeEventArgs<AnnotationData> e)
        {
            if (_isEnabled)
                OnDataCollectionChanging((AnnotationDataCollection)sender, e);
        }

        /// <summary>
        /// Handles the Changed event of DataCollection object.
        /// </summary>
        private void dataCollection_Changed(object sender, CollectionChangeEventArgs<AnnotationData> e)
        {
            if (e.Action == CollectionChangeActionType.InsertItem || e.Action == CollectionChangeActionType.SetItem)
            {
                AnnotationDataCollection dataCollection = (AnnotationDataCollection)sender;
                WpfAnnotationViewCollection viewCollection = _annotationViewer.AnnotationViewController.GetAnnotationsView(dataCollection);
                SubscribeToAnnotationViewEvents(viewCollection.FindView(e.NewValue));
            }
            if (_isEnabled)
                OnDataCollectionChanged((AnnotationDataCollection)sender, e);
        }

        /// <summary>
        /// Handles the ItemPropertyChanging event of DataCollection object.
        /// </summary>
        private void dataCollection_ItemPropertyChanging(object sender, AnnotationDataPropertyChangingEventArgs e)
        {
            if (_isEnabled)
                OnAnnotationDataPropertyChanging((AnnotationDataCollection)sender, e.AnnotationData, e.ChangingArgs);
        }

        /// <summary>
        /// Handles the ItemPropertyChanged event of DataCollection object.
        /// </summary>
        private void dataCollection_ItemPropertyChanged(object sender, AnnotationDataPropertyChangedEventArgs e)
        {
            if (_isEnabled)
                OnAnnotationDataPropertyChanged((AnnotationDataCollection)sender, e.AnnotationData, e.ChangedArgs);
        }

        /// <summary>
        /// Handles the MouseWheel event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseWheel((WpfAnnotationView)sender, e);
        }

        /// <summary>
        /// Handles the MouseUp event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseUp((WpfAnnotationView)sender, e);
        }

        /// <summary>
        /// Handles the MouseMove event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseMove((WpfAnnotationView)sender, e);
        }

        /// <summary>
        /// Handles the MouseLeave event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseWheel((WpfAnnotationView)sender, e);
        }

        /// <summary>
        /// Handles the MouseEnter event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseEnter((WpfAnnotationView)sender, e);
        }

        /// <summary>
        /// Handles the MouseDown event of AnnotationView object.
        /// </summary>
        private void annotationView_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isEnabled)
                OnMouseDown((WpfAnnotationView)sender, e);
        }

        #endregion

        #endregion

    }
}
