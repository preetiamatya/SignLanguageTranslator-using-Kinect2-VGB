namespace SignLanguageTranslatorVGB
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///This class stores the discrete gesture results from VGB API.
    ///The gestures are stored to display in the Presentation Layer.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {
        /// <summary>Kinect can track upto 6 bodies, colors are assigned to seperate bodies from each other </summary>
        private readonly Brush[] trackedColors = new Brush[] { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

        /// <summary>Defines the brush color to display in the Presentation Layer </summary>
        private Brush bodyColor = Brushes.White;

        /// <summary>Initialization of the BodyIndex </summary>
        private int bodyIndex = 0;

        /// <summary>Initialization of the confidence value</summary>
        private float confidence = 0.0f;

        /// <summary>Initialization for the detected gesture. It is set to true when gestures are detected</summary>
        private bool detected = false;
        
        /// <summary>Initialization for tracking body. It is true when body is tracked</summary>
        private bool isTracked = false;

        /// <summary>
        /// Initialization for GestureResultView class
        /// </summary>
        /// <param name="bodyIndex">The body index ranging from 0 - 5 for the detected bodies</param>
        /// <param name="isTracked">When the body is tracked it is set as true</param>
        /// <param name="detected">True, when gesture are detected using AdaBoostTrigger</param>
        /// <param name="confidence">Represents the confidence value of the detected gesture</param>
        public GestureResultView(int bodyIndex, bool isTracked, bool detected, float confidence)
        {
            this.BodyIndex = bodyIndex;
            this.IsTracked = isTracked;
            this.Detected = detected;
            this.Confidence = confidence;
        }
        /// <summary>
        /// It allows the window controls to handle the changable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Gets the body index from the gesture detector
        /// </summary>
        public int BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }

            private set
            {
                if (this.bodyIndex != value)
                {
                    this.bodyIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        ///Gets the body color depending upon the trackingid
        /// </summary>
        public Brush BodyColor
        {
            get
            {
                return this.bodyColor;
            }

            private set
            {
                if (this.bodyColor != value)
                {
                    this.bodyColor = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Checks whether the body is tracked or not
        /// </summary>
        public bool IsTracked 
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Checks whether the discrete gestures are being detected
        /// </summary>
        public bool Detected 
        {
            get
            {
                return this.detected;
            }

            private set
            {
                if (this.detected != value)
                {
                    this.detected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets the confidence value of the gestures
        /// </summary>
        public float Confidence
        {
            get
            {
                return this.confidence;
            }

            private set
            {
                if (this.confidence != value)
                {
                    this.confidence = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Updates the values of gestures after detection
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body is tracked using VGB API</param>
        /// <param name="isGestureDetected">True, if the gestures are detected</param>
        /// <param name="detectionConfidence">Confidence value for the detection of the gestures</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
        {
            this.IsTracked = isBodyTrackingIdValid;
            this.Confidence = 0.0f;

            if (!this.IsTracked)
            {
                this.Detected = false;
                this.BodyColor = Brushes.Gray;
            }
            else
            {
                this.Detected = isGestureDetected;
                this.BodyColor = this.trackedColors[this.BodyIndex];

                if (this.Detected)
                {
                    this.Confidence = detectionConfidence;
                }
            }
        }

        /// <summary>
        /// This function notifies UI when the property has been changed
        /// </summary>
        /// <param name="propertyName">It is the name of the property which has been changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
