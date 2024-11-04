using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;

using OpenCvSharp;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing;

using Point = OpenCvSharp.Point;// have to do this cuz Point is ambigious with System.Drawing :c ohh no
using Microsoft.ML.Data; 

namespace src.Classes.FrameWorks
{
    internal class MicrosoftML : AIManager
    {
        Globals ref_Globals = Instance<Globals>.Get();

        protected Point m_TargetPictureSizeToProcess = new Point(640, 640);
        protected float m_MinimumConfidance = 80.0f;
        protected override string m_FrameWorkName => "MicrosoftML"; // name used for printing and debug

        private MLContext m_mlContext = new MLContext();
        private PredictionEngine<ImageInputData, ModelOutput> m_PredictionEngine;


        public MicrosoftML(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance) {

            m_TargetPictureSizeToProcess = TargetPictureSizeToProcess;
            m_MinimumConfidance = MinimumConfidance;
        }

        public override void RunModel(Mat mat)
        {
            if (m_mlContext == null || m_PredictionEngine == null) return;
            if (ref_Globals.m_ShouldProcessFrames.IsCancellationRequested) return;


            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat); // convert to bitmap so ML.NET can use it 
            //var prediction = m_PredictionEngine.Predict(bitmap);
            //Console.WriteLine(prediction);
        }

        public override bool LoadModel()
        {
            string modelPath = "";
            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0:
                    break;
                case 1: // yolo8
                    modelPath = ref_Globals.m_BaseModelPath + "onnx\\yolov8n-face.onnx";

                    /*
                     * IN ORDER FOR THIS TO WORK USE https://netron.app/ AND DROP YOUR ONNX MODEL INTO IT AND CHECK INPUT AND OUTPUT NAME! OR ELSE IT WILL CRASH
                     */
                    var pipeline = m_mlContext.Transforms.ResizeImages(outputColumnName: "output0", imageWidth: m_TargetPictureSizeToProcess.X, imageHeight: m_TargetPictureSizeToProcess.Y, inputColumnName: nameof(ImageInputData.images));
                    pipeline.Append(m_mlContext.Transforms.ExtractPixels("images"));
                    pipeline.Append(m_mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, inputColumnNames: new[] { "images" }, outputColumnNames: new[] { "output0" }));
                    var emptyData = m_mlContext.Data.LoadFromEnumerable(Array.Empty<ImageInputData>());
                    var model = pipeline.Fit(emptyData);

                    m_PredictionEngine = m_mlContext.Model.CreatePredictionEngine<ImageInputData, ModelOutput>(model);

                    break;
                case 2: // yolo 11
                    
                    break;
                case 3: //
                   
                    break;

                default:
                    break;

            }
            return true;
        }


        public class ImageInputData
        {
            [ColumnName("images")]
            [ImageType(640, 640)]
            public MLImage images { get; set; } // class data have to match data got from the onnx file itself
        }

        public class ModelOutput // 
        {
            //public string PredictedLabel { get; set; }
            [ColumnName("output0")]
            public float[] Score { get; set; }
            //public float[] output0 { get; set; } // our output is output0 name: output0 tensor: float32[1, 5, 6300] so we need a 2 dimensional array. thanks https://netron.app/ :D
        }

       
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }
    }
}
