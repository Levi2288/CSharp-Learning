//using OpenCvSharp.Dnn;
using OpenCvSharp;
using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;
using HumanDetector.src.Misc;


using Point = OpenCvSharp.Point;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using Tensorflow;

using Utils = HumanDetector.src.Misc.Utils;

namespace src.Classes.FrameWorks
{
    internal class OpenCV_DNN : AIManager
    {

        Globals ref_Globals = Instance<Globals>.Get();
        protected Point m_TargetPictureSizeToProcess = new Point(640, 640);
        protected float m_MinimumConfidance = 80.0f;
        protected override string m_FrameWorkName => "OpenCV"; // name used for printing and debug
        private CascadeClassifier m_Cascade;
        private Emgu.CV.Dnn.Net? net;

        public OpenCV_DNN(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance){
            m_TargetPictureSizeToProcess = TargetPictureSizeToProcess;
            m_MinimumConfidance = MinimumConfidance;
        }


        public Emgu.CV.Mat ConvertOpenCvSharpMatToEmguMat(OpenCvSharp.Mat openCvMat)
        {
            // Step 1: Convert OpenCvSharp.Mat to System.Drawing.Bitmap
            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(openCvMat);

            // Step 2: Convert Bitmap to Emgu.CV.Mat
            Emgu.CV.Mat emguMat = new Emgu.CV.Mat();
            emguMat = BitmapToEmguMat(bitmap);

            return emguMat;
        }

        public Emgu.CV.Mat BitmapToEmguMat(Bitmap bitmap)
        {
            return new Emgu.CV.Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3);
        }

        public override void RunModel(Mat mat)
        {
   
            if (ref_Globals.m_ShouldProcessFrames.IsCancellationRequested) return; // return if we should not process frames :D
            if (m_Cascade == null) return;
            if (ref_Globals.m_SelectedModelIndex != 0 && net == null) return;
            if (!Instance<Utils>.Get().CheckMatValidity(mat)) return;


            Rect[] faces = new Rect[] {};

            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0: // haar cascade
                    var grayFrame = new Mat();
                    Cv2.CvtColor(mat, grayFrame, ColorConversionCodes.BGR2GRAY);
                    Cv2.Resize(grayFrame, grayFrame, new OpenCvSharp.Size(m_TargetPictureSizeToProcess.X, m_TargetPictureSizeToProcess.Y));
                    faces = m_Cascade.DetectMultiScale(grayFrame, 1.1, 3, HaarDetectionTypes.ScaleImage);

                    break;
                case 1: // yolo8 // doesnt work for some reason

                   if (net is not Emgu.CV.Dnn.Net netObj)
                        Console.WriteLine("net is not Net?");


                    var blob = Emgu.CV.Dnn.DnnInvoke.BlobFromImage(ConvertOpenCvSharpMatToEmguMat(mat),
                        1.0 / 255.0,              
                        new System.Drawing.Size(640, 640), 
                        new MCvScalar(0, 0, 0), 
                        swapRB: true,            
                        crop: false, 
                        ddepth: Emgu.CV.CvEnum.DepthType.Cv32F
                    );

                    net.SetInput(blob);
                    var output = net.Forward();

    
                    float[] outputData = new float[1 * 5 * 8400];
                    output.CopyTo(outputData); // Copy output data to an array
                    var rows = output.GetShape();

                    int numPredictions = 8400;  // Number of possible detections
                    int dimensionsPerPrediction = 5;  // [x_center, y_center, width, height, confidence]

                    List<float[]> boxes = new List<float[]>();
                    List<float> scores = new List<float>();
                    List<int> classIds = new List<int>();

                    for (int i = 0; i < numPredictions; i++)
                    {
                        // Extract the values for each detection
                        float xCenter = outputData[0 * (dimensionsPerPrediction * numPredictions) + 0 * numPredictions + i];
                        float yCenter = outputData[0 * (dimensionsPerPrediction * numPredictions) + 1 * numPredictions + i];
                        float width = outputData[0 * (dimensionsPerPrediction * numPredictions) + 2 * numPredictions + i];
                        float height = outputData[0 * (dimensionsPerPrediction * numPredictions) + 3 * numPredictions + i];
                        float confidence = outputData[0 * (dimensionsPerPrediction * numPredictions) + 4 * numPredictions + i];

                        // Calculate class scores
                        float[] classScores = outputData.Skip(5 * numPredictions + i).Take(numPredictions - 5).ToArray();

                        // Find maximum score among classes
                        float maxScore = classScores.Max();
                        int maxClassIndex = Array.IndexOf(classScores, maxScore);

                        // Apply confidence threshold
                        if (maxScore >= 0.25f)
                        {
                            // Calculate the top-left corner of the bounding box
                            float xLeft = xCenter - (0.5f * width);
                            float yTop = yCenter - (0.5f * height);

                            // Save the box, score, and class ID
                            boxes.Add(new float[] { xLeft, yTop, width, height });
                            scores.Add(maxScore);
                            classIds.Add(maxClassIndex);
                        }
                    }



                    // Iterate through the reshaped output data to extract detection information



                    /*mat = Cv2.ImRead("E:\\!Levi\\VisualStudio\\#Other Projects\\C#\\HumanDetector\\vendors\\models\\testPic.jpg");
                    Mat blob = CvDnn.BlobFromImage(mat, 1.0/255.0, new Size(640,640), swapRB:true, crop: false);
                    //Cv2.ImShow("Test", blob);
                    net.SetInput(blob);

                    var layerNames = net.GetLayerNames();
                    var outputLayers = new List<string>();
                    List<Mat> detections = new List<Mat> (); 
                    for (int i = 0; i < layerNames.Count(); i++)
                    {
                        if (net.GetUnconnectedOutLayers().Contains(i + 1)) // +1 because OpenCV uses 1-indexing for layers
                        {
                            outputLayers.Add(layerNames[i]);
                            detections.Add(net.Forward(layerNames[i]));
                            Console.WriteLine(detections);
                        } 
                           

                    }

                    foreach (var data in detections)
                    {

                        //float[] detection = new float[data.Cols];
                        //data.GetArray(i, detection);
                        float confidence = data.At<float>(4);
                       // if (confidence > 0.5) // Set confidence threshold
                        //{
                            int centerX = (int)(data.At<float>(0) * mat.Width);
                            int centerY = (int)(data.At<float>(1) * mat.Height);
                            int width = (int)(data.At<float>(2) * mat.Width);
                            int height = (int)(data.At<float>(3) * mat.Height);

                            // Calculate box coordinates
                            int x = centerX - width / 2;
                            int y = centerY - height / 2;

                            // Draw bounding box
                            Cv2.Rectangle(mat, new Rect(x, y, width, height), new Scalar(0, 255, 0), 2);
                       // }
                    
                    }*/



                    ///net.SetPreferableBackend(0); // default backend
                    //net.SetPreferableTarget(0); // target CPU
                    //var output = net.Forward("output0"); // if it crashes here make sure the size is correct in the BlobFromImage call!

                    /*if (output.Empty())
                    {
                        Console.WriteLine("Error: The output is empty. Check model format and output layer name.");
                        return;
                    }
                    //Cv2.Transpose(output);
                    //var output2 = output.Transpose();
                    //Mat output2 = new Mat();
                    //Cv2.Transpose(output, output2);
                    float[] detectionArray = new float[output.Total()];
                    if (output.GetArray(out detectionArray))
                        Console.WriteLine("Success");*/


                    break;
                case 2: // yolo 11

                    break;
                case 3: //


                    break;

                default:
                    return;

            }

