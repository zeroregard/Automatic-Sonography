using System;
using System.Collections;
using System.Text;
using UR10TCPController.Interfaces;
using Util;
namespace UR10TCPController
{
    public class Analyzer : IAnalyzer
    {
        /// <summary>Private variable to count the location of the received byte array.</summary>
        int count;
        /// <summary>Private variable containing the number of bytes received on the socket stream.</summary>
        int numberOfBytes;
        /// <summary>Private variable containing the different robot modes.</summary>
        string[] robotModes;
        /// <summary>Private variable containing the different control modes.</summary>
        string[] controlModes;
        /// <summary>Private variable containing the different safety modes.</summary>
        string[] safetyModes;
        /// <summary>Private variable containing the different joint modes.</summary>
        Hashtable htJointModes;
        /// <summary>Private variable containing the different tool modes.</summary>
        Hashtable htToolModes;

        /// <summary>Actual UR pose event. This event is called when a new pose is received from the socket stream.</summary>
        public delegate void ActualURPoseEventHandler(URPose aPose);
        /// <summary>Actual UR pose event. This event is called when a new pose is received from the socket stream.</summary>
        public static event ActualURPoseEventHandler OnActualURPose;
        /// <summary>Robot Message event. This event is called when a new robot message is received from the UR socket stream.</summary>
        public delegate void RobotMessageEventHandler(RobotMessage robMsg);
        /// <summary>Robot Message event. This event is called when a new robot message is received from the UR socket stream.</summary>
        public static event RobotMessageEventHandler OnRobotMessage;
        /// <summary>Robot mode event. This event is called when a new robot mode is received from the socket stream.</summary>
        public delegate void RobotModeEventHandler(string robotMode, string controlMode);
        /// <summary>Robot mode event. This event is called when a new robot mode is received from the socket stream.</summary>
        public static event RobotModeEventHandler OnRobotMode;
        /// <summary>Tool mode event. This event is called when a new tool mode is received from the socket stream.</summary>
        public delegate void ToolModeEventHandler(string toolMode);
        /// <summary>Tool mode event. This event is called when a new tool mode is received from the socket stream.</summary>
        public static event ToolModeEventHandler OnToolMode;
        /// <summary>Safety mode event. This event is called when a new safety mode is received from the socket stream.</summary>
        public delegate void SafetyModeEventHandler(string safetyMode);
        /// <summary>Safety mode event. This event is called when a new safety mode is received from the socket stream.</summary>
        public static event SafetyModeEventHandler OnSafetyMode;

        /// <summary>Constructor to create instance of AnalyseURData and instantiate the different modes.</summary>
        public Analyzer()
        {
            robotModes = new string[9] { "ROBOT_MODE_DISCONNECTED", "ROBOT_MODE_CONFIRM_SAFETY", "ROBOT_MODE_BOOTING", "ROBOT_MODE_POWER_OFF", "ROBOT_MODE_POWER_ON", "ROBOT_MODE_IDLE", "ROBOT_MODE_BACKDRIVE", "ROBOT_MODE_RUNNING", "ROBOT_MODE_UPDATING_FIRMWARE" };
            controlModes = new string[4] { "CONTROL_MODE_POSITION", "CONTROL_MODE_TEACH", "CONTROL_MODE_FORCE", "CONTROL_MODE_TORQUE" };
            safetyModes = new string[9] { "SAFETY_MODE_NORMAL", "SAFETY_MODE_REDUCED", "SAFETY_MODE_PROTECTIVE_STOP", "SAFETY_MODE_RECOVERY", "SAFETY_MODE_SAFEGUARD_STOP", "SAFETY_MODE_SYSTEM_EMERGENCY_STOP", "SAFETY_MODE_ROBOT_EMERGENCY_STOP", "SAFETY_MODE_VIOLATION", "SAFETY_MODE_FAULT" };

            // Joint modes
            // Hashtable with id and responding string based the the documentation from Universal Robots in the link above
            htJointModes = new Hashtable();
            htJointModes.Add("236", "JOINT_SHUTTING_DOWN_MODE");
            htJointModes.Add("237", "JOINT_PART_D_CALIBRATION_MODE");
            htJointModes.Add("238", "JOINT_BACKDRIVE_MODE");
            htJointModes.Add("239", "JOINT_POWER_OFF_MODE");
            htJointModes.Add("245", "JOINT_NOT_RESPONDING_MODE");
            htJointModes.Add("246", "JOINT_MOTOR_INITIALISATION_MODE");
            htJointModes.Add("247", "JOINT_BOOTING_MODE");
            htJointModes.Add("248", "JOINT_PART_D_CALIBRATION_ERROR_MODE");
            htJointModes.Add("249", "JOINT_BOOTLOADER_MODE");
            htJointModes.Add("250", "JOINT_CALIBRATION_MODE");
            htJointModes.Add("252", "JOINT_FAULT_MODE");
            htJointModes.Add("253", "JOINT_RUNNING_MODE");
            htJointModes.Add("255", "JOINT_IDLE_MODE");

            // Tool modes
            // Hashtable with id and responding string based the the documentation from Universal Robots in the link above
            htToolModes = new Hashtable();
            htToolModes.Add("245", "TOOL_NOT_RESPONDING_MODE");
            htToolModes.Add("249", "TOOL_BOOTLOADER_MODE");
            htToolModes.Add("253", "TOOL_RUNNING_MODE");
            htToolModes.Add("255", "TOOL_IDLE_MODE");
        }

