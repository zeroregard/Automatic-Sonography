using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA;
using Util;
using System.Threading;

namespace LOGIC
{
    /// <summary><para>TestConnection is a class to handle the test of a new connection.</para>
    /// This class tries to establish connection to the test unit. When the connection is completed it tests whether the connected unit
    /// is equal to the desired unit. Thereby the test secures that it is only approved if everything matches.
    /// </summary>
    public class TestConnection
    {
        // Private declerations
        Logic logic;
        Connection connection;
        Unit testUnit;
        RobotMessage robMsg, testRobMsg;

        /// <summary>Test Completed event. This event is thrown when the test has completed.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void TestCompletedEventHandler(bool result);
        /// <summary>Test Completed event. This event is thrown when the test has completed.</summary>
        public static event TestCompletedEventHandler OnTestCompleted;

        /// <summary>Create instance of TestConnection.</summary>
        public TestConnection()
        {
            logic = Logic.GetInstance();
        }

        /// <summary>Runs a test based on the test unit. Test result is given in the OnTestCompleted event.</summary>
        /// <param name="testUnit">Test unit that needs to be tested.</param>
        public void Test(Unit testUnit)
        {
            this.testUnit = testUnit;
            AnalyseURData.OnRobotMessage -= new AnalyseURData.RobotMessageEventHandler(robotMsgReceived);
            AnalyseURData.OnRobotMessage += new AnalyseURData.RobotMessageEventHandler(robotMsgReceived);
            Connection.OnConnectionCompleted -= new Connection.ConnectionCompletedEventHandler(ConnectionCompleted);
            Connection.OnConnectionCompleted += new Connection.ConnectionCompletedEventHandler(ConnectionCompleted);

            testRobMsg = new RobotMessage(testUnit.projectName, testUnit.majorversion, testUnit.minorversion, testUnit.svnRevision, testUnit.buildDate);
            logic.SelectUnit(testUnit);                                                                     // Set the unit in logic that needs testing
            logic.EstablishConnection();                                                                    // Establish a temporary connection to the new unit
        }

        /// <summary>Checks the connection result, throws an event based on the test result and closes the connection.</summary>
        /// <param name="result">Result of the test</param>
        private void ConnectionCompleted(bool result)
        {
            connection = logic.connection;
            bool testResult = false;
            if (result && TestRemoteDesktopConnection() && testRobMsg.Equals(robMsg))                       // Checks that all the test conditions is acheived
            {
                testResult = true;
            }
            else testResult = false;

            if (OnTestCompleted != null)
                OnTestCompleted(testResult);

            connection.CloseConnection();                                                                   // Close the temporary established connection
        }

        /// <summary>Checks whether the invitation from the remote desktop server contains the right data.</summary>
        private bool TestRemoteDesktopConnection()
        {
            string invitation = connection.GetInvitation();
            if (invitation.Contains(testUnit.rdIpAddress) && invitation.Contains(testUnit.location))        // Checks if the invitation contains the right ip address and location from the test unit
                return true;
            else return false;
        }

        /// <summary>Sets the local variable of robot message.</summary>
        private void robotMsgReceived(RobotMessage robMsg)
        {
            this.robMsg = robMsg;
        }
    }
}
