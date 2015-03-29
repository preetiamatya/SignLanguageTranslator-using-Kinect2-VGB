//---------------------------------------------------------------------------------------------------
// <Description>
//This program is built by refering Dynamic time warping to recognize gestures from Microsoft Developer Network 
// </Description>
//-----------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;
/// <summary>
/// This class is used to compute cost for DTW matrix
/// </summary>
class DTW
{
    // Size of obeservations vectors.
    int dim;

    // Known sequences
    ArrayList dataset_sequences;

    // Labels of those known sequences
    ArrayList labels;

    // This value is set to find the maximum distance between the Kinect input with our dataset 
    double DTWThreshold;

    // FirstThreshold is the maximun distance between the between the Kinect input with our dataset 
    //It is a hit and trial method for computing sequence which is a larger DTW distance is going to be a matched sequence 
    double firstThreshold;

    // Maxslope define how many steps to be considered horizontally or vertically in row/column in our cost matrix
    int maxSlope;

   //Minimum length is constraint in sequence before it is matched. This can be ignored
    private readonly double _minimumLength;

    /// <summary>
    /// Constructor for computing DTW matrix
    /// </summary>
    /// <param name="dim">The dimension of the array. It must be 12(X and Y cordinates) to compute six hand joints</param>
    /// <param name="threshold">The threshold parameter for sequence matching in DTW. The less its value, the more similar the sequence.</param>
    /// <param name="firstThreshold">It is boundary condition for the maximum distance between sequences. For more accuracy, less value is required</param>
    /// <param name="maxSlope">It is used to avoid mapping very steep slopes with sequences. For more accuracy, less value is required </param>
    /// <param name="minimumLength">Minimum length is constraint in sequence before it is matched. </param>
    public DTW(int dim, double threshold, double firstThreshold, int maxSlope, double minimumLength)
    {
        this.dim = dim;
        dataset_sequences = new ArrayList();
        labels = new ArrayList();
        this.DTWThreshold = threshold;
        this.firstThreshold = firstThreshold;
        this.maxSlope = maxSlope;
        _minimumLength = minimumLength;
    }


     /// <summary>
    /// This function is used to add the name of the sequences in the label to display in the Presentation Layer
    /// </summary>
    /// <param name="sequence">The sequence from the dataset</param>
    /// <param name="labelText">It is the text to be displayed in the Presentation Layer</param>
    