        /// <summary>Analyses the data received from the UR socket stream on port 30002.</summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        public void ReadURData(byte[] data)
        {
            count = -1;

            // Determine the size of the received data based on the first 4 bytes
            byte[] numberOfBytesArray = new byte[4];
            Array.Copy(data, count + 1, numberOfBytesArray, 0, 4);
            count = count + 4;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(numberOfBytesArray);
            numberOfBytes = BitConverter.ToInt32(numberOfBytesArray, 0);

            //Debug.WriteLine("Overall package size: " + numberOfBytes);

            //Debug.WriteLine("Robot MessageType: " + data[4]);
            count++;

            // switch case to determine which kind of analysis should be used,
            // based on the value of the robot message type (messageType 16->RobotState or 20->RobotMessage)
            if (numberOfBytes < 2048 || numberOfBytes > 0)
            {
                switch (data[4])
                {
                    case 16:
                        RobotState(data);
                        break;
                    case 20:
                        RobotMessage(data);
                        break;
                }
            }
        }

        /// <summary>Analyses the robots state when robot message type is RobotState.
        /// <para>Value for RobotState is 16.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void RobotState(byte[] data)
        {
            while (count < numberOfBytes - 1)
            {
                // determening the size of the next subpackage in the loop
                byte[] subPackageSizeArray = new byte[4];
                Array.Copy(data, count + 1, subPackageSizeArray, 0, 4);
                count = count + 4;
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(subPackageSizeArray);
                //Debug.WriteLine("Subpackage size: " + BitConverter.ToInt32(subPackageSizeArray, 0));

                // Every loop defines a new package type. Switch case determine the analysis
                // based on the subpackage type value.
                switch (data[count + 1])
                {
                    case 0:
                        RobotModeData(data);
                        break;
                    case 1:
                        JointData(data);
                        break;
                    case 2:
                        ToolData(data);
                        break;
                    case 3:
                        MasterboardData(data);
                        break;
                    case 4:
                        CartesianInfo(data);
                        break;
                    case 5:
                        KinematicsInfo(data);
                        break;
                    case 6:
                        ConfigurationData(data);
                        break;
                    case 7:
                        ForceModeData(data);
                        break;
                    case 8:
                        AdditionalInfo(data);
                        break;
                    case 9:
                        CalibrationData(data);
                        break;
                    default:
                        count = numberOfBytes;
                        break;
                }
            }
        }

