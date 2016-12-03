using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA;
using Util;
using System.Threading;
using System.Diagnostics;
using Declarations.Media;
using Declarations.Players;
using Implementation;

namespace LOGIC
{
    /// <summary>
    /// <para>Connection class to handle the connections between control unit and robot unit</para>
    /// This connection consists of the different objects a connection from the control unit is established to.
    /// In this class it is possible to establish connection to all objects at once or reconnect to individual objects.
    /// Events is thrown depending on what kind of connection is being established.
    /// This class also hold different informations collected from the connections.
    /// </summary>
    public class Connection
    {
        Logic logic;
        Positioning positioning;

        /// <summary>Create instance of the class Connection.</summary>
        public Connection()
        {
            logic = Logic.GetInstance();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Properties
        /// <summary>Contains the connection to the Universal Robot socket server.</summary>
        public URSocket socket { get; private set; }
        /// <summary>Contains the current mode of the robot.</summary>
        public string robotMode { get; private set; }
        /// <summary>Contains the current safetymode of the robot.</summary>
        public string safetyMode { get; private set; }
        /// <summary>Contains the connection to the modbus server on the Universal Robot.</summary>
        public ModBus modBus { get; private set; }
        /// <summary>Contains the connection to the GeoMagicTouch.</summary>
        public GeoMagicTouch gmt { get; private set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Private declarations
        RemoteDesktop rd;
        VideoFeed videoFeed;
        Thread socketThread, gmtThread, videoThread, configThread;
        /// <summary>Array of AutoResetEvents to throw an event when a connection is established.</summary>
        AutoResetEvent[] waitHandles = new AutoResetEvent[4];
        /// <summary>Response time for the waithandles. If the connection didnt complete within this time the waithandle will stop waiting.</summary>
        /// <value>The default value is 25000ms.</value>
        int timeout = 25000;
        bool isConnected;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Connection handle
        /// <summary>Connection Completed event. This event is thrown when the connection attempt has completed.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void ConnectionCompletedEventHandler(bool result);
        /// <summary>Connection Completed event. This event is thrown when the connection attempt has completed.</summary>
        public static event ConnectionCompletedEventHandler OnConnectionCompleted;

        /// <summary>Initialize the sequence of creating the different connections and wait for all processes to respond.
        /// When the connection is completed, successful or not, a ConnectionCompleted event is thrown.</summary>
        public void EstablishConnection()
        {
            isConnected = false;

            EstablishConnectionUR();

            EstablishConnectionRD();

            EstablishConnectionModbus();

            EstablishConnectionGMT();

            EstablishConnectionVF();

            Thread waitThread = new Thread(() =>
            {
                if (WaitHandle.WaitAll(waitHandles, timeout, false)) // Waits for all AutoResetEvents to be set in the different connection threads
                {
                    if (socket.IsConnected() && modBus.IsConnected() && gmt.IsConnected() && rd.IsConnected() && videoFeed.IsConnected()) //Doublechecks that all connections is still alive
                    {
                        isConnected = true;

                        // If all the connections where successful, initialize the positioning and listen for events about the robot mode and safetymode of the robot
                        positioning = new Positioning();
                        AnalyseURData.OnRobotMode -= new AnalyseURData.RobotModeEventHandler(SetRobotMode);
                        AnalyseURData.OnRobotMode += new AnalyseURData.RobotModeEventHandler(SetRobotMode);
                        AnalyseURData.OnSafetyMode -= new AnalyseURData.SafetyModeEventHandler(SetSafetyMode);
                        AnalyseURData.OnSafetyMode += new AnalyseURData.SafetyModeEventHandler(SetSafetyMode);
                    }
                    else isConnected = false;

                    if (OnConnectionCompleted != null)
                        OnConnectionCompleted(isConnected);
                }
                else
                {
                    CloseConnection();                                                          // Make sure the successful established connections is closed again

                    isConnected = false;

                    if (OnConnectionCompleted != null)
                        OnConnectionCompleted(isConnected);
                }
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        /// <summary>Closes all the different connections.</summary>
        public void CloseConnection()
        {
            socket.RequestSocketStop();
            socketThread.Join();

            modBus.Disconnect();

            gmt.RequestStop();
            gmtThread.Join();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Modes
        /// <summary>Sets the property safetymode.</summary>
        private void SetSafetyMode(string safetyMode)
        {
            this.safetyMode = safetyMode;
        }

        /// <summary>Sets the property robotmode.</summary>
        private void SetRobotMode(string robotMode, string controlMode)
        {
            this.robotMode = robotMode;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // URSocket connection
        /// <summary>UR connection event. This event is thrown when the connection to the URSocket is established or the timeout has run out.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void URConnectionEventHandler(bool result);
        /// <summary>UR connection event. This event is thrown when the connection to the URSocket is established or the timeout has run out.</summary>
        public static event URConnectionEventHandler OnConnectionUR;
        /// <summary>Initializes a new socket and starts the SocketListener. Creates a waiting thread to wait for the connection to be established.</summary>
        public void EstablishConnectionUR()
        {
            waitHandles[0] = new AutoResetEvent(false);
            socket = new URSocket();
            socketThread = new Thread(() => socket.RunSocketListener(logic.selectedUnit.urIpAddress, waitHandles[0])); // Creates a new thread to connect to the socket based on the selected Unit collected from logic
            socketThread.IsBackground = true;
            socketThread.Name = "socketThread";
            socketThread.Start();

            Thread waitThread = new Thread(() =>
            {
                if (OnConnectionUR != null)
                    OnConnectionUR(waitHandles[0].WaitOne(timeout, false));                     // Event is thrown when the AutoResetEvent is set or when timeout has run out.
            });
            waitThread.IsBackground = true;
            waitThread.Name = "waitThreadUR";
            waitThread.Start();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Remote Desktop connection
        /// <summary>Remote desktop connection event. This event is thrown when the connection to the remote desktop is established or the timeout has run out.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void RDConnectionEventHandler(bool result);
        /// <summary>Remote desktop connection event. This event is thrown when the connection to the remote desktop is established or the timeout has run out.</summary>
        public static event RDConnectionEventHandler OnConnectionRD;
        /// <summary>Private variable to contain the current invitation from the remote desktop server.</summary>
        string invitation = null;

        /// <summary>Initializes a new remote desktop client and asks the server for a new invitation. Creates a waiting thread to wait for the server to send an invitation.</summary>
        public void EstablishConnectionRD()
        {
            waitHandles[1] = new AutoResetEvent(false);
            Thread remoteThread = new Thread(() =>
            {
                rd = new RemoteDesktop();
                invitation = rd.GetInvitation(logic.selectedUnit.rdIpAddress);                  // Creates a new thread to connect to the remote destop server based on the selected Unit collected from logic
                if (invitation != null)
                {
                    waitHandles[1].Set();
                }
            });
            remoteThread.IsBackground = true;
            remoteThread.Start();

            Thread waitThread = new Thread(() =>
            {
                if (OnConnectionRD != null)
                    OnConnectionRD(waitHandles[1].WaitOne(timeout, false));                     // Event is thrown when the AutoResetEvent is set or when timeout has run out.
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        /// <summary>Gets the invitation created when connection was established.</summary>
        public string GetInvitation()
        {
            return invitation;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // GeoMagicTouch connection
        /// <summary>GeoMagicTouch connection event. This event is thrown when the connection to the GeoMagicTouch is established or the timeout has run out.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void GMTConnectionEventHandler(bool result);
        /// <summary>GeoMagicTouch connection event. This event is thrown when the connection to the GeoMagicTouch is established or the timeout has run out.</summary>
        public static event GMTConnectionEventHandler OnConnectionGMT;

        /// <summary>Initializes a new GeoMagicTouch and starts the Listener. Creates a waiting thread to wait for the connection to be established.</summary>
        public void EstablishConnectionGMT()
        {
            waitHandles[2] = new AutoResetEvent(false);
            gmt = new GeoMagicTouch();
            gmtThread = new Thread(() => gmt.GeoListener(waitHandles[2]));                      // Creates a new thread to connect to the GeoMagicTouch 
            gmtThread.IsBackground = true;
            gmtThread.Name = "gmtThread";
            gmtThread.Start();

            Thread waitThread = new Thread(() =>
            {
                if (OnConnectionGMT != null)
                    OnConnectionGMT(waitHandles[2].WaitOne(timeout));                           // Event is thrown when the AutoResetEvent is set or when timeout has run out.
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // VideoFeed connection
        /// <summary>VideoFeed connection event. This event is thrown when the connection to the VideoFeed server is established or the timeout has run out.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void VFConnectionEventHandler(bool result);
        /// <summary>VideoFeed connection event. This event is thrown when the connection to the VideoFeed server is established or the timeout has run out.</summary>
        public static event VFConnectionEventHandler OnConnectionVF;

        /// <summary>Initializes a new videofeed and calls for a confirmation from the server. Creates a waiting thread to wait for the server to send a confirmation.</summary>
        public void EstablishConnectionVF()
        {
            waitHandles[3] = new AutoResetEvent(false);

            videoThread = new Thread(() =>
            {
                videoFeed = new VideoFeed();
                bool result = videoFeed.ConfirmConnection(logic.selectedUnit.vfIpAddress);      // Calls for a confirmation from the server based on the selected unit collected from logic
                if (result == true)
                {
                    waitHandles[3].Set();
                }
            });
            videoThread.IsBackground = true;
            videoThread.Start();

            Thread waitThread = new Thread(() =>
            {
                if (OnConnectionVF != null)
                    OnConnectionVF(waitHandles[3].WaitOne(timeout, false));                     // Event is thrown when the AutoResetEvent is set or when timeout has run out.
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        /// <summary>Initializes the connection to the videofeed.</summary>
        /// <param name="vPlayer">Reference to the videoplayer in the Presentation frame to play the videofeed.</param>
        public void GetVideoFeed(ref IVideoPlayer vPlayer)
        {
            var options = new string[]                                                          // Create options for the videoplayer
            {
                "--network-caching=150",                                                        // Define value for network caching
            };

            vPlayer = new MediaPlayerFactory(options, true).CreatePlayer<IVideoPlayer>();       // creates a new videoplayer with the specific options

            videoFeed.Connect(logic.selectedUnit.vfIpAddress, ref vPlayer);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Modbus connection
        /// <summary>Establishes the connection to the modbus server.</summary>
        public void EstablishConnectionModbus()
        {
            modBus = new ModBus(logic.selectedUnit.urIpAddress);
        }

        /// <summary>Configuration completed event. This event is thrown when the configuration has been sent to the modbus server or the timeout has run out.</summary>
        /// <param name="result">Result of the process</param>
        public delegate void ConfigurationEventHandler(bool result);
        /// <summary>Configuration completed event. This event is thrown when the configuration has been sent to the modbus server or the timeout has run out.</summary>
        public static event ConfigurationEventHandler OnConfigCompleted;

        /// <summary>Sends the configurations to the modbus server to change the configurations of the robot.</summary>
        /// <param name="configData">Object containing the desired configurations.</param>
        public void SetConfigurations(ConfigurationData configData)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            Thread newConfigThread = new Thread(() =>
            {
                ConfigurationData temp = new ConfigurationData();
                while (!temp.Equals(configData))                                    // Sends configurations until the configurations on the robot is equal to the desired
                {
                    modBus.SendConfigurations(configData);                          // Sends the desired configuration
                    Thread.Sleep(10);                                               // Sleep thread to optain a frequency on 100Hz
                    temp = modBus.GetConfigurations();                              // Gets the current configurations on the robot
                }
                are.Set();
            });
            newConfigThread.IsBackground = true;
            newConfigThread.Start();

            Thread waitThread = new Thread(() =>
            {
                if (OnConfigCompleted != null)
                    OnConfigCompleted(are.WaitOne(timeout, false));                 // Event is thrown when the AutoResetEvent is set or when timeout has run out.
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        /// <summary>Current Configuration event. This event is thrown when the current configuration has been collected from the modbus server.</summary>
        /// <param name="conf">Current configuration data object</param>
        public delegate void CurrentConfigEventHandler(ConfigurationData conf, ForceData force);
        /// <summary>Current Configuration event. This event is thrown when the current configuration has been collected from the modbus server.</summary>
        public static event CurrentConfigEventHandler OnCurrentConfig;

        /// <summary>Gets the current configurations while the connection to the modbus server i alive.</summary>
        public void CheckConfigurations()
        {
            configThread = new Thread(() =>
            {
                ConfigurationData currentConfig = new ConfigurationData();
                while (true)
                {
                    currentConfig = modBus.GetConfigurations();
                    ForceData force = modBus.ReadURForce();
                    if (OnCurrentConfig != null)
                        OnCurrentConfig(currentConfig, force);
                    Thread.Sleep(10);                                               // Sleep thread to optain a frequency on 100Hz
                }
            });
            configThread.IsBackground = true;
            configThread.Name = "configThread";
            configThread.Start();
        }
    }
}
