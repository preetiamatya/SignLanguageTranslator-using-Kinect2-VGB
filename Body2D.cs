using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace SignLanguageTranslatorVGB
{
    /// <summary>
    /// This class is used to get only 2D body joints from Kinect Sensor
    /// </summary>
    class Body2D
    {
        private string[] array = new string[0];
        /// <summary>    
        /// This is a constructor which gets 2D body joints from Kinect and adds to the arraylist
        /// </summary>
        /// <param name="joint">It is the body joints from Kinect</param>
        public Body2D(Joint joint)
        {           
            var list = new List<string>();           
            list.Add(joint.JointType.ToString());
            list.Add(joint.Position.X.ToString());
            list.Add(joint.Position.Y.ToString());           
            array = list.ToArray(); 
        }
        public string[] getArray()
        {
            return array;
        }    
    }
}
