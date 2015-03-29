# SignLanguageTranslator using Kinect2
Sign language recogniser using VGB and DTW

This application translates Sign Language gestures into text.

Sign Language gestures are complex in nature, and this program is used to find the accuracy of Sign Language gestures using VGB API provided by Kinect SDK and DTW algorithm. This application is built using Discrete Gesture Basics of Kinect SDK 2.0 and referring Kinect SDK Dynamic time warping (DTW) Gesture Recognition.

Dynamic Sign Language gestures can be recognized using DTW algorithm. However, DTW algorithm has high Space and Time complexity.
The concept behind this application is to recognize the start position and end position of gestures using AdaBoost from VGB API.
When start position and end position are of the same Sign Language gesture, the DTW compares with only one dataset reducing computation cost.

The dataset which is used in this application is provided by the Sign Language teacher of Bavaria. 11 DGS gestures are used for the application.









