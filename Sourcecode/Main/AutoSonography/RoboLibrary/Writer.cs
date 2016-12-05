using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoboLibrary.Interfaces;
using Util;
using DataStructures;
namespace RoboLibrary
{
    [ExcludeFromCodeCoverage]
    public class Writer : IWriter
    {
        private ModBus _modbus;
        private static ushort _port = 502;
        private string _ip_address;
        private static int _timeout = 25000; //ms
        private IData dataInstance;

        public Writer(IData data, string ip_address)
        {
            dataInstance = data;
            _ip_address = ip_address;
            try
            {
                _modbus = new ModBus(_ip_address, _port);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("ModBus could not be initialized: " + exc.Message);
                if (_modbus != null)
                    _modbus.Disconnect();
            }
        }

        public void SendURPose(DataStructures.URPose pose)
        {
            byte[] data = new byte[12];
            ushort ID = 2;
            byte unit = 0;
            ushort StartAddress = 135;
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.Xpose * 1000))), 0, data, 0, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.Ypose * 1000))), 0, data, 2, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.Zpose * 1000))), 0, data, 4, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.RXpose * 1000))), 0, data, 6, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.RYpose * 1000))), 0, data, 8, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(pose.RZpose * 1000))), 0, data, 10, 2);

            byte[] result = new byte[100];
            if (_modbus != null)
                _modbus.WriteMultipleRegister(ID, unit, StartAddress, data, ref result);
        }

        public ConfigurationData GetConfigurations()
        {
            ConfigurationData conf = new ConfigurationData();
            if (_modbus != null)
            {
                ushort ID = 10;
                byte unit = 0;
                ushort StartAddress = 144; // StartAddress in the general purpose register
                byte Length = 10; // Magnitude of the desired data array. A representation of the number of registers to be read.

                byte[] response = new byte[20];

                _modbus.ReadInputRegister(ID, unit, StartAddress, Length, ref response);

                if (response != null && response.Length == 20)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(response);
                        conf.OffsetX = Convert.ToDouble(BitConverter.ToInt16(response, 18)) / 1000;
                        conf.OffsetY = Convert.ToDouble(BitConverter.ToInt16(response, 16)) / 1000;
                        conf.OffsetZ = Convert.ToDouble(BitConverter.ToInt16(response, 14)) / 1000;
                        conf.OffsetRx = Convert.ToDouble(BitConverter.ToInt16(response, 12)) / 1000;
                        conf.OffsetRy = Convert.ToDouble(BitConverter.ToInt16(response, 10)) / 1000;
                        conf.OffsetRz = Convert.ToDouble(BitConverter.ToInt16(response, 8)) / 1000;
                        conf.Payload = Convert.ToDouble(BitConverter.ToInt16(response, 6)) / 1000;
                        conf.Acceleration = Convert.ToDouble(BitConverter.ToInt16(response, 4)) / 1000;
                        conf.Speed = Convert.ToDouble(BitConverter.ToInt16(response, 2)) / 1000;
                        conf.Blend = Convert.ToDouble(BitConverter.ToInt16(response, 0)) / 1000;
                    }
                    else
                    {
                        conf.OffsetX = Convert.ToDouble(BitConverter.ToInt16(response, 0)) / 1000;
                        conf.OffsetY = Convert.ToDouble(BitConverter.ToInt16(response, 2)) / 1000;
                        conf.OffsetZ = Convert.ToDouble(BitConverter.ToInt16(response, 4)) / 1000;
                        conf.OffsetRx = Convert.ToDouble(BitConverter.ToInt16(response, 6)) / 1000;
                        conf.OffsetRy = Convert.ToDouble(BitConverter.ToInt16(response, 8)) / 1000;
                        conf.OffsetRz = Convert.ToDouble(BitConverter.ToInt16(response, 10)) / 1000;
                        conf.Payload = Convert.ToDouble(BitConverter.ToInt16(response, 12)) / 1000;
                        conf.Acceleration = Convert.ToDouble(BitConverter.ToInt16(response, 14)) / 1000;
                        conf.Speed = Convert.ToDouble(BitConverter.ToInt16(response, 16)) / 1000;
                        conf.Blend = Convert.ToDouble(BitConverter.ToInt16(response, 18)) / 1000;
                    }
                }
            }
            dataInstance.ReceiveConfiguration(conf);
            return conf;
        }

        private void WriteConfigurationBytes(ConfigurationData confData)
        {
            byte[] data = new byte[22];

            ushort ID = 6;
            byte unit = 0;
            ushort StartAddress = 143; // StartAddress in the general purpose register

            // Generates a byte array with data in the specific order based on the documentation, to be send to the modbus register
            // Every value is multiplied by 1000 to secure data precision and then converted into 16-bit signed integer
            // Byte is reversed to NetwotkOrder and then converted to a word
            // The first word in the data array is a 1 to signal the robots program that a new configuration has been send
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(1))), 0, data, 0, 2); //143, 144
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetX * 1000))), 0, data, 2, 2); //
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetY * 1000))), 0, data, 4, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetZ * 1000))), 0, data, 6, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetRx * 1000))), 0, data, 8, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetRy * 1000))), 0, data, 10, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.OffsetRz * 1000))), 0, data, 12, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.Payload * 1000))), 0, data, 14, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.Acceleration * 1000))), 0, data, 16, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.Speed * 1000))), 0, data, 18, 2);
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(confData.Blend * 1000))), 0, data, 20, 2);

            byte[] result = new byte[100];
            if (_modbus != null)
                _modbus.WriteMultipleRegister(ID, unit, StartAddress, data, ref result);
        }

        public void SendConfigurations(ConfigurationData confData)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            Thread newConfigThread = new Thread(() =>
            {
                int threadNumber = DateTime.Now.Second;
                ConfigurationData temp = new ConfigurationData();
                while (!AreSame(temp, confData)) // Sends configurations until the configurations on the robot is equal to the desired
                {
                    WriteConfigurationBytes(confData);
                    Thread.Sleep(10); // 10+ hZ frequency
                    temp = GetConfigurations();
                    Debug.WriteLine(threadNumber + " - Not equal");
                }
                are.Set();
            });
            newConfigThread.IsBackground = true;
            newConfigThread.Start();

            Thread waitThread = new Thread(() =>
            {
                bool configSent = are.WaitOne(_timeout, false);
                if(configSent)
                    dataInstance.ReceiveConfiguration(confData);
            });
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        private bool AreSame(ConfigurationData a, ConfigurationData b)
        {
            float threshold = 0.00001f;
            if (Math.Abs(a.Speed - b.Speed) > threshold) return false;
            if (Math.Abs(a.Acceleration - b.Acceleration) > threshold) return false;
            return true;
        }

    }
}
