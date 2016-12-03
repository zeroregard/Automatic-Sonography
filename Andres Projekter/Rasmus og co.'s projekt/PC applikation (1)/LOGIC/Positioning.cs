using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA;
using Util;

namespace LOGIC
{
    /// <summary>
    /// <para>Positioning is the class that handles the positioning of the robot.</para>
    /// This class listens for changes in position from the GeoMagicTouch and based on the actual position of the robot a new position is calculated and sent to the robot.
    /// This class also handles the forcefeedback from the robot and calculates the force to be set on the GeoMagicTouch.
    /// </summary>
    public class Positioning
    {
        //Logic logic;
        Connection connection;

        /// <summary>Instance of the current position of the GeoMagicTouch.</summary>
        GeoMagicPose actualGeoPose;
        /// <summary>New center position of the GeoMagicTouch used to calculate the movement.</summary>
        GeoMagicPose newCenterGeoPose = new GeoMagicPose();
        /// <summary>Instance of the current position of the universal robot.</summary>
        URPose actualURPose;
        /// <summary>New center position of the universal robot used to calculate the movement.</summary>
        URPose newCenterURPose = new URPose();

        /// <summary>Defines whether the robot should move or not.</summary>
        private static bool isMoving = false;

        /// <summary>Positioning exception event.This event is thrown when an exception occurres with the positioning.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void PositioningEventHandler(string error, string source);
        /// <summary>Positioning exception event.This event is thrown when an exception occurres with the positioning.</summary>
        public static event PositioningEventHandler OnException;

        public delegate void ForceEventHandler(ForceData force);
        public static event ForceEventHandler OnForce;


        /// <summary>Constructor for Positioning.</summary>
        public Positioning()
        {
            //logic = Logic.GetInstance();
            //connection = logic.connection;
            connection = Logic.GetInstance().connection;                                                        // Gets the instance of the established connection from logic

            GeoMagicTouch.OnGeoPose -= new GeoMagicTouch.GeoMagicPoseEventHandler(SetGeoPose);
            GeoMagicTouch.OnGeoPose += new GeoMagicTouch.GeoMagicPoseEventHandler(SetGeoPose);                  // Initializes a listener to listen for a new position from the GeoMagicTouch
            AnalyseURData.OnActualURPose -= new AnalyseURData.ActualURPoseEventHandler(SetURPose);
            AnalyseURData.OnActualURPose += new AnalyseURData.ActualURPoseEventHandler(SetURPose);              // Initializes a listener to listen for a new position from the universal robot
        }

        /// <summary>Sets the instance of the acutal position of the universal robot.</summary>
        /// <param name="pose">New position from the universal robot.</param>
        private void SetURPose(URPose pose)
        {
            actualURPose = pose;
        }

        /// <summary>Sets the instance of the actual position of the GeoMagicTouch and initializes the movement of the robot.</summary>
        /// <param name="geoPose">New position from the GeoMagicTouch.</param>
        private void SetGeoPose(GeoMagicPose geoPose)
        {
            actualGeoPose = geoPose;
            MoveRobot();
        }

        /// <summary>Checks if the robot should move or not, and whether the robot is in the right mode.
        /// Based on those results the moving process is continued.</summary>
        private void MoveRobot()
        {
            if (actualGeoPose.BtnState == 3) BtnPressed();                                                                  // Both buttons on the GeoMagicTouch needs to be pressed equal to BtnState value 3
            else BtnReleased();

            if (isMoving)
            {
                if (connection.robotMode == "ROBOT_MODE_RUNNING" && connection.safetyMode == "SAFETY_MODE_NORMAL")          // Checks if the robot is in the right mode to be moving
                {
                    HandleForce();                                                                                          // Initializes the force to be set on the GeoMagicTouch
                    connection.modBus.SendPose(CalculatePose());                                                            // Initializes a calculation of the new position and sends it to the modbus

                    //connection.modBus.SendJoints(CalculateJoints());
                }
                else
                {
                    string message = null;
                    if (connection.robotMode != "ROBOT_MODE_RUNNING")                                                       // Checks which mode caused the error
                    {
                        message = connection.robotMode;
                    }
                    else if (connection.safetyMode != "SAFETY_MODE_NORMAL")
                    {
                        message = connection.safetyMode;
                    }
                    if (OnException != null)
                        OnException("Robotten er i: " + message + " , og kan ikke positioneres.", "URNOTRUNNING");          // Exception thrown to indicate which mode caused the error
                    isMoving = false;
                    HandleForce();                                                                                          // Called to set the force based on the exception
                }
            }
            else
            {
                HandleForce();                                                                                              // Called to set force when the robot doesnt move
            }
        }

