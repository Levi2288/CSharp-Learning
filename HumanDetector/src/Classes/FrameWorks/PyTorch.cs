using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;
using OpenCvSharp.Dnn;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace src.Classes.FrameWorks
{
    internal class PyTorch : AIManager
    {
        Globals ref_Globals = Instance<Globals>.Get();
        protected override string m_FrameWorkName => "PyTorch"; // name used for printing and debug
        private CascadeClassifier m_Cascade;
        private Net? net;

        public PyTorch(Point TargetPictureSizeToProcess, float MinimumConfidance) : base(TargetPictureSizeToProcess, MinimumConfidance) { }

        public override void RunModel(Mat mat)
        {
        }

        public override bool LoadModel()
        {
            return true;
        }
    }
}
