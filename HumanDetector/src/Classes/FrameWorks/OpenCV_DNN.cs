using OpenCvSharp.Dnn;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Tensorflow;
using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Tensorflow.NumPy;

namespace src.Classes.FrameWorks
{
    internal class OpenCV_DNN : AIManager
    {

        Globals ref_Globals = Instance<Globals>.Get();
        protected override string m_FrameWorkName => "OpenCV"; // name used for printing and debug
        private CascadeClassifier m_Cascade;
        private Net? net;

        public OpenCV_DNN(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance){}


      
        public override void RunModel(Mat mat)
        {
            if (ref_Globals.m_ShouldProcessFrames.IsCancellationRequested) return; // return if we should not process frames :D


            Rect[] faces = new Rect[] { };

            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0: // haar cascade
                    var grayFrame = new Mat();
                    Cv2.CvtColor(mat, grayFrame, ColorConversionCodes.BGR2GRAY);
                    Cv2.Resize(mat, grayFrame, new Size(m_TargetPictureSizeToProcess.X, m_TargetPictureSizeToProcess.Y));
                    faces = m_Cascade.DetectMultiScale(grayFrame, 1.1, 3, HaarDetectionTypes.ScaleImage);

                    break;
                case 1: // yolo8 // doesnt work for some reason
                    
                    Mat blob = CvDnn.BlobFromImage(mat, 1.0 / 255.0, new Size(m_TargetPictureSizeToProcess.X, m_TargetPictureSizeToProcess.Y), new Scalar(0, 0, 0), swapRB: true, crop: false);
                    net.SetInput(blob);
                    net.SetPreferableBackend(0); // default backend
                    net.SetPreferableTarget(0); // target CPU
                    var output = net.Forward("output0"); // if it crashes here make sure the size is correct in the BlobFromImage call!

                    if (output.Empty())
                    {
                        Console.WriteLine("Error: The output is empty. Check model format and output layer name.");
                        return;
                    }

                    float[] detectionArray = new float[output.Total()];
                    output.GetArray(out detectionArray);


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
                Size TextSize = Cv2.GetTextSize(m_FrameWorkName + " | " + ref_Globals.m_AvailableModelNames[0], OpenCvSharp.HersheyFonts.HersheySimplex, 0.7, 1, out int baseLine);
                Cv2.Rectangle(mat, new Point(face.X, face.Y - 25), new Point(face.X + TextSize.Width, face.Y), OpenCvSharp.Scalar.Red, -1);
                Cv2.Rectangle(mat, face, OpenCvSharp.Scalar.Red, 2);
                Cv2.PutText(mat, m_FrameWorkName + " | " + ref_Globals.m_AvailableModelNames[ref_Globals.m_SelectedModelIndex], new Point(face.X, face.Y - 5), OpenCvSharp.HersheyFonts.HersheySimplex, 0.7, OpenCvSharp.Scalar.White, 2);
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

                    net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\yolov8n.onnx");
                    //net = CvDnn.ReadNetFromTorch(ref_Globals.m_BaseModelPath + "pytorch\\yolov8l-face.pt");

                    break;
                case 2: // yolo 11
                    net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\yolov11n-face.onnx");
                    break;
                case 3: //
                    net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\arcfaceresnet100-8.onnx");
                    break;

                default:
                    break;

            }
            return true;
        }
    }
}
