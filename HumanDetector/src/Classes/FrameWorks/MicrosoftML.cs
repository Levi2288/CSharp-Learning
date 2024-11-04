using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;

using OpenCvSharp;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp.Dnn;

namespace src.Classes.FrameWorks
{
    internal class MicrosoftML : AIManager
    {
        Globals ref_Globals = Instance<Globals>.Get();
        protected override string m_FrameWorkName => "MicrosoftML"; // name used for printing and debug

        private MLContext mlContext = new MLContext();


        public MicrosoftML(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance) { }

        public override void RunModel(Mat mat)
        {
        }

        public override bool LoadModel()
        {
            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0:
                    break;
                case 1: // yolo8

                    var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: 224, imageHeight: 224, inputColumnName: nameof(ImageInputData.Image));

                    break;
                case 2: // yolo 11
                    
                    break;
                case 3: //
                   
                    break;

                default:
                    break;

            }
            return true;
            return true;
        }
    }
}
