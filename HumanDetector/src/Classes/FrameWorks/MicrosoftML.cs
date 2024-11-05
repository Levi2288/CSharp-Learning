using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;

using System.IO;
using System.Drawing;
using OpenCvSharp;

using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.Transforms.Image;

using Point = OpenCvSharp.Point;// have to do this cuz Point is ambigious with System.Drawing :c ohh no
using Microsoft.ML.Data;

using Bitmap = System.Drawing.Bitmap;
using HumanDetector.src.Misc;

namespace src.Classes.FrameWorks
{
    internal class MicrosoftML : AIManager
    {
        Globals ref_Globals = Instance<Globals>.Get();

        protected new Point m_TargetPictureSizeToProcess = new Point(640, 640);
        protected new float m_MinimumConfidance = 80.0f;
        protected override string m_FrameWorkName => "MicrosoftML"; // name used for printing and debug

        private MLContext m_mlContext = new MLContext();
        private PredictionEngine<InputData, ModelOutput> m_PredictionEngine;
        private TransformerChain<OnnxTransformer?> m_Model;


        public MicrosoftML(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance) {

            m_TargetPictureSizeToProcess = TargetPictureSizeToProcess;
            m_MinimumConfidance = MinimumConfidance;
        }

        public override void RunModel(Mat mat)
        {
            if (m_mlContext == null || m_PredictionEngine == null) return;
            if (ref_Globals.m_ShouldProcessFrames.IsCancellationRequested) return;
            if (!Instance<Utils>.Get().CheckMatValidity(mat)) return;


            ModelOutput? prediction; // cant declare it in try catch


            try
            {
                System.Drawing.Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);

                bitmap = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size(m_TargetPictureSizeToProcess.X, m_TargetPictureSizeToProcess.Y));
                //byte[] imageBytes = BitmapToByteArray(bitmap);

    
 




                //IDataView scoredData = m_Model.Transform(testData);
                // Make a prediction
                prediction = m_PredictionEngine.Predict(new InputData { Image = bitmap });

                

                // Process prediction results here (e.g., visualize bounding boxes)
                Console.WriteLine(string.Join(",", prediction.PredictedBoxes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during prediction: {ex.Message}");
            }

            //Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat); // convert to bitmap so ML.NET can use it 
            //var Tensor = BitmapToTensor(bitmap, 640,640);
            // byte[] imageData;
            //Cv2.ImEncode(".bmp", mat, out imageData); // Use .bmp or another format

            // Step 2: Load byte array into a MemoryStream
            //using var memoryStream = new MemoryStream(imageData);
            
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
                    /*var pipeline = m_mlContext.Transforms.ResizeImages(outputColumnName: "output0", imageWidth: m_TargetPictureSizeToProcess.X, imageHeight: m_TargetPictureSizeToProcess.Y, inputColumnName: nameof(InputData.Image));
                    pipeline.Append(m_mlContext.Transforms.ExtractPixels(outputColumnName: "output0", inputColumnName:"images"));
                    pipeline.Append(m_mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, inputColumnNames: new[] { "images" }, outputColumnNames: new[] { "output0" }));
                   // var emptyData = m_mlContext.Data.LoadFromEnumerable(Array.Empty<ImageInputData>());
                    var emptyData = m_mlContext.Data.LoadFromEnumerable(new List<InputData>());
                    var model = pipeline.Fit(emptyData);

                    m_PredictionEngine = m_mlContext.Model.CreatePredictionEngine<InputData, ModelOutput>(model);*/

                    m_mlContext = new MLContext();
                    var emptyData = new List<InputData>();
                    var data = m_mlContext.Data.LoadFromEnumerable(emptyData);


                    var pipeline = m_mlContext.Transforms.ResizeImages(outputColumnName: "images", imageWidth: m_TargetPictureSizeToProcess.X, imageHeight: m_TargetPictureSizeToProcess.Y, inputColumnName: "images")
                    .Append(m_mlContext.Transforms.ExtractPixels(outputColumnName: "images"))
                    .Append(m_mlContext.Transforms.ApplyOnnxModel(
                        modelFile: modelPath,
                        inputColumnName: "images",
                        outputColumnName: "output0" ));



                    m_Model = pipeline.Fit(data);

                    m_PredictionEngine = m_mlContext.Model.CreatePredictionEngine<InputData, ModelOutput>(m_Model);
                    Console.WriteLine("Model loaded successfully.");

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


        public struct ModelSettings
        {
            // for checking TIny yolo2 Model input and  output  parameter names,
            //you can use tools like Netron,
            // which is installed by Visual Studio AI Tools

            // input tensor name
            //[ColumnName("images")]
            public const string ModelInput = "images";

            // output tensor name
            //[ColumnName("output0")]
            public const string ModelOutput = "output0";
        }


        public class InputData
        {
            [ColumnName("image")]
            [ImageType(640, 640)]

            //System.Drawing.Bitmap
            public System.Drawing.Bitmap Image { get; set; }
        }

        public class ModelOutput
        {
            [ColumnName("output0")]
            //[VectorType(1, 5, 8400)]
            public float[] PredictedBoxes;
        }


        public class DimensionsBase
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Height { get; set; }
            public float Width { get; set; }
        }
        public class BoundingBoxDimensions : DimensionsBase { }
        public class YoloBoundingBox
        {
            public BoundingBoxDimensions Dimensions { get; set; }

            public string Label { get; set; }

            public float Confidence { get; set; }

            public RectangleF Rect
            {
                get { return new RectangleF(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
            }
        }




        private byte[] BitmapToByteArray(System.Drawing.Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }


        /* private static InputData LoadImage(string imagePath)
         {
             var image = new Bitmap(imagePath);

             var resizedImage = new Bitmap(image, 640, 640); // Resize if necessary
                                                                       // Convert to float array and normalize
             var imageData = new float[640, 640, 3];
             for (int y = 0; y < resizedImage.Height; y++)
             {
                 for (int x = 0; x < resizedImage.Width; x++)
                 {
                     var pixel = resizedImage.GetPixel(x, y);
                     imageData[y, x, 0] = pixel.R / 255.0f; // Red channel
                     imageData[y, x, 1] = pixel.G / 255.0f; // Green channel
                     imageData[y, x, 2] = pixel.B / 255.0f; // Blue channel
                 }
             }
             return new InputData { Image = imageData };
         }*/
    }

    /*public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        public static Tensor<float> BitmapToTensor(Bitmap bitmap, int width, int height)
        {
            Bitmap resizedBitmap = new Bitmap(bitmap, new System.Drawing.Size(width, height));
            var bitmapData = resizedBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            float[] imageData = new float[width * height * 3];
            int index = 0;
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        imageData[index++] = ptr[2] / 255.0f; // R
                        imageData[index++] = ptr[1] / 255.0f; // G
                        imageData[index++] = ptr[0] / 255.0f; // B
                        ptr += 3;
                    }
                    ptr += bitmapData.Stride - (width * 3);
                }
            }
            resizedBitmap.UnlockBits(bitmapData);

            return new DenseTensor<float>(imageData, new[] { 1, 3, height, width });
        }
    }*/
}