        /// <summary>Gets the forcefeedback from the robots modbus server and sets the force on the GeoMagicTouch, based on if the robot should move or not.</summary>
        private void HandleForce()
        {
            if (isMoving)
            {
                ForceData f = connection.modBus.ReadURForce();                                                              // Gets the forcefeedback from the modbus server

                connection.gmt.SetForce(MovingAverage(f));                                                                  // Sets the new force on the GeoMagicTouch based on the averaging
            }
            else
            {
                connection.gmt.SetForce(new ForceData(0, 0, 0, 0, 0, 0));                                                   // When the robot is not moving sets the force to zero
            }
        }

        /// <summary>List containing the latest collected ForceData. Size of the list is based on the samples value.</summary>
        List<ForceData> avgForceList = new List<ForceData>();
        /// <summary>Number of samples to base the averaging on.</summary>
        int samples = 100;
        /// <summary>Scale value to scale the force from the robot before it is sent to the GeoMagicTouch.</summary>
        int scale = 10;
        /// <summary>Calculates a moving average based on the latest collected force values.</summary>
        /// <param name="force">New force data to calculate a new average.</param>
        private ForceData MovingAverage(ForceData force)
        {
            if (avgForceList.Count == samples)
            {
                avgForceList.RemoveAt(0);                                                                                   // When the list has reached the sample size, the oldest collected value is removed
            }                                                                                                               // from the list to maintain the list size equal to number of samples
            avgForceList.Add(force);                                                                                        // Adds the newest force value

            ForceData avgForce = new ForceData();

            foreach (ForceData f in avgForceList)                                                                           // Calculates the total force values for each directions
            {
                avgForce.Xforce += f.Xforce;
                avgForce.Yforce += f.Yforce;
                avgForce.Zforce += f.Zforce;
            }

            avgForce.Xforce = (avgForce.Xforce / samples) / scale;                                                          // Calculates the average force in each directions and scales the values
            avgForce.Yforce = (avgForce.Yforce / samples) / scale;
            avgForce.Zforce = (avgForce.Zforce / samples) / scale;

            if (OnForce != null)
                OnForce(avgForce);

            return avgForce;
        }

