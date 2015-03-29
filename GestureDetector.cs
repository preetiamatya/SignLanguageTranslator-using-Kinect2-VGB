namespace SignLanguageTranslatorVGB
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using System.Linq;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System.IO;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Globalization;

    /// <summary>
    /// This class gets the frame from Kinect input using VGB API.
    /// The gesture name from VGB application are read as gestures in this class.
    /// Also, this class updates the gestures received from VGB.   
    /// </summary>
    public class GestureDetector : IDisposable
    {
        private static int count = 1;
        private static string[] KINECT_COORDINATES = new string[0];
        private Body[] bodies = null;
        private string finalgesture;
        private DTW _dtw;
        static List<string> _list = new List<string>();
        private static int EMPTY = 0; // Initialization for empty body coordinates
        private static int RECORD = 1; // Initialization to record coordinates
        private static int RECORD_BODY_DATA = EMPTY;// flag to record body data

        private static string DETECTED_GESTURE;

        // Path to the gesture database that was trained with VGB
        private readonly string gestureDatabase = @"D:\ToSubmit_Github\Sign Language Translator using Kinect2-VGB\SignLanguageTranslatorVGB-WPF\Database\DGS-SignLanguageTranslatorAADBMSVDUIG.gbd";
        private VisualGestureBuilderFrameSource vgbFrameSource = null;
        /// <summary> It handles the frames using VGB API </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;


        /// <summary>
        ///Initialization of the gesture detector class. The GestureResultView has the discrete results for the gesture detector
        /// </summary>
        /// <param name="kinectSensor">The active Kinect Sensor</param>
        /// <param name="gestureResultView">It is an object which stores the gesture results</param>     
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            verifyKinectSensorExists(kinectSensor);
            assureGestureResultViewNotNull(gestureResultView);
            this.GestureResultView = gestureResultView;
            // A valid body frame is the one with the valid tracking ID
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the trained gesture using VGB from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                // Load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures),     
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    this.vgbFrameSource.AddGesture(gesture);
                }
            }
        }

        // This function assures if the gestures stored are not null 
        private static void assureGestureResultViewNotNull(GestureResultView gestureResultView)
        {
            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }
        }
        // This function checks if the Kinect Sensor is connected to the PC
        private static void verifyKinectSensorExists(KinectSensor kinectSensor)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
        }

        /// <summary> 
        ///It stores the results which are to be displayed in the Presentation Layer
        ///</summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// The tracking id changes whenever the body is detected from a not detected state
        /// Handles TrackingID assosiated with the Kinect Sensor
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// When Tracking ID is not valid the detector is paused      
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes the objects which are not handled
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the objects which are related with the VGB API
        /// </summary>
        /// <param name="disposing">To dispose the objects it is set as true</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        /// <summary>
        /// This functions handles the results of the frame with gesture names
        /// A valid tracking ID is associated with the frames
        /// </summary>
        /// <param name="sender">Object which sends the events</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
        
            this.GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }

        /// <summary>
        /// This function reads every input frame from Kinect using VisualGestureBuilder API
        /// A valid frame is stored in an array as soon as it is received
        /// recordbodydata is a flag that is a boundary condition to record the coordinates to know about the status of last detected frame.
        /// The 'result' is set to true when gesture received from frame matches either start or end position of gestures
        /// The function passes to DTW for sequence matching when the start and end position are of same gesture.
        /// </summary>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            if (RECORD_BODY_DATA == RECORD)
            {
                storebodycoordinates();
            }
            else
            {
                clearstoredframes();
            }

            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    if (discreteResults != null)
                    {
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);
                                if (result != null)
                                {
                                    this.GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                                }

                                try
                                {
                                    if (result != null && result.Detected == true)
                                    {
                                        //conditional statement for the detected gesture
                                        System.Diagnostics.Debug.WriteLine("starttimeVGB" + DateTime.Now.ToString("ss.fff", CultureInfo.InvariantCulture));
                                        System.Diagnostics.Debug.WriteLine(gesture.Name + " " + count.ToString());
                                        System.Diagnostics.Debug.WriteLine("endtimeVGB" + DateTime.Now.ToString("ss.fff", CultureInfo.InvariantCulture));
                                        count += 1;
                                        if (isStartGesture(gesture.Name))
                                        {
                                            DETECTED_GESTURE = gesture.Name;
                                            storebodycoordinates(); //stores current frame
                                            RECORD_BODY_DATA = RECORD;
                                        }
                                        else if (DETECTED_GESTURE != String.Empty && DETECTED_GESTURE != null)
                                        {
                                            if (isEndGestureofCurrentGesture(DETECTED_GESTURE, gesture.Name))
                                            {
                                                storebodycoordinates();
                                                string gestureFilepath = ("D:\\DTW_files\\VGB\\" + gesture.Name + ".txt");
                                                string gestureName = passToDTW(gestureFilepath);
                                                GestureStore ig = new GestureStore(gestureName);
                                                DETECTED_GESTURE = String.Empty;
                                                RECORD_BODY_DATA = EMPTY;
                                            }
                                        }
                                    }
                                }

                                catch (Exception ex)
                                {
                                    // do nothing
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This function gets the body joint coordinates.
        /// Stores the current body frame into a static array.
        /// When a new frame arrives the arraylist merges to an existing one.      
        /// </summary>
        public void storebodycoordinates()
        {
            Body[] bodies = GetBody;
            try
            {
                for (int i = 0; i <= 5; i++)
                {
                    if (bodies[i].IsTracked == true)
                    {
                        var chosenbody = bodies[i].Joints;
                        for (int j = 0; j <= 24; j++)
                        {
                            //joints below hip region are ignored
                            if (j != 14 && j != 18 && j != 15 && j != 19 && j != 12 && j != 16 && j != 13 && j != 17)
                            {
                                string[] newarray = new string[0];
                                Joint bodyJoints = chosenbody.ElementAt(j).Value;
                                Body2D mb = new Body2D(bodyJoints);
                                int oldLength = KINECT_COORDINATES.Length;
                                try
                                {
                                    Array.Resize<string>(ref KINECT_COORDINATES, oldLength + mb.getArray().Length);
                                    Array.Copy(mb.getArray(), 0, KINECT_COORDINATES, oldLength, mb.getArray().Length);
                                }
                                catch (Exception ex)
                                { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }
        /// <summary>
        /// This function receives the Kinect Body joints in 3D      
        /// </summary>
        /// <returns>returns the body joint coordinates</returns>
        public Body[] GetBody
        {
            get
            {
                return this.bodies;
            }
            set
            {
                this.bodies = value;
            }
        }

        /// <summary>
        /// This function is called to get normalised result before passing to DTW algorithm.
        /// The input from the Kinect and the text files are normalised and passed into DTW.
        /// If the gesture name matches with the input, the name of gesture is received in the parameter finalgesture.
        /// </summary>
        ///  <param name="gesturename">It is used to describe the name of gesture in dataset with which the Body coordinates are going to be compared with</param>
        /// <returns>returns the gesture matched from DTW</returns>
        private string passToDTW(string gesturename)
        {
            try
            {
                Recorder recorder = new Recorder();
                ArrayList gesturefromkinect = recorder.readFiles(KINECT_COORDINATES);// getting normalised co ordinates here.....               
                string storedtextLines = System.IO.File.ReadAllText(gesturename);
                List<double[]> normalisedresult = new List<double[]>();

                string[] result = storedtextLines.Split('@');
                for (int i = 0; i < result.Length; i++)
                {
                    normalisedresult = recorder.explode_array(result, ',');
                }
                string txtFiles = Path.GetFileName(gesturename);

                int dimension = 12;
                int threshold = 3;
                int firstThreshold = 5;
                int maxSlope = 2;
                int minimumLength = 10;
                _dtw = new DTW(dimension, threshold, firstThreshold, maxSlope, minimumLength);
                ArrayList arrlstTemp = ArrayList.Adapter(normalisedresult);
                Regex regex = new Regex(@"_(.+?)_");
                MatchCollection mc2 = regex.Matches(txtFiles);
                _dtw.Add(arrlstTemp, "The gesture is " + mc2[0]);
                finalgesture = _dtw.recognize(gesturefromkinect);
                KINECT_COORDINATES = new string[] { };
            }
            catch (Exception ex)
            { }
            return finalgesture;
        }

        /// <summary>
        /// This function checks if an kinectinput is the start position of the Sign Language gesture       
        /// </summary>
        ///  <param name="gesturename">It is the start or end position of the gesturename read by VGB API</param>
        /// <returns>returns true if the frame contains start position of Sign Language gesture</returns>
        private bool isStartGesture(string gesturename)
        {
            string[] checkstart = gesturename.Split('_');
            if (checkstart[2] == "start")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This function checks if the kinectinput is the end position of last detected start position of Sign Language gesture       
        /// </summary>
        ///  <param name="gesture">It is the start position of the gesturename read by VGB API</param>
        /// <param name="tag">It is the end position of the gesturename read by VGB API</param>
        /// <returns>returns true if the frame contains start position and end position of the same Sign Language gesture</returns>
        private bool isEndGestureofCurrentGesture(string gesture, string tag)
        {
            if (gesture != String.Empty)
            {
                Regex r = new Regex(@"_(.+?)_");
                MatchCollection mc = r.Matches(gesture);
                MatchCollection mc1 = r.Matches(tag);
                if (tag.Contains("end") && mc[0].ToString() == mc1[0].ToString())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This function clears the stored frames in array to avoid stack overflow      
        /// </summary>
        private void clearstoredframes()
        {
            KINECT_COORDINATES = new string[] { };
        }
    }
}