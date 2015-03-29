//---------------------------------------------------------------------------------------------------
// <Description>
//This program is built by refering KinectSDK Dynamic time warping(DTW) Gesture Recognition 
// This program is responsible for Normalization of body Joints coordinates
// </Description>
//-----------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;
using System.Collections;

namespace SignLanguageTranslatorVGB
{
    /// <summary>
    /// This class is used to perform Normalization of the coordinates.
    /// The six hand joints are normalised using this class.
    /// </summary>
    class JointsExtractor
    {
       /// <summary>
       /// This function Normalizes the sequence
       /// </summary>
       /// <param name="data">the Kinect input that is to be normalised</param>
       /// <returns>X and Y cordinates of Six handjoints after Normalization</returns>
        public ArrayList normalizationCoordinates(string[] data)
        {
            ArrayList pointsList = new ArrayList();
            var point = new Point[6]; double posx; double posy;
            Point shoulderRight = new Point(), shoulderLeft = new Point();
          
            for (int i = 0; i < data.Length; i++)
           {
                if (data[i] == "HandLeft")
                {
                    posx = Convert.ToDouble(data[i + 1]); //position for X coordinates
                    posy = Convert.ToDouble(data[i + 2]);// position for Y coordinates
                  
                    point[0] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                  
                }
                else if (data[i] == "WristLeft")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    point[1] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
                else if (data[i] == "ElbowLeft")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    point[2] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
                else if (data[i] == "ElbowRight")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    point[3] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
                else if (data[i] == "WristRight")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    point[4] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
                else if (data[i] == "HandRight")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    point[5] = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                 
                }
                else if (data[i] == "ShoulderLeft")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    shoulderLeft = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
                else if (data[i] == "ShoulderRight")
                {
                    posx = Convert.ToDouble(data[i + 1]);
                    posy = Convert.ToDouble(data[i + 2]);
                    shoulderRight = new Point(posx, posy);
                    System.Diagnostics.Debug.WriteLine(posx);
                }
              
                // This conditional statement checks if the last block of the data has been repeated and stored in multiple indexes of the array.
                // It can be ignored.
                if ( (i + 4 < data.Length) && (data[i] == data[i + 4] ))
                {
                    i = i + 7;
                }               
              // This conditional statement checks whether all the points in the array has been filled
                if ( isPointsFilled(point))
                {
                    // The center is assumed to be approximately equal to SpineShoulder. 
                    // Instead of computing midpoint between shoulders, spineshoulder coordinates can be directly used
                    var center = new Point((shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2);
                    for (int j = 0; j < 6; j++)
                    {
                        point[j].X -= center.X;
                        point[j].Y -= center.Y;
                    }

                   //Calculating the Shoulderdistance
                    double shoulderDist =
                        Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) +
                                  Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2));
                    for (int j = 0; j < 6; j++)
                    {
                        point[j].X /= shoulderDist;
                        point[j].Y /= shoulderDist;
                    }
                    pointsList.Add(point);
                    point = new Point[6];
               }
                    posx = 0; posy = 0;// reset coordinates
            }

           return pointsList;
        }
        /// <summary>
        /// This functions checks if all the points are filled in the array
        /// </summary>
        /// <param name="p">It is the point which is filled</param>
        /// <returns>returns true only if the points have been filled</returns>
        private Boolean isPointsFilled(Point[] p) {
            for (int i = 0; i < p.Length; i++) {
                if (p[i].X == 0 && p[i].Y == 0) {
                    return false;
                }
            }
            return true;
        }
          
    }
}