        /// <summary>Calculates the new position to be sent to the robot.</summary>
        private URPose CalculatePose()
        {
            double x = newCenterURPose.Xpose + ((actualGeoPose.Xpose + newCenterGeoPose.Xpose * -1) * -1 / 1000);           // Calculates each direction of the position based on the registered center position of the robot and GeoMagicTouch when movement is initiated.
            double y = newCenterURPose.Ypose + ((actualGeoPose.Zpose + newCenterGeoPose.Zpose * -1) / 1000);                // Changes in direction of the GeoMagicTouch is calculated by taking the actual position and adding the invers of the center position.
            double z = newCenterURPose.Zpose + ((actualGeoPose.Ypose + newCenterGeoPose.Ypose * -1) / 1000);                // The x-direction is inversed to inverse the movement from the GeoMagicTouch to the robot based on observation angle of the robot.

            //double Rx = actualGeoPose.RXpose;                                                                               // Direct transfor of the rotation angles from the GeoMagicTouch
            //double Ry = actualGeoPose.RYpose;                                                                               // These values are not used in this version
            //double Rz = actualGeoPose.RZpose;

            double RYpose = actualGeoPose.RYpose + (actualGeoPose.Joint3pose - 3.14 / 2);
            double RZpose = actualGeoPose.RZpose - actualGeoPose.Joint1pose - 0.32272222222222222222*-1;

            double a11 = Math.Cos(actualGeoPose.RXpose * -1) * Math.Cos(RYpose);
            double a22 = Math.Cos(RZpose) * Math.Cos(RYpose) - Math.Sin(RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(RYpose);
            double a33 = Math.Cos(RZpose) * Math.Cos(actualGeoPose.RXpose * -1);

            double angle = Math.Acos(0.5 * (a11 + a22 + a33 - 1));

            double a32 = Math.Sin(RZpose) * Math.Cos(RYpose) + Math.Cos(RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(RYpose);
            double a23 = -Math.Sin(RZpose) * Math.Cos(actualGeoPose.RXpose * -1);
            double e1 = ((a32 - a23) / (2 * Math.Sin(angle))) * angle;

            double a13 = Math.Sin(actualGeoPose.RXpose * -1);
            double a31 = Math.Sin(RZpose) * Math.Sin(RYpose) - Math.Cos(RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(RYpose);
            double e2 = ((a13 - a31) / (2 * Math.Sin(angle))) * angle;

            double a21 = Math.Cos(RZpose) * Math.Sin(RYpose) + Math.Sin(RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(RYpose);
            double a12 = -(Math.Cos(actualGeoPose.RXpose * -1) * Math.Sin(RYpose));
            double e3 = ((a21 - a12) / (2 * Math.Sin(angle))) * angle;

            URPose p = new URPose(x, y, z, e1, e2 + 3.14, e3);                                                              // Creates a position object based on the different calculated directions. 
            return p;                                                                                                       // Last three values are fixed to maintain the same position of the robots head.
        }

        ///// <summary>Calculates the new position to be sent to the robot.</summary>
        //private URPose CalculatePose()
        //{
        //    double x = newCenterURPose.Xpose + ((actualGeoPose.Xpose + newCenterGeoPose.Xpose * -1) * -1 / 1000);           // Calculates each direction of the position based on the registered center position of the robot and GeoMagicTouch when movement is initiated.
        //    double y = newCenterURPose.Ypose + ((actualGeoPose.Zpose + newCenterGeoPose.Zpose * -1) / 1000);                // Changes in direction of the GeoMagicTouch is calculated by taking the actual position and adding the invers of the center position.
        //    double z = newCenterURPose.Zpose + ((actualGeoPose.Ypose + newCenterGeoPose.Ypose * -1) / 1000);                // The x-direction is inversed to inverse the movement from the GeoMagicTouch to the robot based on observation angle of the robot.

        //    //double Rx = actualGeoPose.RXpose;                                                                               // Direct transfor of the rotation angles from the GeoMagicTouch
        //    //double Ry = actualGeoPose.RYpose;                                                                               // These values are not used in this version
        //    //double Rz = actualGeoPose.RZpose;

        //    double a11 = Math.Cos(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
        //    double a22 = Math.Cos(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RYpose) - Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose);
        //    double a33 = Math.Cos(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RXpose * -1);

        //    double angle = Math.Acos(0.5 * (a11 + a22 + a33 - 1));

        //    double a32 = Math.Sin(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RYpose) + Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose);
        //    double a23 = -Math.Sin(actualGeoPose.RZpose) * Math.Cos(actualGeoPose.RXpose * -1);
        //    double e1 = ((a32 - a23) / (2 * Math.Sin(angle))) * angle;

        //    double a13 = Math.Sin(actualGeoPose.RXpose * -1);
        //    double a31 = Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RYpose) - Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
        //    double e2 = ((a13 - a31) / (2 * Math.Sin(angle))) * angle;

        //    double a21 = Math.Cos(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RYpose) + Math.Sin(actualGeoPose.RZpose) * Math.Sin(actualGeoPose.RXpose * -1) * Math.Cos(actualGeoPose.RYpose);
        //    double a12 = -(Math.Cos(actualGeoPose.RXpose * -1) * Math.Sin(actualGeoPose.RYpose));
        //    double e3 = ((a21 - a12) / (2 * Math.Sin(angle))) * angle;

        //    URPose p = new URPose(x, y, z, e1, e2 + 3.14, e3);                                                                     // Creates a position object based on the different calculated directions. 
        //    return p;                                                                                                       // Last three values are fixed to maintain the same position of the robots head.
        //}

        //private URPose CalculateJoints()
        //{
        //    URPose joints = new URPose();
        //    joints.RXpose = actualGeoPose.RYpose - 2.3;                                                                               // Direct transfor of the rotation angles from the GeoMagicTouch
        //    joints.RYpose = actualGeoPose.RXpose - 1.6;                                                                               // These values are not used in this version
        //    joints.RZpose = actualGeoPose.RZpose;

        //    return joints;
        //}

        /// <summary>Indicating the movement process can begin.</summary>
        private void BtnPressed()
        {
            if (!isMoving)
            {
                newCenterURPose = actualURPose;                                                                             // When the buttons on the GeoMagicTouch is pressed, the actual positions of the
                newCenterGeoPose = actualGeoPose;                                                                           // robot and GeoMagicTouch is registered as new center positions
                isMoving = true;
            }
        }

        /// <summary>Indicating the movement process to stop.</summary>
        private void BtnReleased()
        {
            isMoving = false;
        }
    }
}
