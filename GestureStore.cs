using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignLanguageTranslatorVGB
{
    /// <summary>
    /// This class is responsible to store the gestures to display in Presentation Layer.
    /// The gestures are stored in the static string to keep the record of the last detected gestures.
    /// </summary>
    class GestureStore
    {
        public  static string RECOGNIZED_GESTURE;
        /// <summary>
        /// Constructor to get the gesturename to display in the View
        /// </summary>
        public GestureStore(string gesturename)
        {
            RECOGNIZED_GESTURE = RECOGNIZED_GESTURE+"\n"+ gesturename;           
        }
    }
}