        /// <summary>Analyses the robots message when robot message type is RobotMessage.
        /// <para>Value for RobotMessage is 20.</para>
        /// <para>Package length is 61 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void RobotMessage(byte[] data)
        {
            double timeStamp = data[count + 1] * Math.Pow(2, 8 * 7) + data[count + 2] * Math.Pow(2, 8 * 6) + data[count + 3] * Math.Pow(2, 8 * 5) + data[count + 4] * Math.Pow(2, 8 * 4) + data[count + 5] * Math.Pow(2, 8 * 3) + data[count + 6] * Math.Pow(2, 8 * 2) + data[count + 7] * Math.Pow(2, 8 * 1) + data[count + 8];
            count = count + 8;
            //Debug.WriteLine("Timestamp: " + timeStamp);
            int source = data[count + 1];
            //Debug.WriteLine("Source: " + data[count + 1]);
            count++;
            int robotMsgVersion = data[count + 1];
            //Debug.WriteLine("ROBOT_MESSAGE_VERSION: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Project name size: " + data[count + 1]);
            int projectNameSize = data[count + 1];
            count++;
            byte[] projectNameArray = new byte[projectNameSize];
            Array.Copy(data, count + 1, projectNameArray, 0, projectNameSize);
            count = count + projectNameSize;
            string projectName = new string(Encoding.ASCII.GetString(projectNameArray).ToCharArray());
            //Debug.WriteLine("Project name: " + projectName);
            int majorVersion = data[count + 1];
            //Debug.WriteLine("Major version: " + data[count + 1]);
            count++;
            int minorVersion = data[count + 1];
            //Debug.WriteLine("Minor version: " + data[count + 1]);
            count++;
            byte[] svnRevisionArray = new byte[] { data[count + 1], data[count + 2], data[count + 3], data[count + 4] };
            if (BitConverter.IsLittleEndian)
                Array.Reverse(svnRevisionArray);
            int svnRevision = BitConverter.ToInt32(svnRevisionArray, 0);
            count = count + 4;
            //Debug.WriteLine("SVN Revision: " + svnRevision);
            int buildDateSize = numberOfBytes - 1 - count;
            char[] buildDateArray = new char[buildDateSize];
            Array.Copy(data, count + 1, buildDateArray, 0, buildDateSize);
            count = count + buildDateSize;
            string buildDate = new string(buildDateArray);
            //Debug.WriteLine("Build date: " + buildDate);
            RobotMessage robMsg = new RobotMessage(projectName, majorVersion, minorVersion, svnRevision, buildDate);
            //RobotMessage robMsg = new RobotMessage(timeStamp, source, robotMsgVersion, projectName, majorVersion, minorVersion, svnRevision, buildDate);
            //events.NewRobotMessage(robMsg);
            if (OnRobotMessage != null)
                OnRobotMessage(robMsg);
        }

        /// <summary>Analyses the robot mode when robot message type is RobotModeData.
        /// Subpackage containing different informations about the robot mode.
        /// <para>Value for Robot Mode Data is 0.</para>
        /// <para>Package length is 38 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void RobotModeData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - ROBOT MODE DATA", data[count + 1]);
            count++;
            double timeStamp = data[count + 1] * Math.Pow(2, 8 * 7) + data[count + 2] * Math.Pow(2, 8 * 6) + data[count + 3] * Math.Pow(2, 8 * 5) + data[count + 4] * Math.Pow(2, 8 * 4) + data[count + 5] * Math.Pow(2, 8 * 3) + data[count + 6] * Math.Pow(2, 8 * 2) + data[count + 7] * Math.Pow(2, 8 * 1) + data[count + 8];
            count = count + 8;
            //Debug.WriteLine("Timestamp: " + timeStamp);
            //Debug.WriteLine("Content: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7]);
            count = count + 7;

            //Debug.WriteLine("Robot mode: " + robotModes[data[count + 1]]);
            string robotMode = robotModes[data[count + 1]];
            count++;
            //Debug.WriteLine("Control mode: " + controlModes[data[count + 1]]);
            string controlMode = controlModes[data[count + 1]];
            count++;

            //Debug.WriteLine("Target Speed Fraction: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Speed Scaling: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            if (OnRobotMode != null)
                OnRobotMode(robotMode, controlMode);
        }

