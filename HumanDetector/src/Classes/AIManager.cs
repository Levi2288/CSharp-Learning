using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Extensions;
using Tensorflow;
using TorchSharp;
using TorchSharp.Modules;


using static Tensorflow.Binding;
using static TorchSharp.torch.nn;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;


//https://github.com/quozd/awesome-dotnet#machine-learning-and-data-science
// https://netron.app/

namespace HumanDetector.src.Classes
{
    internal abstract class AIManager
    {
        public virtual void RunModel(Mat mat) { } // We are bigballers so we gonna use a template (PS: i removed templates because it would most likely impack performance and i would have to do additional checks in every RunModule call to verify or cast stuff.
                                                  // So i will just cast stuff in the function itself
        public virtual bool LoadModel(){return true;}

        protected Point m_TargetPictureSizeToProcess = new Point(640, 640);
        protected float m_MinimumConfidance = 80.0f;
        protected extern virtual string m_FrameWorkName { get; set; }


        public AIManager(Point TargetPictureSizeToProcess, float MinimumConfidance)
        {
            m_TargetPictureSizeToProcess = TargetPictureSizeToProcess;
            m_MinimumConfidance = MinimumConfidance;
        }


    }
    internal class Globals
    {

        public CancellationTokenSource m_ShouldProcessFrames = new CancellationTokenSource();
        //names mostly used for UI display and display while processing
        public string[] m_AvailableFrameworks = new string[] { "OpenCV DNN", "ML.NET", "ONNX Runtime", "TensorFlow", "Pytorch" };
        public string[] m_AvailableModelNames = new string[] { "Haar Cascade", "YOLO8", "YOLO11", "ResNet50", "VGGFace", "RetinaFace" };

        public string m_BaseModelPath = "E:\\!Levi\\VisualStudio\\#Other Projects\\C#\\HumanDetector\\vendors\\models\\";

        public int m_SelectedModelIndex = 0;
        public int m_SelectedFrameWorkIndex = 0;
        public bool m_HumanPresent = false;



        //struct to store multiple frameworks status (IK i could use a bool array too but this is way cleaner like this and i want to learn more about struct in C#
        public struct FrameWorks
        {
            public bool OpenCV_DNN { get; set; } // get and set stuff is pretty cool
            public bool ML_NET { get; set; }
            public bool TorchSharp { get; set; }

            public FrameWorks()
            {
                OpenCV_DNN = false;
                ML_NET = false;
                TorchSharp = true;
            }
        };

        public FrameWorks m_FrameWorks = new FrameWorks { };
      


        //TENSORFLOW.NET
        //private static string labelsPath = "models/coco-labels-paper.txt";
        //private static List<string> labels;
       // Session? session;

        //MICROSOFT.ML
        //private MLContext _mlContext;
        // private ITransformer _model;



        /*private static Tensor PrepareInputTensor(Mat frame)
        {
            // Resize and normalize the image as needed (depends on the model’s expected input size and format)
            Mat resizedFrame = new Mat();
            Cv2.Resize(frame, resizedFrame, new Size(640, 640)); // Adjust size as needed
            Cv2.CvtColor(resizedFrame, resizedFrame, ColorConversionCodes.BGR2RGB);

            // Convert Mat to byte array
            var inputArray = resizedFrame.ToBytes(".bmp"); // Convert to BMP format or raw pixel data, as required by the model

            // Create Tensor from byte array with the appropriate shape (e.g., [1, 640, 640, 3] for a batch of 1 image)
            return new Tensor(inputArray, new Shape(1, resizedFrame.Height, resizedFrame.Width, 3));
        }

        private static object[] DetectObjects(Mat frame, Session session)
        {
            // Prepare input tensor from OpenCV Mat
            Tensor inputTensor = PrepareInputTensor(frame);

                // Define inputs and outputs for session.Run()
                var inputDict = new FeedItem[]
                {
            new FeedItem(session.graph.OperationByName("image_tensor"), inputTensor)
                };

                var outputOperations = new Operation[]
                {
            session.graph.OperationByName("detection_boxes"),
            session.graph.OperationByName("detection_scores"),
            session.graph.OperationByName("detection_classes"),
            session.graph.OperationByName("num_detections")
                };

            // Run the session to get the output tensors
            var output = session.run(outputOperations, inputDict);

            // Convert outputs to arrays
            var boxes = output[0].ToArray<float>();
            var scores = output[1].ToArray<float>();
            var classes = output[2].ToArray<float>();
            var numDetections = output[3].ToArray<float>()[0]; // Number of detections is a single value

            return new object[] { boxes, scores, classes, numDetections };
        }*/
    }

    
}
