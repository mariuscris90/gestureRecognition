
namespace DTWGestureRecognition
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private State
        private const int eachLabelRecordingNb = 4;
        private const int Ignore = 2;
        private const int BufferSize = 32;
        private const int MinimumFrames = 6;
        private const int CaptureCountdownSeconds = 3;
        private string GestureSaveFileLocation = Environment.CurrentDirectory;
        private const string GestureBodySaveFileNamePrefix = @"BodyGestures";
        private const string GestureFaceSaveFileNamePrefix = @"FaceGestures";
        private const string GestureStaticBodySaveFileNamePrefix = @"StaticBodyGestures";
        private const string GestureStaticFaceSaveFileNamePrefix = @"StaticFaceGestures";
        private string[] staticFaceGestures = {"Neutral", "Yawn", "Sad", "Happy", "ReiseEyeBrows" };
        private string[] staticFaceAttributes = { "BrowLower", "BrowRaiser", "JawLower", "LipCornerDepressor", "LipRaiser", "LipStretcher" };
        private string[] staticBodyAttributes = { "HandLeft_X", "HandLeft_Y", "HandLeft_Z", "WristLeft_X", "WristLeft_Y", "WristLeft_Z", "ElbowLeft_X", "ElbowLeft_Y", "ElbowLeft_Z", 
                                                     "ElbowRight_X", "ElbowRight_Y", "ElbowRight_Z", "WristRight_X", "WristRight_Y", "WristRight_Z", "HandRight_X", "HandRight_Y", "HandRight_Z"};
        private string[] staticBodyGestures = { "Left_Hand_Up", "Left_Hand_To_The_Left", "Left_Hand_To_The_Right", "Right_Hand_Up", "Right_Hand_To_The_Left", "Right_Hand_To_The_Right", "Both_Hands_Up", "Both_Hands_Down" };
        private bool staticFaceInit = false;
        private bool staticBodyInit = false;
        private Weka.BayesNaive acFace;
        private Weka.BayesNaive acBody;
        private bool _capturing;
        private DtwGestureRecognizer _dtw1;
        private DtwGestureRecognizer _dtw2;
        private StaticGestureDataExtract staticFaceExtractor;
        private StaticGestureDataExtract staticBodyExtractor;
        private int _flipFlop;
        private ArrayList _bodyVideo;
        private double[] auxBodyVideo={};
        private double[] auxFaceAnimationUnit = {};
        private ArrayList _faceVideo;
        
        private DateTime _captureCountdown = DateTime.Now;
        private System.Windows.Forms.Timer _captureCountdownTimer;
        private KinectSensor _Kinect;
        public static Skeleton[] _FrameSkeletons;
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        private MouseControl mouse;

        private SpeechRecognizer speechRecognizer;

        public static EnumIndexableCollection<FeaturePoint, PointF> facePointsS;
        EnumIndexableCollection<FeaturePoint, Vector3DF> facePoints3D;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        #endregion Private State
        #region ctor + Window Events
        public MainWindow()
        {
            InitializeComponent();

            var faceTrackingViewerBinding = new System.Windows.Data.Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            mouse = new MouseControl();
            sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {

            DiscoverKinectSensor(kinectChangedEventArgs);
            speechRecognizer = new SpeechRecognizer(this._Kinect);
            this.RecognizedSpeech.Text = speechRecognizer.getRecognizedSpeech();
            _dtw2 = new DtwGestureRecognizer(6, 0.45, 8, 3, 16);//face gesture dtw
            _dtw1 = new DtwGestureRecognizer(18, 0.9, 3, 3, 10);//body movements dtw
            staticFaceExtractor = new StaticGestureDataExtract();
            staticBodyExtractor = new StaticGestureDataExtract();
            _bodyVideo = new ArrayList();
            _faceVideo = new ArrayList();
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //kinect discovery and initialization


           
        }
        private void WindowClosed(object sender, EventArgs e)
        {
            this.Kinect = null;
            Environment.Exit(0);
        }
        #endregion ctor + Window Events
        #region Kinect discovery + set up
        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                //enable skeleton stream
                sensor.SkeletonStream.Enable();
                _FrameSkeletons = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
                sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(NuiSkeletonFrameReady);
                //enable color stream
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,
                                                colorStream.FrameHeight, 96, 96,
                                                PixelFormats.Bgr32, null);
                this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,
                colorStream.FrameHeight);
                this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                videoImage.Source = this._ColorImageBitmap;
                FaceImage.Source = this._ColorImageBitmap;
                sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(NuiColorFrameReady);
                //Dtw events
               sensor.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;
                Skeleton2DDataExtract.Skeleton2DdataCoordReady += NuiSkeleton2DdataCoordReady;
              //  sensor.Start();
            }

            if (sensor != null)
            {
                try
                {
                   // sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                       // sensor.DepthStream.Range = DepthRange.Near;
                       // sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                  //      sensor.DepthStream.Range = DepthRange.Default;
                  //      sensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                   //sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                   // sensor.SkeletonStream.Enable();
                   
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }
        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.SkeletonFrameReady -= NuiSkeletonFrameReady;
                sensor.ColorFrameReady -= NuiColorFrameReady;
            }
        }
        public KinectSensor Kinect
        {
            get { return this._Kinect; }
            set
            {
                if (this._Kinect != null)
                {
                    UpdateDisplayStatus("No connected device.");
                    UninitializeKinectSensor(this._Kinect);
                    this._Kinect = null;
                }
                if (value != null && value.Status == KinectStatus.Connected)
                {
                    this._Kinect = value;
                    InitializeKinectSensor(this._Kinect);
                    kinectStatus.Text = string.Format("{0} - {1}", this._Kinect.UniqueKinectId, this._Kinect.Status);
                }
                else
                {
                    UpdateDisplayStatus("No connected device.");
                }
            }
        }
        private void DiscoverKinectSensor( KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = kinectChangedEventArgs.NewSensor;
            SliderValue.Text = "Tilt angle : " + _Kinect.ElevationAngle;
            TiltSlider.Value = _Kinect.ElevationAngle;
           // this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                        UpdateDisplayStatus("Sensor connected.");
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        if (this.Kinect == null)
                        {
                            UpdateDisplayStatus("No connected device.");
                        }
                    }
                    break;
                //TODO: Handle all other statuses according to needs
            }
            if (e.Status == KinectStatus.Connected)
            {
                this.Kinect = e.Sensor;
            }
        }
        private void UpdateDisplayStatus(string message)
        {
            kinectStatus.Text = "Kinect: "+message;
        }
        #endregion Kinect discovery + set up
        #region KinectEventsMethods
        private void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
           
            facePoints3D = FaceTrackingViewer.facePointS3D;
            if (facePoints3D != null)
            {
                if (facePoints3D[FeaturePoint.AboveChin] != null)
                {
                    //Debug.WriteLine(FaceDataExtract.ProcessData(facePointsS)[1]);
                }
            }
            if (!(bool)seated.IsChecked)
            {
                _Kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }
            if (!(bool)faceTracking.IsChecked)
            {

                _Kinect.DepthStream.Disable();
            }
            else
            {
                this.faceOrientation.Text = "Face Orientation X = " + FaceTrackingViewer.x + " Y = " + FaceTrackingViewer.y + " Z = " + FaceTrackingViewer.z;
                //this.faceOrientationText.Text = " Face Orientation X = " + FaceTrackingViewer.x;
                var orientationString = FaceDataExtract.getFaceLookingPosition(FaceTrackingViewer.x, FaceTrackingViewer.y, FaceTrackingViewer.z);
                this.faceOrientationText.Text = "Face Orientation : "+orientationString;
                //mouse.moveMouse(-FaceTrackingViewer.x, -FaceTrackingViewer.y);

                

            }
            
            if ((bool)captureBody.IsChecked)
            {
                Debug.WriteLine(captureBody);
            }
            if (!(bool)faceMesh.IsChecked)
            {
                FaceTrackingViewer.meshDisabled = false;
            }
            else
            {
                FaceTrackingViewer.meshDisabled = true;
            }
            if (!(bool)nearTracking.IsChecked)
            {
                _Kinect.DepthStream.Range = DepthRange.Default;
                _Kinect.SkeletonStream.EnableTrackingInNearRange = false;
            }
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(_FrameSkeletons);
                    Skeleton data = (from s in _FrameSkeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();
                    if (data != null)
                    {
                        Brush brush = new SolidColorBrush(Colors.Blue);
                        skeletonCanvas.Children.Clear();
                        //Draw bones
                        if (!(bool)seated.IsChecked)
                        {
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.Spine, JointType.ShoulderCenter, JointType.Head));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight));
                        }
                        else
                        {
        
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush,  JointType.ShoulderCenter, JointType.Head));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft));
                            skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight));
                        }
                        // Draw joints
                        foreach (Joint joint in data.Joints)
                        {

                            System.Windows.Point jointPos = GetDisplayPosition(joint);
                                Ellipse ellipse = new Ellipse
                                {
                                    Fill = brush,
                                    Width = 10,
                                    Height = 10,
                                    Margin = new Thickness(jointPos.X, jointPos.Y, 0, 0)
                                };
                                skeletonCanvas.Children.Add(ellipse);        
                        }
                    }
                }
            }
        }
        private void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData,
                    this._ColorImageStride, 0);
                }
            }
            this.RecognizedSpeech.Text = speechRecognizer.getRecognizedSpeech();
        }
        private Polyline GetBodySegment(JointCollection joints, Brush brush, params JointType[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(GetDisplayPosition(joints[ids[i]]));
            }
            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 2;
            return polyline;
        }
        private System.Windows.Point GetDisplayPosition(Joint joint)
        {
            ColorImagePoint colorImgpoint = Kinect.MapSkeletonPointToColor(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);
            return new System.Windows.Point(colorImgpoint.X, colorImgpoint.Y);
        }
        #endregion KinectEventsMethods
        #region DTWGestureRecognition
        private bool LoadGesturesFromFile(string fileLocation, DtwGestureRecognizer dtw,int dim)
        {
            try
            {
                int itemCount = 0;
                string line;
                string gestureName = String.Empty;
                // TODO I'm defaulting this to 12 here for now as it meets my current need but I need to cater for variable lengths in the future
                ArrayList frames = new ArrayList();
                double[] items = new double[dim];
                // Read the file and display it line by line.
                System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("@"))
                    {
                        gestureName = line;
                        continue;
                    }
                    if (line.StartsWith("~"))
                    {
                        frames.Add(items);
                        itemCount = 0;
                        items = new double[dim];
                        continue;
                    }
                    if (!line.StartsWith("----"))
                    {
                        items[itemCount] = Double.Parse(line);
                    }
                    itemCount++;
                    if (line.StartsWith("----"))
                    {
                        dtw.AddOrUpdate(frames, gestureName, eachLabelRecordingNb);
                        frames = new ArrayList();
                        gestureName = String.Empty;
                        itemCount = 0;
                    }
                }
                file.Close();
            }
            catch (Exception exp)
            {
                return false;
            }
            return true;
        }
        private static void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(_FrameSkeletons);
                    Skeleton data = (from s in _FrameSkeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();
                    Skeleton2DDataExtract.ProcessData(data);
                }
            }
        }

        private void NuiSkeleton2DdataCoordReady(object sender, Skeleton2DdataCoordEventArgs a)
        {
            currentBufferFrame.Text = "Current Buffer Frame: "+ _bodyVideo.Count.ToString();
            // We need a sensible number of frames before we start attempting to match gestures against remembered sequences
            if (_bodyVideo.Count > MinimumFrames && _capturing == false)
            {
                ////Debug.WriteLine("Reading and video.Count=" + video.Count);
                string s = _dtw1.Recognize(_bodyVideo);
                results.Text = "Body movements: " + s;
                if ((bool)enableNui.IsChecked)
                {
                    GestureInterfaceControl.executeGesture(s);
                }
                if (!s.Contains("__UNKNOWN"))
                {
                    // There was no match so reset the buffer
                    _bodyVideo = new ArrayList();
                }
            }
            if (_faceVideo.Count > MinimumFrames && _capturing == false)
            {
                ////Debug.WriteLine("Reading and video.Count=" + video.Count);
                string s = _dtw2.Recognize(_faceVideo);
                faceResults.Text = "Face gesture: " + s;
                if ((bool)enableNui.IsChecked)
                {
                    GestureInterfaceControl.executeGesture(s);
                }
                if (!s.Contains("__UNKNOWN"))
                {
                    // There was no match so reset the buffer
                    _faceVideo = new ArrayList();
                }
            }
            // Ensures that we remember only the last x frames
            if (_bodyVideo.Count > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    if ((bool)captureBody.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }
                    else if ((bool)captureBodyS.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }
                 }
                else
                {
                    // Remove the first frame in the buffer
                    _bodyVideo.RemoveAt(0);
                }
            }
            if (_faceVideo.Count > BufferSize)
            {
                // If we are currently capturing and we reach the maximum buffer size then automatically store
                if (_capturing)
                {
                    if ((bool)captureFace.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }
                    else if ((bool)captureFaceS.IsChecked)
                    {
                        DtwStoreClick(null, null);
                    }

                }
                else
                {
                    // Remove the first frame in the buffer
                    _faceVideo.RemoveAt(0);
                }
            }
            
            // Decide which skeleton frames to capture. Only do so if the frames actually returned a number. 
            // For some reason my Kinect/PC setup didn't always return a double in range (i.e. infinity) even when standing completely within the frame.
            // TODO Weird. Need to investigate this

            double aux = a.GetPoint(0).X;
            if (!double.IsNaN(aux))
            {
                // Optionally register only 1 frame out of every n
                _flipFlop = (_flipFlop + 1) % Ignore;
                if (_flipFlop == 0)
                {
                    if ((bool)faceTracking.IsChecked)
                    {
                        if (FaceTrackingViewer.animationUnits != null)
                        {
                             auxFaceAnimationUnit = FaceDataExtract.ProcessData(FaceTrackingViewer.animationUnits);
                            _faceVideo.Add(auxFaceAnimationUnit);
                            for (int i = 0; i < auxFaceAnimationUnit.Length; i++)
                            {
                                auxFaceAnimationUnit[i] =(int)(auxFaceAnimationUnit[i]*10000);
                            }
                            if (staticFaceInit)
                            {
                                double[] percentages = new double[staticFaceGestures.Length];
                                acFace.ClassifyInstance(auxFaceAnimationUnit, out percentages);
                                double minF = 0;
                                int indexF = -1;
                                for (int i = 0; i < percentages.Length; i++)
                                {
                                   
                                    if (percentages[i] > minF)
                                    {
                                        minF = percentages[i];
                                        indexF = i;
                                    }
                                }
                                if (indexF != -1)
                                {
                                    faceResultsS.Text = "Face static gesture: " + staticFaceGestures[indexF];
                                }
                                else
                                {
                                    faceResultsS.Text = "Face static gesture: __UNKNOWN";
                                }
                            }
                        }
                    }
                    {
                        _bodyVideo.Add(a.GetCoords());
                        auxBodyVideo =  a.GetCoords();
                        for(int i=0;i<auxBodyVideo.Length;i++)
                        {
                            auxBodyVideo[i] = (int)(auxBodyVideo[i]*10000);
                        //    Debug.WriteLine(auxBodyVideo[i]);
                        }
                        if (staticBodyInit)
                        {
                            double[] percentages = new double[staticFaceGestures.Length];
                            acBody.ClassifyInstance(auxBodyVideo, out percentages);
                            double min = 0;
                            int index = -1;
                            for (int i = 0; i < percentages.Length; i++)
                            {
                              //  Debug.WriteLine(percentages[i]);
                                if (percentages[i] > min)
                                {
                                    min = percentages[i];
                                    index = i;
                                }                       
                            }
                            if (index != -1)
                            {
                                resultsS.Text = "Body static gesture: " + staticBodyGestures[index];
                            }
                            else
                            {
                                resultsS.Text = "Body static gesture: __UNKNOWN";
                            }
                        }
                    }
  
                    
                }
            }
            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }
        private void DtwReadClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;
            // Set the capturing? flag
            _capturing = false;
            // Update the status display
            status.Text = "Status: Reading";
        }
        private void DtwCaptureClick(object sender, RoutedEventArgs e)
        {
            _captureCountdown = DateTime.Now.AddSeconds(CaptureCountdownSeconds);
            _captureCountdownTimer = new System.Windows.Forms.Timer();
            _captureCountdownTimer.Interval = 50;
            _captureCountdownTimer.Start();
            _captureCountdownTimer.Tick += CaptureCountdown;
        }
        private void CaptureCountdown(object sender, EventArgs e)
        {
            if (sender == _captureCountdownTimer)
            {
                if (DateTime.Now < _captureCountdown)
                {
                    status.Text = "Status: Wait " + ((_captureCountdown - DateTime.Now).Seconds + 1) + " seconds";
                }
                else
                {
                    _captureCountdownTimer.Stop();
                    if ((bool)captureFace.IsChecked)
                    {
                        status.Text = "Status: Recording face movement";
                    }
                    else
                    {
                        status.Text = "Status: Recording gesture";
                    }
                    StartCapture();
                }
            }
        }
        private void StartCapture()
        {
            // Set the buttons enabled state
            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = false;
            dtwStore.IsEnabled = true;
            // Set the capturing? flag
            _capturing = true;

            ////_captureCountdownTimer.Dispose();
            if ((bool)captureFace.IsChecked)
            {
                status.Text = "Status: Recording face movement";
            }
            else
            {
                status.Text = "Status: Recording gesture";
            }
            // Clear the _video buffer and start from the beginning
            _bodyVideo = new ArrayList();
              _faceVideo = new ArrayList();
        }
        private void DtwStoreClick(object sender, RoutedEventArgs e)
        {
            // Set the buttons enabled state.
            DtwGestureRecognizer dtw;
            ArrayList _video;
            if ((bool)captureFace.IsChecked)
            {
                dtw = _dtw2;
                _video = _faceVideo;
                // Add the current video buffer to the dtw sequences list
                dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                faceResults.Text = "Gesture " + gestureList.Text + " added";
            } else if ((bool)captureBody.IsChecked)
            {
                dtw = _dtw1;
                _video = _bodyVideo;
                // Add the current video buffer to the dtw sequences list
                dtw.AddOrUpdate(_video, gestureList.Text, eachLabelRecordingNb);
                results.Text = "Gesture " + gestureList.Text + " added";
            }
            else if ((bool)captureFaceS.IsChecked)
            {
                staticFaceExtractor.AddOrUpdate(_faceVideo, gestureList.Text, eachLabelRecordingNb);
                 _faceVideo = new ArrayList();
                 _video = _faceVideo;
            }
            else if ((bool)captureBodyS.IsChecked)
            {
                staticBodyExtractor.AddOrUpdate(_bodyVideo, gestureList.Text, eachLabelRecordingNb);
                _bodyVideo = new ArrayList();
                _video = _bodyVideo;
            }

            dtwRead.IsEnabled = false;
            dtwCapture.IsEnabled = true;
            dtwStore.IsEnabled = false;
            // Set the capturing? flag
            _capturing = false;
            status.Text = "Status: Remembering " + gestureList.Text;
            // Scratch the _video buffer
            _video = new ArrayList();
            // Switch back to Read mode
            DtwReadClick(null, null);
        }
        private void DtwSaveToFile(object sender, RoutedEventArgs e)
        {
            if ((bool)captureFace.IsChecked)
            {
                string fileName = GestureFaceSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw2.RetrieveText());
            }
            else if ((bool)captureBody.IsChecked)
            {
                string fileName = GestureBodySaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, _dtw1.RetrieveText());
            }
            else if ((bool)captureFaceS.IsChecked)
            {
                string fileName = GestureStaticFaceSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".arff";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, staticFaceExtractor.RetrieveText(staticFaceAttributes, staticFaceGestures, "staticFaceGestures"));
            }
            else if ((bool)captureBodyS.IsChecked)
            {
                string fileName = GestureStaticBodySaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".arff";
                status.Text = "Status: Saved to " + fileName;
                System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, staticBodyExtractor.RetrieveText(staticBodyAttributes,staticBodyGestures, "staticBodyGestures"));  
            }
            else
            {
                status.Text = "Status: Please select what type of dynamic gestures to save";
            }
           
            
        }

        private void DtwLoadFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            _Kinect.ElevationAngle = 10;
            if ((bool)captureFaceS.IsChecked || (bool)captureBodyS.IsChecked)
            {
                dlg.DefaultExt = ".model";
                dlg.Filter = "Text documents (.model)|*.model";
            }
            else
            {
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents (.txt)|*.txt";
            }
            dlg.InitialDirectory = GestureSaveFileLocation;
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                if ((bool)captureBody.IsChecked)
                {
                   bool loaderResult = LoadGesturesFromFile(dlg.FileName, _dtw1, 18);
                   if (loaderResult)
                   {
                       status.Text = "Status: Body Gesture loaded";
                   }
                   else
                   {
                       status.Text = "Status: Invalid Body Gesture File";
                   }
                }
                else if ((bool)captureFace.IsChecked)
                {
                    bool loaderResult = LoadGesturesFromFile(dlg.FileName, _dtw2, 6);
                    if (loaderResult)
                    {
                        status.Text = "Status: Face Gesture loaded";
                    }
                    else
                    {
                        status.Text = "Status: Invalid Face Gesture File";
                    }
                }
                else if ((bool)captureFaceS.IsChecked)
                {        
                    acFace = new Weka.BayesNaive();
                    string[] attributes = staticFaceAttributes;
                    string[] gestures = staticFaceGestures;
                    string classAttribute = "gesture";
                    string modelLocation = dlg.FileName;
                    acFace.InitializeClassifier(attributes,gestures,classAttribute,modelLocation);
                    staticFaceInit = true;                  
                    status.Text = "The Weka model " + dlg.FileName + " was loaded";
                }
                else if ((bool)captureBodyS.IsChecked)
                {
                    acBody = new Weka.BayesNaive();
                    string[] atributes = staticBodyAttributes;
                    string[] gestures = staticBodyGestures;
                    string classAttribute = "gesture";
                    string modelLocation = dlg.FileName;
                    acBody.InitializeClassifier(atributes, gestures, classAttribute, modelLocation);
                    staticBodyInit = true;   
                    status.Text = "The Weka model " + dlg.FileName + " was loaded";
                }
                else
                {
                    status.Text = "Status: Please select what type of dynamic gestures to load";
                }
                //dtwTextOutput.Text = _dtw.RetrieveText();
               
            }
        }
        private void DtwShowGestureText(object sender, RoutedEventArgs e)
        {
            //dtwTextOutput.Text = _dtw.RetrieveText();
        }
        #endregion DTWGestureRecognition

        private void enableNui_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)faceTracking.IsChecked)
            {
               
                _Kinect.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            }
            else
            {

            }
        }

        private void seated_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)seated.IsChecked)
            {
                _Kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                Debug.WriteLine("seated");
            }
            else
            {
                 Debug.WriteLine("not seated");
            }
        }

        private void nearTracking_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)nearTracking.IsChecked)
            {
                _Kinect.DepthStream.Range = DepthRange.Near;
                _Kinect.SkeletonStream.EnableTrackingInNearRange = true;
            }
            else
            {
                _Kinect.DepthStream.Range = DepthRange.Default;
                _Kinect.SkeletonStream.EnableTrackingInNearRange = false;
            }
        }

        private void faceOrientation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }
        private void face_movement_checked(object sender, RoutedEventArgs e)
        {
        }
        private void checkBox1_Checked_1(object sender, RoutedEventArgs e)
        {
        }
        private void faceMesh_checked(object sender, RoutedEventArgs e)
        {
        }

        private void TiltSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)TiltSlider.Value;
            SliderValue.Text = "Tilt Angle : " + value + "";
            BackgroundWorker barInvoker = new BackgroundWorker();
            barInvoker.DoWork += delegate
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    _Kinect.ElevationAngle = value;
                   
                }
                catch (InvalidOperationException exp)
                {
                }
            };
            barInvoker.RunWorkerAsync();
        }
    }
}