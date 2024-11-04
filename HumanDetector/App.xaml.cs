#define DEBUG
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using HumanDetector.src.Classes;
using HumanDetector.vendors.include.singleton;

namespace HumanDetector
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Custom logic like QueryWMI if needed
            var data = Instance<Camera>.Get().QueryWMI();
            Console.WriteLine($"{data}");

        }

    }


}
