using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
//using Emgu.CV;
//using Emgu.CV.Structure;
using HumanDetector.vendors.include.singleton;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Numerics;



namespace HumanDetector.src.Classes
{
    class Camera
    {
        #region Variables

        public VideoCapture? m_Capture = null;
        public uint m_FPS = 30;

        public uint m_PictureWidth = 640;
        public uint m_PictureHeight = 420;

        #endregion

        // Get camera name list from WMI (there is probably a lib for this too but this seemed pretty lightweight and the fact that Microsoft even made a tool for it makes me more confident.
        public List<string> QueryWMI()
        {


            List<string> cameraDictionary = new List<string> { }; 
            try
            {
                ManagementObjectSearcher cameraSearcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'Camera'"); // Following instruction was generated with WMICodeCreator.exe found this tool in this thread  https://stackoverflow.com/questions/18407548/how-to-detect-cameras-attached-to-my-machine-using-c-sharp 


                var cameraIndex = 0;
             
                foreach (ManagementObject queryObj in cameraSearcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Win32_PnPEntity instance");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("PNPClass: {0}", queryObj["PNPClass"]);
                    Console.WriteLine($"Camera {cameraIndex}: {queryObj["Name"]}");
                    cameraDictionary.Add(queryObj["Name"].ToString());


                    cameraIndex++;
                }

            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }

            return cameraDictionary;
        }
    }
}