        /// <summary>Analyses the joint data when robot message type is JointData.
        /// Subpackage containing different informations about the joint data, such as position and mode.
        /// <para>Value for Joint Data is 1.</para>
        /// <para>Package length is 251 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void JointData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - JOINT DATA", data[count + 1]);
            count++;

            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("Actual joint position nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
                //if (i == 4)
                //    Debug.WriteLine("Target joint position nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
                //Debug.WriteLine("Actual joint speed nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
                //Debug.WriteLine("Actual joint current nr. {0}: [{1}] [{2}] [{3}] [{4}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Actual joint voltage nr. {0}: [{1}] [{2}] [{3}] [{4}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Joint motor temperature nr. {0}: [{1}] [{2}] [{3}] [{4}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("T micro nr. {0}: [{1}] [{2}] [{3}] [{4}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Joint mode nr. {0}: {1}", i, htJointModes[data[count + 1].ToString()]);
                count++;
            }
        }

        /// <summary>Analyses the tool data when robot message type is ToolData.
        /// Subpackage containing different informations about the tool, such as tool mode and analog inputs.
        /// <para>Value for Tool Data is 2.</para>
        /// <para>Package length is 37 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void ToolData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - TOOL DATA", data[count + 1]);
            count++;

            //Debug.WriteLine("Analog Input Range 2: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog Input Range 3: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog input 2: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Analog input 3: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Tool Voltage 48V: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Tool Output Voltage: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Tool Current: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Tool Temperature: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Tool Mode: " + htToolModes[data[count + 1]]);
            string toolMode = htToolModes[data[count + 1].ToString()].ToString();
            if (OnToolMode != null)
                OnToolMode(toolMode);
            count++;
        }

        /// <summary>Analyses the masterboard data when robot message type is MasterboardData.
        /// Subpackage containing different informations about the Masterboard, such as safetymode of the robot.
        /// <para>Value for Masterboard Data is 3.</para>
        /// <para>Package length is 72 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void MasterboardData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - MASTERBOARD DATA", data[count + 1]);
            count++;
            //Debug.WriteLine("Digital Input Bits: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Digital Output Bits: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Analog Input Range 0: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog Input Range 1: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog Input 0: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Analog Input 1: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Analog Output Domain 0: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog Output Domain 1: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Analog Output 0: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Analog Output 1: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Masterboard Temperature: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Robot Voltage 48V: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Robot Current: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Master IO Current: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Safety mode: " + safetyModes[data[count + 1] - 1]);
            string safetyMode = safetyModes[data[count + 1] - 1];
            if (OnSafetyMode != null)
                OnSafetyMode(safetyMode);
            count++;
            //Debug.WriteLine("In Reduced Mode: " + data[count + 1]);
            count++;
            //Debug.WriteLine("Euromap67 Installed?: " + data[count + 1]);
            if (data[count + 1] == 1) // Vis Euromap67 er installeret
            {
                //Debug.WriteLine("Euromap Input Bits: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Euromap Output Bits: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Euromap Voltage: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
                //Debug.WriteLine("Euromap Current: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
                count = count + 4;
            }
            else
            {
                count++;
            }
            //Debug.WriteLine("Used by UR software only: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
        }

        /// <summary>Analyses the cartesian info when robot message type is CartesianInfo.
        /// Subpackage containing different informations about the CartesianInfo, such as TCP position and offset.
        /// <para>Value for CartesianInfo is 4.</para>
        /// <para>Package length is 101 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void CartesianInfo(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - CARTESIAN INFO", data[count + 1]);
            count++;

            //Debug.WriteLine("X: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double xPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("X: " + xPoseDouble);
            count = count + 8;

            //Debug.WriteLine("Y: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double yPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Y: " + yPoseDouble);
            count = count + 8;

            //Debug.WriteLine("Z: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double zPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Z: " + zPoseDouble);
            count = count + 8;

            //Debug.WriteLine("Rx: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RxPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Rx: " + RxPoseDouble);
            count = count + 8;

            //Debug.WriteLine("Ry: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RyPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Ry: " + RyPoseDouble);
            count = count + 8;

            //Debug.WriteLine("Rz: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RzPoseDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Rz: " + RzPoseDouble);
            count = count + 8;

            //Debug.WriteLine("TCPOffsetX: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("TCPOffsetY: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("TCPOffsetZ: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("TCPOffsetRx: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("TCPOffsetRy: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("TCPOffsetRz: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;

            URPose pose = new URPose(xPoseDouble, yPoseDouble, zPoseDouble, RxPoseDouble, RyPoseDouble, RzPoseDouble);
            Data.Instance.ReceivePose(pose);
            //events.UpdateActualURPose(pose);
            if (OnActualURPose != null)
                OnActualURPose(pose);
        }

        /// <summary>Analyses the Kinematics Info when robot message type is KinematicsInfo.
        /// Subpackage containing different informations about the robots kinematics, such as a checksum for the specific robot.
        /// <para>Value for Masterboard Data is 5.</para>
        /// <para>Package length is 225 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void KinematicsInfo(byte[] data)
        {
            // Contains a checksum for the specific robot
            // It might be subject of change in the near future.
            //Debug.WriteLine("Package type: {0} - KINEMATICS INFO", data[count + 1]);
            count++;

            for (int i = 0; i < 220; i++)
            {
                count++;
            }
        }

        /// <summary>Analyses the Configuration Data when robot message type is ConfigurationData.
        /// Subpackage containing different informations about the Configuration, such as joint limits.
        /// <para>Value for Configuration Data is 6.</para>
        /// <para>Package length is 445 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void ConfigurationData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - CONFIGURATION DATA", data[count + 1]);
            count++;

            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("Joint min limit nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
                //Debug.WriteLine("Joint max limit nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }
            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("Joint max speed nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
                //Debug.WriteLine("Joint max acceleration nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }

            //Debug.WriteLine("Joint default speed limit: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Joint default acceleration limit: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Tool default speed limit: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Tool default acceleration limit: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;
            //Debug.WriteLine("Characteristic size of the tool (EqRadius): [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            count = count + 8;

            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("DHa nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }
            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("DHd nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }
            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("DHalpha nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }
            for (int i = 0; i < 6; i++)
            {
                //Debug.WriteLine("DHtheta nr. {0}: [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}] [{8}]", i, data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
                count = count + 8;
            }
            //Debug.WriteLine("Masterboard version: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Controller Box Type: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Robot type: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
            //Debug.WriteLine("Robot sub type: [{0}] [{1}] [{2}] [{3}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4]);
            count = count + 4;
        }

        /// <summary>Analyses the Force Mode Data when robot message type is ForceModeData.
        /// Subpackage containing different informations about the force.
        /// <para>Value for Force Mode Data is 7.</para>
        /// <para>Package length is 61 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void ForceModeData(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - FORCE MODE DATA", data[count + 1]);
            count++;

            //Debug.WriteLine("X: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double xForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("X: " + xForceDouble);
            count = count + 8;

            //Debug.WriteLine("Y: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double yForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Y: " + yForceDouble);
            count = count + 8;

            //Debug.WriteLine("Z: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double zForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Z: " + zForceDouble);
            count = count + 8;

            //Debug.WriteLine("Rx: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RxForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Rx: " + RxForceDouble);
            count = count + 8;

            //Debug.WriteLine("Ry: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RyForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Ry: " + RyForceDouble);
            count = count + 8;

            //Debug.WriteLine("Rz: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double RzForceDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Rz: " + RzForceDouble);
            count = count + 8;

            //Debug.WriteLine("Robot Dexterity: [{0}] [{1}] [{2}] [{3}] [{4}] [{5}] [{6}] [{7}]", data[count + 1], data[count + 2], data[count + 3], data[count + 4], data[count + 5], data[count + 6], data[count + 7], data[count + 8]);
            double robotDexterityDouble = GetDoubleFromByteArray(data, count);
            //Debug.WriteLine("Robot Dexterity: " + robotDexterityDouble);
            count = count + 8;
        }

        /// <summary>Analyses the Additional Info when robot message type is AdditionalInfo.
        /// Subpackage containing informations about the teach button on the robots controller.
        /// <para>Value for Additional Info is 8.</para>
        /// <para>Package length is 7 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void AdditionalInfo(byte[] data)
        {
            //Debug.WriteLine("Package type: {0} - ADDITIONAL INFO", data[count + 1]);
            count++;

            //Debug.WriteLine("Teach Button Pressed?: {0}", data[count + 1]);
            count++;
            //Debug.WriteLine("Teach Button Enabled?: {0}", data[count + 1]);
            count++;
        }

        /// <summary>Analyses the Calibration Data when robot message type is CalibrationData.
        /// Subpackage containing different informations about the Calibration. This package is used internally by Universal Robots software only.
        /// <para>Value for Calibration Data is 9.</para>
        /// <para>Package length is 53 bytes.</para>
        /// </summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        private void CalibrationData(byte[] data)
        {
            // This package is used internally by Universal Robots software only.
            // It might be subject of change in the near future.

            //Debug.WriteLine("Package type: {0} - CALIBRATION DATA", data[count + 1]);
            count++;

            for (int i = 0; i < 48; i++)
            {
                count++;
            }
        }

        /// <summary>Method to copy an 8-byte array from the main data array, and convert it to a double value.</summary>
        /// <param name="data">Byte array received on the socket stream.</param>
        /// <param name="count">Current count of the analysis.</param>
        private double GetDoubleFromByteArray(byte[] data, int count)
        {
            byte[] byteArray = new byte[8];
            Array.Copy(data, count + 1, byteArray, 0, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byteArray);
            double number = BitConverter.ToDouble(byteArray, 0);
            return number;
        }
    }
}
