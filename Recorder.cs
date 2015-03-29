using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignLanguageTranslatorVGB
{
    /// <summary>
    /// This class is used to store the gestures read from the dataset(.txt) file into the array
    /// </summary>
    class Recorder
    {        
        private Body[] bodies = null;
        /// <summary>
        /// Reads the data from the .txt file and adds into the arraylist
        /// </summary>
        /// <param name="arr">Arraylist of data which are read from dataset</param>
        /// <param name="delimiter">The delimiter which is used to seperate the coorddinates</param>
        /// <returns>An arraylist for DTW comparision</returns>
        public List<double[]> explode_array(string[] arr, char delimiter)
        {
            string returnString = String.Empty;
            List<double[]> list = new List<double[]>();
            double[] result = { };
            arr = arr.Except(new string[] { "" }).ToArray();
            foreach (string el in arr)
            {
                result = Array.ConvertAll(el.Split(delimiter), Double.Parse);
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// This function reads the body cordinates from Kinect and normalizes the coordinates
        /// </summary>
        /// <param name="textLines">Joint cordinates which is provided by Kinect</param>
        /// <returns>Normalized coordinates in 2D</returns>
        public ArrayList readFiles(string[] textLines)
        {
            ArrayList pointsList = new ArrayList();
            JointsExtractor sd = new JointsExtractor();
            ArrayList refpoints = sd.normalizationCoordinates(textLines);
            foreach (Point[] refpoint in refpoints)
            {
                JointsCoordEventArgs converter = new JointsCoordEventArgs(refpoint);
                double[] gesturecapturedbykinect = converter.GetCoordinates();
                pointsList.Add(gesturecapturedbykinect);
            }
            return pointsList;
        }
       
    }
}
