using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using RoboLibrary.Dummies;
using System.Threading;
using Util;
using DataStructures;
namespace RoboLibrary
{
    public class Data : IData
    {
        private DataStructures.URPose LastKnownPose;
        public ConfigurationData CurrentConfiguration;
        public IWriter writer;
        public IReader reader;
        public static string IP_ADDRESS = "192.168.0.103";
        private const float INITIAL_SPEED = 0.075f;
        private const float INITIAL_ACCELERATION = 0.65f;
        public float Speed { get; private set; }
        public float Acceleration { get; private set; }

        public delegate void ConfigurationEventHandler(bool result);
        //public static event ConfigurationEventHandler OnConfigCompleted;

        public delegate void CurrentURPoseReceived(DataStructures.URPose pose);
        public static event CurrentURPoseReceived OnCurrentURPoseReceived;

        public delegate void CurrentConfigurationReceived(ConfigurationData config);
        public static event CurrentConfigurationReceived OnCurrentConfigurationReceived;

        public void Init(IReader reader, IWriter writer)
        {
            this.reader = reader;
            Thread readerThread = new Thread(() => this.reader.StartListen(IP_ADDRESS, new AutoResetEvent(false)));
            readerThread.IsBackground = true;
            readerThread.SetApartmentState(ApartmentState.STA);
            readerThread.Start();
            this.writer = writer;

            Speed = INITIAL_SPEED;
            Acceleration = INITIAL_ACCELERATION;
            SetUpVelocity();
        }

        public void SlowDown()
        {
            ChangeVelocity(INITIAL_SPEED/1f, INITIAL_ACCELERATION/1f);
        }

        public void SpeedUp()
        {
            ChangeVelocity(INITIAL_SPEED, INITIAL_ACCELERATION);
        }

        public void ChangeVelocity(float speed, float acceleration)
        {
            Speed = speed;
            Acceleration = acceleration;
            SetUpVelocity();
        }

        private void SetUpVelocity()
        {
            ConfigurationData d = new ConfigurationData();
            d.Speed = Speed;
            d.Acceleration = Acceleration;
            SendConfiguration(d);
        }

        public void FetchCurrentConfiguration()
        {
            writer.GetConfigurations();
        }

        public void SendConfiguration(ConfigurationData config)
        {
            writer.SendConfigurations(config);
        }

        public void ReceiveConfiguration(ConfigurationData config)
        {
            CurrentConfiguration = config;
            OnCurrentConfigurationReceived?.Invoke(config);
        }

        public void SetLastKnownPose(DataStructures.URPose pose)
        {
            LastKnownPose = pose;
            OnCurrentURPoseReceived?.Invoke(pose);
        }

        /*
        /// <summary>
        /// Add pose to the current pose, mainly used for moving it just slightly
        /// </summary>
        /// <param name="pose">Pose to be added to the current UR10 pose</param>
        public void SendAddedPose(DataStructures.URPose pose)
        {
            writer.AddURPose(pose);
        }*/

        /// <summary>
        /// Force it to pose an entirely new way
        /// </summary>
        /// <param name="pose"></param>
        public void SendNewPose(DataStructures.URPose pose)
        {
            writer.SendURPose(pose);
        }

        public DataStructures.URPose GetLastKnownPose()
        {
            return LastKnownPose;
        }
    }
}
