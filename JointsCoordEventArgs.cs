using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace SignLanguageTranslatorVGB
{  
    /// <summary>
    /// This class is used for conversion of the Kinect body joints into Arraylist for DTW processing
    /// </summary>
    internal class JointsCoordEventArgs
    {
        /// <summary>
        /// Defines the Hand Joints position
        /// </summary>
        private readonly Point[] _points;

        /// <summary>
        ///Initialization of the class
        /// </summary>
        /// <param name="points">Hand Joints</param>
        public JointsCoordEventArgs(Point[] points)
        {
            _points = (Point[]) points.Clone();
        }

        /// <summary>
        /// This function is used to get the point of the joints based upon the index
        /// </summary>
        /// <param name="index">Defines the index for the joints</param>
        /// <returns>Returns the body Joints</returns>
        public Point GetPoint(int index)
        {
            return _points[index];
        }

        /// <summary>
        /// Gets the cordintates of the hand joints.
        /// This function aligns X coordinates of hand into Arraylist with odd index and Y coordinates into even index.
        /// The array index is multiplied with 2 because we are using 2D coordinates.
        /// To compute 3D coordiinates, the constant 2 needs to be changed.
        /// </summary>
        /// <returns>The joint cordinates of the hand </returns>
        internal double[] GetCoordinates()
        {
            var tmp = new double[_points.Length * 2];
            for (int i = 0; i < _points.Length; i++)
            {
                tmp[2 * i] = _points[i].X;
                tmp[(2 * i) + 1] = _points[i].Y;
            }

            return tmp;
        }
    
    }
}
