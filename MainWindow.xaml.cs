///---------------------------------------------------------------------------------------------------
/// <Description>
/// This program is built using KinectSDK2.0
/// It can track upto six people but only one person's body joints are used in this application
/// The gestures which are recognized are displayed in the application as the form of the text
/// </Description>
///----------------------------------------------------------------------------------------------------

namespace SignLanguageTranslatorVGB
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /// <summary>
    /// The application logic for the Presentation layer.
    /// The human body form is displayed in this layer.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary> To find if Kinect is connected to run the application </summary>
        private KinectSensor kinectSensor = null;
        
        /// <summary> SDK 2.0 can track up to six people simultaneously, bodies array stores the body joints from Kinect</summary>
        private Body[] bodies = null;

        /// <summary>The datareader for the body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>To display whether application is running or kinect is not connected </summary>
        private string statusText = null;

        /// <summary> This object draws the Kinectbodies in the presentation layer</summary>
        private BodyView kinectBodyView = null;

        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        /// <summary>
        /// Application logic for the Main Window.
        /// SDK 2.0 can support one kinect sensor.
        /// </summary>
        public MainWindow()
        {
            
            this.kinectSensor = KinectSensor.GetDefault();
            
            //This event handler is set to know if the Kinect sensor status is changed
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // To open the sensor to detect the bodies
            this.kinectSensor.Open();

            // The status text to find the status of Kinect
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // open the body frame reader to read the data frames from Kinect
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // This event notifier is used to identify if the body frame has arrived
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // It initializes the bodyviewer object to display tracked bodies in Presentation Layer           
            this.kinectBodyView = new BodyView(this.kinectSensor);

            // initialize the gesture detection objects for the gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // initialize the MainWindow
            this.InitializeComponent();

            // The data context objects are set to display in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
            // The total bodies are six but we can use only one for the application
            
            int col0Row = 0;
            int col1Row = 0;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this.kinectSensor, result);
                this.gestureDetectorList.Add(detector);                
                // It is used for displaying gestures                 
                ContentControl contentControl = new ContentControl();
                contentControl.Content = this.gestureDetectorList[i].GestureResultView;
                
                if (i % 2 == 0)
                {
                    // For even number of bodies
                    Grid.SetColumn(contentControl, 0);
                    Grid.SetRow(contentControl, col0Row);
                    ++col0Row;
                }
                else
                {
                    // For odd number of bodies
                    Grid.SetColumn(contentControl, 1);
                    Grid.SetRow(contentControl, col1Row);
                    ++col1Row;
                }

               // this.contentGrid.Children.Add(contentControl);
            }
        }

        /// <summary>       
        /// Allows to bind the data when the change occurs
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// get or set to display the status text
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;                    
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// It is called when main window is closed.
        /// </summary>
        /// <param name="sender"> It is an object for sending elements</param>
        /// <param name="e">event arguments for an object</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                //calls body frame arrived
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
                // The GestureDetector is provided by VGB API for gesture detection
                foreach (GestureDetector detector in this.gestureDetectorList)
                {
                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.IsAvailableChanged -= this.Sensor_IsAvailableChanged;
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        ///  Event handler when the sensor becomes unavailable
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor and updates the associated gesture detector object for each body.
        /// On every frame received processes if the last detected gesture is recognised.
        /// Gets the gesture name of the last detected gesture.
        /// </summary>
        /// <param name="sender">is an object for sending elements</param>
        /// <param name="e">event arguments for an object</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            string is_gesture_recoginised = GestureStore.RECOGNIZED_GESTURE;
            if (!String.IsNullOrEmpty(is_gesture_recoginised))
            {
            }

            txtGesture.Text = is_gesture_recoginised;
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {                      
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    //Adds each body in an array
                    //These objects are used unless they are either set to null or empty                   
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                //Is called when a valid frame is received i.e it consists of all 25 joints in 3D                
                this.kinectBodyView.UpdateBodyFrame(this.bodies);             
                if (this.bodies != null)
                {
                    // This is done to find if the gesture detector needs to be updated
                    //Gets the current body frame
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;
                        var getbodies = this.bodies;

                        if (bodies[i].IsTracked == true)
                        {
                            this.gestureDetectorList[i].GetBody = getbodies;
                        }  
                        //Kinect sensor provides tracking ID for each body, When TrackingId changes the gesture detector needs to be updated with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // If current body gets tracked, it calls VisualGestureBuilderFrameArrived events                          
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }
    }
}
