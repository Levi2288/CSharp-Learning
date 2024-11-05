using Emgu.CV.Dnn;
using HumanDetector.src.Classes;
using HumanDetector.src.Misc;
using HumanDetector.vendors.include.singleton;


using OpenCvSharp;
using System.Windows.Input;
using TorchSharp;

namespace src.Classes.FrameWorks
{
    internal class TorchSharp : AIManager
    {
        Globals ref_Globals = Instance<Globals>.Get();
        protected override string m_FrameWorkName => "TorchSharp"; // name used for printing and debug
        private torch.jit.ScriptModule? m_Model;

        public TorchSharp(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance) { }



        static torch.Tensor MatToTensor(Mat mat)
        {
            // Assuming mat is a color image (3 channels)
            int height = mat.Rows;
            int width = mat.Cols;
            int channels = mat.Channels();

            // Convert the Mat to a float array, then to TorchSharp Tensor
            var data = new float[height, width, channels];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Vec3b pixel = mat.At<Vec3b>(h, w);
                    data[h, w, 0] = pixel.Item0 / 255.0f; // Blue
                    data[h, w, 1] = pixel.Item1 / 255.0f; // Green
                    data[h, w, 2] = pixel.Item2 / 255.0f; // Red
                }
            }

            // Create a TorchSharp Tensor from the data
            var tensor = torch.from_array(data).permute(2, 0, 1).unsqueeze(0); // Shape [1, 3, height, width]
            return tensor;
        }
        public override void RunModel(Mat mat)
        {
            if (m_Model == null) return;
            if (!Instance<Utils>.Get().CheckMatValidity(mat)) return;
            var tensor = MatToTensor(mat);

            var outputTensor = m_Model.forward(tensor);

            // Post-process the result (example)
            Console.WriteLine(outputTensor);


        }

        public override bool LoadModel()
        {

            torch.InitializeDeviceType(DeviceType.CPU); // this call needs torch CPU and cuda windows packages too
            switch (ref_Globals.m_SelectedModelIndex)
            {
                case 0:
                    break;
                case 1: // yolo8

                    // net = CvDnn.ReadNetFromOnnx(ref_Globals.m_BaseModelPath + "onnx\\yolov8n-face.onnx");
                    try
                    {
                        m_Model = torch.jit.load(ref_Globals.m_BaseModelPath + "pytorch\\yolov8l-face.pt"); // cant use not ScriptedModule whatever that means so convert model using a py script found here https://pytorch.org/tutorials/advanced/cpp_export.html#converting-to-torch-script-via-tracing
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading the model: {ex.Message}");
                        return false;
                    }



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
    }
}
