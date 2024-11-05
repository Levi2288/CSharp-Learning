
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Threading;
using System.Text.RegularExpressions;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.Extensions; // Needed for ToBitmap()
using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;
using HumanDetector.src.Misc;
using static HumanDetector.src.Classes.Globals;
using static HumanDetector.src.Classes.AIManager;
using src.Classes.FrameWorks;

using MyTorch = src.Classes.FrameWorks.TorchSharp; // Alias the library namespace





namespace HumanDetector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /*
     * OpenCvSharp
     * ML.NET
     * Accord.NET
     * DlibDotNet
     * YOLOv4 and YOLOv5.NET
     * ONNX Runtime for .NET
     * SciSharp TensorFlow.NET
     * TorchSharp
     */

    public partial class MainWindow : System.Windows.Window
    {
        static Camera ref_CameraClass = Instance<Camera>.Get();
        static Globals ref_AIManagerClass = Instance<Globals>.Get();
        private VideoCapture _capture;
        private CancellationTokenSource _cancellationTokenSource;


        AIManager OpenCVDNNClass = new OpenCV_DNN(new OpenCvSharp.Point(640, 480), 80.0f);
        AIManager mlNetClass = new MicrosoftML(new OpenCvSharp.Point(640, 640), 80.0f);
        AIManager TorchSharpClass = new MyTorch(new OpenCvSharp.Point(640, 640), 80.0f);
        public MainWindow()
        {
            InitializeComponent();

            LoadRequiredModels();
            var data = Instance<Camera>.Get().QueryWMI();
            for(int i= 0; i < data.Count; i++)
            {
                cmbCameraSelector.Items.Add(data[i]);
            }
            //cmbCameraSelector.Items.Add("Test Value");
            cmbCameraSelector.SelectedIndex = 0;

            for (int i = 0; i < ref_AIManagerClass.m_AvailableModelNames.Length; i++)
            {
                cmbModelSelector.Items.Add(ref_AIManagerClass.m_AvailableModelNames[i]);
            }
            cmbModelSelector.SelectedIndex = 0;

            for (int i = 0; i < ref_AIManagerClass.m_AvailableFrameworks.Length; i++)
            {
                cmbFrameWorkSelector.Items.Add(ref_AIManagerClass.m_AvailableFrameworks[i]);
            }
            cmbFrameWorkSelector.SelectedIndex = 0;

        }

        //combo box data changed, we have to update capture
        private void cmbCameraSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // handle camera changing
           int resultIndex = cmbCameraSelector.SelectedIndex;
           UpdateCamera(resultIndex);


        }

        private void cmbModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // handle camera changing
            ref_AIManagerClass.m_ShouldProcessFrames.Cancel();
            ref_AIManagerClass.m_SelectedModelIndex = cmbModelSelector.SelectedIndex;


            LoadRequiredModels();



            ref_AIManagerClass.m_ShouldProcessFrames = new CancellationTokenSource();




        }

        private void cmbFrameWork_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ref_AIManagerClass.m_ShouldProcessFrames.Cancel();
            ref_AIManagerClass.m_SelectedFrameWorkIndex = cmbModelSelector.SelectedIndex;
          
            //ref_AIManagerClass.LoadModel();
            ref_AIManagerClass.m_ShouldProcessFrames = new CancellationTokenSource();
        }

        private void ProcessFrames(CancellationToken token)
        {
            var frame = new Mat();
      
            while (!token.IsCancellationRequested)
            {
                // Read a frame from the camera
                ref_CameraClass.m_Capture.Read(frame);

                if (ref_AIManagerClass.m_FrameWorks.OpenCV_DNN) OpenCVDNNClass.RunModel(frame); // Run model
                if (ref_AIManagerClass.m_FrameWorks.ML_NET) mlNetClass.RunModel(frame); // Run model
                if (ref_AIManagerClass.m_FrameWorks.TorchSharp) TorchSharpClass.RunModel(frame); // Run model




                if (frame.Empty())
                {
                    Console.WriteLine("Empty frame received. Stopping capture.");
                    break;
                }

                var currentFPS = ref_CameraClass.m_Capture.Get(VideoCaptureProperties.Fps); // cast to float cuz i prefer working with that
                imgFeedDisplay.Dispatcher.Invoke(() =>
                {
                    txtFPSDisplay.Text = "FPS: " + currentFPS.ToString("0.0");
                    imgFeedDisplay.Source = Instance<Utils>.Get().ConvertMatToBitmapSource(frame);
                    txtMainLabel.Text = ref_AIManagerClass.m_HumanPresent ? "Human Detected!" : "No Human Detected!";
                    txtMainLabel.Foreground = ref_AIManagerClass.m_HumanPresent ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                });


                // Optional delay to control frame rate (about 30 FPS)
                Thread.Sleep(5);
            }
            
        }


        private async void UpdateCamera(int selected)
        {


            if (ref_CameraClass.m_Capture != null)
            {
                if(_cancellationTokenSource != null)
                    _cancellationTokenSource.Cancel();
                ref_CameraClass.m_Capture.Release();
                ref_CameraClass.m_Capture.Dispose();
                ref_CameraClass.m_Capture = null;
                
            }

            // Initialize VideoCapture with selected camera index
            ref_CameraClass.m_Capture = new VideoCapture(selected, VideoCaptureAPIs.DSHOW); // if its not DSHOW it will not work for sum reason

            // Set custom properties if supported
            ref_CameraClass.m_Capture.Set(VideoCaptureProperties.Fps, ref_CameraClass.m_FPS);
            ref_CameraClass.m_Capture.Set(VideoCaptureProperties.FrameWidth, ref_CameraClass.m_PictureWidth);
            ref_CameraClass.m_Capture.Set(VideoCaptureProperties.FrameHeight, ref_CameraClass.m_PictureHeight);

            var Width = ref_CameraClass.m_Capture.Get(VideoCaptureProperties.FrameWidth);
            var Height =  ref_CameraClass.m_Capture.Get(VideoCaptureProperties.FrameHeight);

            Console.WriteLine($"Height: {Height} | Width: {Width}");


            if (!ref_CameraClass.m_Capture.IsOpened())
            {
                Console.WriteLine("Failed to open camera.");
                return;
            }

            // Start frame processing in a thread
            _cancellationTokenSource = new CancellationTokenSource();

            Thread thread = new Thread(() => ProcessFrames(_cancellationTokenSource.Token));
            thread.Start();



        }

        public void LoadRequiredModels()
        {
            // could use a loop here but for readability for now i will just do ifs
            if (ref_AIManagerClass.m_FrameWorks.OpenCV_DNN)
                OpenCVDNNClass.LoadModel();

            if (ref_AIManagerClass.m_FrameWorks.ML_NET)
                mlNetClass.LoadModel();

            if (ref_AIManagerClass.m_FrameWorks.TorchSharp)
                TorchSharpClass.LoadModel();
        }
    }
}