            ref_Globals.m_HumanPresent = faces.Length > 0 ? true : false;


            // Draw rectangles around detected faces
            foreach (var face in faces)
            {
            
                // draw text
                OpenCvSharp.Size TextSize = Cv2.GetTextSize(m_FrameWorkName + " | " + ref_Globals.m_AvailableModelNames[0], OpenCvSharp.HersheyFonts.HersheySimplex, 0.7, 1, out int baseLine);
                Cv2.Rectangle(mat, new Point(face.X, face.Y - 25), new Point(face.X + TextSize.Width, face.Y), OpenCvSharp.Scalar.Red, -1);
                Cv2.PutText(mat, m_FrameWorkName + " | " + ref_Globals.m_AvailableModelNames[ref_Globals.m_SelectedModelIndex], new Point(face.X, face.Y - 5), OpenCvSharp.HersheyFonts.HersheySimplex, 0.7, OpenCvSharp.Scalar.White, 2);

                // draw rect
                Cv2.Rectangle(mat, face, OpenCvSharp.Scalar.Red, 2);


            }
        }

        public override bool LoadModel()
        {
            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0:
                    m_Cascade = new CascadeClassifier(ref_Globals.m_BaseModelPath + "opencv-cascade\\haarcascade_frontalface_alt.xml");
                    break;
                case 1: // yolo8

                   // net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\yolov8n-face.onnx");
                    net = Emgu.CV.Dnn.DnnInvoke.ReadNetFromONNX(ref_Globals.m_BaseModelPath + "onnx\\yolov8n-face.onnx");


                    //net = CvDnn.ReadNetFromTorch(ref_Globals.m_BaseModelPath + "pytorch\\yolov8l-face.pt");

                    break;
                case 2: // yolo 11
                    //net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\yolov11n-face.onnx");
                    break;
                case 3: //
                    //net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\arcfaceresnet100-8.onnx");
                    break;

                default:
                    break;

            }
            return true;
        }
    }
}
