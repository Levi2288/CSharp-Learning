using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanDetector.vendors.include.singleton
{
    // got a lil singleton class so in case i ever need it
    public class Instance<T> where T : new()
    {
        // Ensures a single instance of T
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

        public static T Get()
        {
            return _instance.Value;
        }

        // Prevent instantiation
        private Instance() { }
    }

}
