using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UR10TCPController.Interfaces;
using UR10TCPController.Dummies;
using System.Threading;
using Util;

namespace UR10TCPController
{
    public class Data
    {
        private static Data _instance;
        public URPose LastKnownPose;
        public ConfigurationData CurrentConfiguration;
        public IWriter _writer;
        public IReader _reader;
        private static string _ip_address = "192.168.0.103";

        public delegate void ConfigurationEventHandler(bool result);
        public static event ConfigurationEventHandler OnConfigCompleted;

        public delegate void CurrentURPoseReceived(URPose pose);
        public static event CurrentURPoseReceived OnCurrentURPoseReceived;

        public delegate void CurrentConfigurationReceived(ConfigurationData config);
        public static event CurrentConfigurationReceived OnCurrentConfigurationReceived;

        private Data()
        {
            _reader = new Reader();
            Thread readerThread = new Thread(() => _reader.StartListen(_ip_address, new AutoResetEvent(false)));
            readerThread.IsBackground = true;
            readerThread.SetApartmentState(ApartmentState.STA);
            readerThread.Start();

            _writer = new Writer(_ip_address);
        }

        public void FetchCurrentConfiguration()
        {
            _writer.GetConfigurations();
        }

        public void SendConfiguration(ConfigurationData config)
        {
            _writer.SendConfigurations(config);
        }

        public void ReceiveConfiguration(ConfigurationData config)
        {
            CurrentConfiguration = config;
            OnCurrentConfigurationReceived(config);
        }

        public void ReceivePose(URPose pose)
        {
            LastKnownPose = pose;
            OnCurrentURPoseReceived(pose);
        }

        /// <summary>
        /// Add pose to the current pose, mainly used for moving it just slightly
        /// </summary>
        /// <param name="pose">Pose to be added to the current UR10 pose</param>
        public void SendAddedPose(URPose pose)
        {
            _writer.AddURPose(pose);
        }

        /// <summary>
        /// Force it to pose an entirely new way
        /// </summary>
        /// <param name="pose"></param>
        public void SendNewPose(URPose pose)
        {
            _writer.SendURPose(pose);
        }

        public static Data Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Data();
                }
                return _instance;
            }
        }
    }
}