    public void Add(ArrayList sequence, string labelText)
    {
        dataset_sequences.Add(sequence);
        labels.Add(labelText);
    }

 
    /// <summary>
    ///This function is used to recognise the gesture
    ///It recognizes gestures only if it is below DTW threshold level
    ///DTW computation takes place only if firstThreshold computation condition is matched which is done to avoid sequences with high cost      
    /// </summary>
    /// <param name="sequence">Normalised kinect input</param>
    /// <returns>returns gesture name if DTW threshold is matched, otherwise it returns UNKNOWN</returns>
    public string recognize(ArrayList sequence)
    {
        double minimumDistance = double.PositiveInfinity;
        string _class = "UNKNOWN";
        for (int i = 0; i < dataset_sequences.Count; i++)
        {
            ArrayList dataset_sequence = (ArrayList)dataset_sequences[i];
            try
            {
                if (euclideanDistance((double[])sequence[sequence.Count - 1], (double[])dataset_sequence[dataset_sequence.Count - 1]) < firstThreshold)
                {
                    //This comparision is done to avoid the sequences with high cost
                    System.Diagnostics.Debug.WriteLine("starttimeDTW=" + DateTime.Now.ToString("ss.fff", CultureInfo.InvariantCulture));
                    double distance = dtw(sequence, dataset_sequence) / (dataset_sequence.Count);
                    System.Diagnostics.Debug.WriteLine("endtimeDTW=" + DateTime.Now.ToString("ss.fff", CultureInfo.InvariantCulture));
                    //This is done to get the least distance from the DTW computation. 
                    //This comparision can be ignored because DTW itself returns the least distance from the top row of cost matrix
                    if (distance < minimumDistance)
                    {                      
                        minimumDistance = distance;
                        _class = (string)(labels[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                // ignore
            }
        }
        return (minimumDistance < DTWThreshold ? _class : "UNKNOWN");// +" " + minimumDistance.ToString();
    }

    //This function computes the Euclidean distance of Kinect input and dataset
    private double euclideanDistance(double[] point1, double[] point2)
    {
        double distSqr = 0;
        for (int i = 0; i < dim; i++)
        {
            distSqr += Math.Pow(point1[i] - point2[i], 2);
        }
        return Math.Sqrt(distSqr);
    }

    /// <summary>
    /// This function is a dynamic programming approach to compute DTW cost matrix.
    /// The 'cell' represents all cells of the DTW cost matrix
    /// SlopeI and SlopeJ are arraylist of slopes for two sequences
    /// To improve the mapping of sequences the slope constraints are added
    /// </summary>
    /// <param name="seq1">Normalised sequence from Kinect input</param>
    /// <param name="seq2">Normalised sequence from dataset</param>
    /// <returns></returns>
    public double dtw(ArrayList seq1, ArrayList seq2)
    {       
        ArrayList seq1Rev = new ArrayList(seq1); seq1Rev.Reverse();
        ArrayList seq2Rev = new ArrayList(seq2); seq2Rev.Reverse();
        double[,] cell = new double[seq1Rev.Count + 1, seq2Rev.Count + 1];
        int[,] slopeI = new int[seq1Rev.Count + 1, seq2Rev.Count + 1];
        int[,] slopeJ = new int[seq1Rev.Count + 1, seq2Rev.Count + 1];

        for (int i = 0; i < seq1Rev.Count + 1; i++)
        {
            for (int j = 0; j < seq2Rev.Count + 1; j++)
            {
                cell[i, j] = double.PositiveInfinity;
                slopeI[i, j] = 0;
                slopeJ[i, j] = 0;
            }
        }
        cell[0, 0] = 0;

       //Computation of DTW cost matrix
       //An Euclidean distance between two points are added to the least distance of its neighbours which is filled in the cell of the matrix
        for (int i = 1; i < seq1Rev.Count + 1; i++)
        {
            for (int j = 1; j < seq2Rev.Count + 1; j++)
            {
                if (cell[i, j - 1] < cell[i - 1, j - 1] && cell[i, j - 1] < cell[i - 1, j] && slopeI[i, j - 1] < maxSlope)
                {
                    cell[i, j] = euclideanDistance((double[])seq1Rev[i - 1], (double[])seq2Rev[j - 1]) + cell[i, j - 1];
                    slopeI[i, j] = slopeJ[i, j - 1] + 1; ;
                    slopeJ[i, j] = 0;
                }
                else if (cell[i - 1, j] < cell[i - 1, j - 1] && cell[i - 1, j] < cell[i, j - 1] && slopeJ[i - 1, j] < maxSlope)
                {
                    cell[i, j] = euclideanDistance((double[])seq1Rev[i - 1], (double[])seq2Rev[j - 1]) + cell[i - 1, j];
                    slopeI[i, j] = 0;
                    slopeJ[i, j] = slopeJ[i - 1, j] + 1;
                }
                else
                {
                    cell[i, j] = euclideanDistance((double[])seq1Rev[i - 1], (double[])seq2Rev[j - 1]) + cell[i - 1, j - 1];
                    slopeI[i, j] = 0;
                    slopeJ[i, j] = 0;
                }
            }
        }

       // The cost matrix results the highest cost in its top row because of its filling order.
       // The overall cost of the computation can be found at the top right corner of the matrix.
       // However, the least cost of the computation can be found out from the top row using this sorting method.
        double bestMatch = double.PositiveInfinity;
        for (int i = 1; i < seq1Rev.Count + 1; i++)
        {
            if (cell[i, seq2Rev.Count] < bestMatch)
                bestMatch = cell[i, seq2Rev.Count];
        }
        return bestMatch;
    }
}
